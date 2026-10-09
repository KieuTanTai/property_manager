using Microsoft.EntityFrameworkCore;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Infrastructure.Repository.TicketRepository
{
    public class TicketMediaRepository(
        TicketAndNotificationDbContext context,
        ILogger<TicketMediaRepository> logger,
        ILogPool logPool) : ITicketMediaRepository
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Infrastructure/Repository/TicketRepository";

        private readonly TicketAndNotificationDbContext _db = context;
        private readonly ILogger<TicketMediaRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region POST

        public async Task AddAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket media creation.");
            if (ticketMedia.TicketMediaTicketId == Guid.Empty
                || string.IsNullOrWhiteSpace(ticketMedia.TicketMediaImageUrl))
            {
                var exception = new ArgumentException(
                    "TicketMediaModel ticket id and image URL are required.",
                    nameof(ticketMedia));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket media creation rejected because a required value is missing.");
                throw exception;
            }

            await _db.TicketMedias.AddAsync(ticketMedia, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket media range creation.");
            var ticketMediaModels = ticketMedias.ToList();
            await _db.TicketMedias.AddRangeAsync(ticketMediaModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media range staged for creation.");
        }

        public async Task UpdateAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket media update.");
            if (ticketMedia.TicketMediaId <= 0)
            {
                var exception = new ArgumentException(
                    "TicketMediaModel id is required.",
                    nameof(ticketMedia.TicketMediaId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket media update rejected because the id is missing.");
                throw exception;
            }

            var existingTicketMedia = await _db.TicketMedias.AsNoTracking()
                .FirstOrDefaultAsync(
                    existing => existing.TicketMediaId == ticketMedia.TicketMediaId,
                    cancellationToken);
            if (existingTicketMedia is null)
            {
                var exception = new InvalidOperationException(
                    $"TicketMediaModel not found! {ticketMedia.TicketMediaId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket media update rejected because it was not found.");
                throw exception;
            }

            _db.TicketMedias.Update(ticketMedia);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media staged for update.");
        }

        public void UpdateRange(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket media range update.");
            var ticketMediaModels = ticketMedias.ToList();
            _db.TicketMedias.UpdateRange(ticketMediaModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media range staged for update.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<TicketMediaModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all ticket media.");
            return await _db.TicketMedias.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<TicketMediaModel?> GetByIdAsync(
            int ticketMediaId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading ticket media by id.");
            return await _db.TicketMedias.AsNoTracking()
                .FirstOrDefaultAsync(
                    ticketMedia => ticketMedia.TicketMediaId == ticketMediaId,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<TicketMediaModel>> GetByIdsAsync(
            IEnumerable<int> ticketMediaIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading ticket media by ids.");
            return await _db.TicketMedias.AsNoTracking()
                .Where(ticketMedia => ticketMediaIds.Contains(ticketMedia.TicketMediaId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TicketMediaModel>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Loading ticket media by ticket id.");
            return await _db.TicketMedias.AsNoTracking()
                .Where(ticketMedia => ticketMedia.TicketMediaTicketId == ticketId)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}
