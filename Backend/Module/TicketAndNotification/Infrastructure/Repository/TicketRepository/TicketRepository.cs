using Microsoft.EntityFrameworkCore;
using Shared.Enum;
using Shared.Logging;
using TicketAndNotification.Infrastructure.Persistence.DbContext;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Infrastructure.Repository.TicketRepository
{
    public class TicketRepository(
        TicketAndNotificationDbContext context,
        ILogger<TicketRepository> logger,
        ILogPool logPool
    ): ITicketRepository
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Infrastructure/Repository/TicketRepository";
        private readonly TicketAndNotificationDbContext _db = context;
        private ILogger<TicketRepository> _logger = logger;
        private ILogPool _logPool = logPool;

        #region POST

        public async Task AddAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket creation.");
            if (ticket.TicketAccountId == Guid.Empty
                || string.IsNullOrWhiteSpace(ticket.TicketContent))
            {
                var exception = new ArgumentException(
                    "TicketModel account id and content are required.", nameof(ticket));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket creation rejected because a required value is missing.");
                throw exception;
            }

            await _db.Tickets.AddAsync(ticket, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket staged for creation.");
        }

        public async Task AddRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket range creation.");
            var ticketModels = tickets.ToList();
            await _db.Tickets.AddRangeAsync(ticketModels, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket range staged for creation.");
        }

        public async Task UpdateAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket update.");
            if (ticket.TicketId == Guid.Empty)
            {
                var exception = new ArgumentException(
                    "TicketModel id is required.", nameof(ticket.TicketId));
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket update rejected because the id is missing.");
                throw exception;
            }

            var existingTicket = await _db.Tickets.AsNoTracking()
                .FirstOrDefaultAsync(
                    existing => existing.TicketId == ticket.TicketId,
                    cancellationToken);
            if (existingTicket is null)
            {
                var exception = new InvalidOperationException(
                    $"TicketModel not found! {ticket.TicketId}");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    "Ticket update rejected because the ticket was not found.");
                throw exception;
            }

            _db.Tickets.Update(ticket);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket staged for update.");
        }

        public void UpdateRange(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Preparing ticket range update.");
            var ticketModels = tickets.ToList();
            _db.Tickets.UpdateRange(ticketModels);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket range staged for update.");
        }

        #endregion

        #region GET

        public async Task<IReadOnlyList<TicketModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all tickets.");
            return await _db.Tickets.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<TicketModel?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading ticket by id.");
            return await _db.Tickets.AsNoTracking()
                .FirstOrDefaultAsync(ticket => ticket.TicketId == ticketId, cancellationToken);
        }

        public async Task<IReadOnlyList<TicketModel>> GetByIdsAsync(
            IEnumerable<Guid> ticketIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tickets by ids.");
            return await _db.Tickets.AsNoTracking()
                .Where(ticket => ticketIds.Contains(ticket.TicketId))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TicketModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tickets by account id.");
            return await _db.Tickets.AsNoTracking()
                .Where(ticket => ticket.TicketAccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TicketModel>> GetByTypeAsync(
            ETicketType type,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tickets by type.");
            return await _db.Tickets.AsNoTracking()
                .Where(ticket => ticket.TicketType == type)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TicketModel>> GetByIsResolvedAsync(
            bool isResolved,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading tickets by resolved status.");
            return await _db.Tickets.AsNoTracking()
                .Where(ticket => ticket.TicketIsResolved == isResolved)
                .ToListAsync(cancellationToken);
        }

        public async Task<string?> GetContentByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading ticket content by id.");
            return await _db.Tickets.AsNoTracking()
                .Where(ticket => ticket.TicketId == ticketId)
                .Select(ticket => ticket.TicketContent)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion
    }
}