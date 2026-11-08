using Identity.Interfaces.Credentials;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Credentials
{
    public sealed class RsaSigningKeyRotationService(
        IOptions<JwtOptions> options,
        ILogger<RsaSigningKeyRotationService> logger) : BackgroundService, IRsaSigningKeyProvider, IDisposable
    {
        private readonly JwtOptions _options = options.Value;
        private readonly ILogger<RsaSigningKeyRotationService> _logger = logger;
        private readonly Lock _sync = new();
        private readonly List<RsaSigningKey> _keys = [];

        public RsaSigningKey GetCurrentSigningKey()
        {
            lock (_sync)
            {
                RotateIfRequired(DateTimeOffset.UtcNow);
                return _keys[^1];
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
            {
                lock (_sync)
                {
                    RotateIfRequired(DateTimeOffset.UtcNow);
                }
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
            _logger.LogInformation("Generated JWT signing key {KeyId}; signing ends at {SigningEndsAt} UTC", key.KeyId, key.SigningEndsAt);

            foreach (var expiredKey in _keys.Where(existing => existing != key && existing.VerificationEndsAt <= now).ToArray())
            {
                _keys.Remove(expiredKey);
                expiredKey.Dispose();
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

        public override void Dispose()
        {
            lock (_sync)
            {
                foreach (var key in _keys)
                {
                    key.Dispose();
                }
                _keys.Clear();
            }

            base.Dispose();
        }
    }
}
