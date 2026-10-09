using Shared.Enum;
using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Application
{
    public class TicketApplication(
        IUnitOfWork unitOfWork,
        ITicketRepository ticketRepository,
        ILogger<TicketApplication> logger,
        ILogPool logPool) : ITicketApplication
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Application";

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITicketRepository _ticketRepository = ticketRepository;
        private readonly ILogger<TicketApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        public Task<IReadOnlyList<TicketModel>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetAllAsync(cancellationToken);

        public Task<TicketModel?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetByIdAsync(ticketId, cancellationToken);

        public Task<IReadOnlyList<TicketModel>> GetByIdsAsync(
            IEnumerable<Guid> ticketIds,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetByIdsAsync(ticketIds, cancellationToken);

        public Task<IReadOnlyList<TicketModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetByAccountIdAsync(accountId, cancellationToken);

        public Task<IReadOnlyList<TicketModel>> GetByTypeAsync(
            ETicketType type,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetByTypeAsync(type, cancellationToken);

        public Task<IReadOnlyList<TicketModel>> GetByIsResolvedAsync(
            bool isResolved,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetByIsResolvedAsync(isResolved, cancellationToken);

        public Task<string?> GetContentByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default) =>
            _ticketRepository.GetContentByIdAsync(ticketId, cancellationToken);

        #endregion

        #region POST

        public async Task<TicketModel> AddAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket creation operation started.");
            await _ticketRepository.AddAsync(ticket, cancellationToken);
            await SaveChangesOrThrowAsync("create ticket", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket creation completed.");
            return ticket;
        }

        public async Task<int> AddRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default)
        {
            var ticketModels = RequireItems(tickets, nameof(tickets));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket range creation operation started.");
            await _ticketRepository.AddRangeAsync(ticketModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "create tickets", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket range creation completed.");
            return affectedRows;
        }

        #endregion

        #region UPDATE

        public async Task<TicketModel> UpdateAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket update operation started.");
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            await SaveChangesOrThrowAsync("update ticket", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket update completed.");
            return ticket;
        }

        public async Task<int> UpdateRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default)
        {
            var ticketModels = RequireItems(tickets, nameof(tickets));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket range update operation started.");
            _ticketRepository.UpdateRange(ticketModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "update tickets", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket range update completed.");
            return affectedRows;
        }

        #endregion

        private async Task<int> SaveChangesOrThrowAsync(
            string operation,
            CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                var exception = new InvalidOperationException(
                    $"Ticket persistence returned zero affected rows while attempting to {operation}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    $"Ticket operation failed to {operation}.");
                throw exception;
            }

            return affectedRows;
        }

        private List<TicketModel> RequireItems(
            IEnumerable<TicketModel> tickets,
            string parameterName)
        {
            var ticketModels = tickets.ToList();
            if (ticketModels.Count == 0)
            {
                var exception = new ArgumentException(
                    "Ticket collection cannot be empty.", parameterName);
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Ticket batch operation rejected because the collection is empty.");
                throw exception;
            }

            return ticketModels;
        }
    }
}
