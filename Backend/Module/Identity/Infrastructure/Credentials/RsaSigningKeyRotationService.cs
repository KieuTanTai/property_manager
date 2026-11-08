using Identity.Infrastructure.Credentials.Models;
using Identity.Interfaces.Credentials;
using Microsoft.Extensions.Options;
using Shared.Logging;

namespace Identity.Infrastructure.Credentials
{
    public sealed class RsaSigningKeyRotationService(
        IOptions<JwtOptions> options,
        ILogger<RsaSigningKeyRotationService> logger,
        ILogPool logPool)
        : BackgroundService, IRsaSigningKeyProvider, IDisposable
    {
        private const string Module = "Identity";

        private const string Layer = "Infrastructure/Credentials";

        private readonly List<RsaSigningKey> _keys = [];

        private readonly JwtOptions _options = options.Value;

        private readonly Lock _sync = new();

        public override void Dispose()
        {
            lock (_sync)
            {
                foreach (var key in _keys)
                    key.Dispose();
                _keys.Clear();
            }

            base.Dispose();
        }

        public RsaSigningKey GetCurrentSigningKey()
        {
            lock (_sync)
            {
                RotateIfRequired(DateTimeOffset.UtcNow);
                return _keys[^1];
            }
        }

        public string? GetPublicKeyPem(string keyId)
        {
            lock (_sync)
            {
                var now = DateTimeOffset.UtcNow;
                RotateIfRequired(now);
                var key = _keys.FirstOrDefault(existing =>
                    existing.KeyId.Equals(keyId, StringComparison.Ordinal) && existing.VerificationEndsAt > now);
                return key?.Rsa.ExportSubjectPublicKeyInfoPem();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            lock (_sync)
            {
                RotateIfRequired(DateTimeOffset.UtcNow);
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
                lock (_sync)
                {
                    RotateIfRequired(DateTimeOffset.UtcNow);
                }
        }

        private void RotateIfRequired(DateTimeOffset now)
        {
            if (_keys.Count > 0 && now < _keys[^1].SigningEndsAt)
            {
                return;
            }

            var key = new RsaSigningKey(
                now,
                TimeSpan.FromHours(_options.KeyRotationHours),
                TimeSpan.FromHours(_options.ExpirationHours),
                TimeSpan.FromHours(_options.RetiredKeyGraceHours));
            _keys.Add(key);
            logger.LogLayerInformation(logPool, Module, Layer,
                $"Generated JWT signing key {key.KeyId}; signing ends at {key.SigningEndsAt:O}.");

            var expiredKeys = _keys.Where(existing => existing != key && existing.VerificationEndsAt <= now).ToList();
            foreach (var expiredKey in expiredKeys)
            {
                _keys.Remove(expiredKey);
                expiredKey.Dispose();
            }
        }
    }
}
