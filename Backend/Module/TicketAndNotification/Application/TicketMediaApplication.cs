using Shared.Interfaces;
using Shared.Logging;
using TicketAndNotification.Interfaces.Application;
using TicketAndNotification.Interfaces.Repository;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Application
{
    public class TicketMediaApplication(
        IUnitOfWork unitOfWork,
        ITicketMediaRepository ticketMediaRepository,
        ILogger<TicketMediaApplication> logger,
        ILogPool logPool) : ITicketMediaApplication
    {
        private const string Module = "TicketAndNotification";
        private const string Layer = "Application";

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ITicketMediaRepository _ticketMediaRepository = ticketMediaRepository;
        private readonly ILogger<TicketMediaApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        #region GET

        public Task<IReadOnlyList<TicketMediaModel>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            _ticketMediaRepository.GetAllAsync(cancellationToken);

        public Task<TicketMediaModel?> GetByIdAsync(
            int ticketMediaId,
            CancellationToken cancellationToken = default) =>
            _ticketMediaRepository.GetByIdAsync(ticketMediaId, cancellationToken);

        public Task<IReadOnlyList<TicketMediaModel>> GetByIdsAsync(
            IEnumerable<int> ticketMediaIds,
            CancellationToken cancellationToken = default) =>
            _ticketMediaRepository.GetByIdsAsync(ticketMediaIds, cancellationToken);

        public Task<IReadOnlyList<TicketMediaModel>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default) =>
            _ticketMediaRepository.GetByTicketIdAsync(ticketId, cancellationToken);

        #endregion

        #region POST

        public async Task<TicketMediaModel> AddAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket media creation operation started.");
            await _ticketMediaRepository.AddAsync(ticketMedia, cancellationToken);
            await SaveChangesOrThrowAsync("create ticket media", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media creation completed.");
            return ticketMedia;
        }

        public async Task<int> AddRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default)
        {
            var ticketMediaModels = RequireItems(ticketMedias, nameof(ticketMedias));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket media range creation operation started.");
            await _ticketMediaRepository.AddRangeAsync(ticketMediaModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "create ticket media", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media range creation completed.");
            return affectedRows;
        }

        #endregion

        #region UPDATE

        public async Task<TicketMediaModel> UpdateAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket media update operation started.");
            await _ticketMediaRepository.UpdateAsync(ticketMedia, cancellationToken);
            await SaveChangesOrThrowAsync("update ticket media", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media update completed.");
            return ticketMedia;
        }

        public async Task<int> UpdateRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default)
        {
            var ticketMediaModels = RequireItems(ticketMedias, nameof(ticketMedias));
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Ticket media range update operation started.");
            _ticketMediaRepository.UpdateRange(ticketMediaModels, cancellationToken);
            var affectedRows = await SaveChangesOrThrowAsync(
                "update ticket media", cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                "Ticket media range update completed.");
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
                    $"Ticket media persistence returned zero affected rows while attempting to {operation}.");
                _logger.LogLayerError(_logPool, Module, Layer, exception,
                    $"Ticket media operation failed to {operation}.");
                throw exception;
            }

            return affectedRows;
        }

        private List<TicketMediaModel> RequireItems(
            IEnumerable<TicketMediaModel> ticketMedias,
            string parameterName)
        {
            var ticketMediaModels = ticketMedias.ToList();
            if (ticketMediaModels.Count == 0)
            {
                var exception = new ArgumentException(
                    "Ticket media collection cannot be empty.", parameterName);
                _logger.LogLayerWarning(_logPool, Module, Layer,
                    "Ticket media batch operation rejected because the collection is empty.");
                throw exception;
            }

            return ticketMediaModels;
        }
    }
}
