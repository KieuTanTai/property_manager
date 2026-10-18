using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Contract;
using Contract.Models.Regulation;
using Contract.Utils.Enum;
using Shared.Interfaces;
using Shared.Logging;
using Shared.Persistence.Record;

namespace Contract.Application
{
    public class ContractApplication(
        IUnitOfWork unitOfWork,
        IContractRepository contractRepository,
        IContractRegulationRepository contractRegulationRepository,
        IRegulationRepository regulationRepository,
        ILogger<ContractApplication> logger,
        ILogPool logPool)
        : IContractApplication
    {
        private const string Module = "Contract";

        private const string Layer = "Application";

        private readonly IContractRegulationRepository _contractRegulationRepository = contractRegulationRepository;

        private readonly IContractRepository _contractRepository = contractRepository;

        private readonly ILogPool _logPool = logPool;

        private readonly ILogger<ContractApplication> _logger = logger;

        private readonly IRegulationRepository _regulationRepository = regulationRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        #region CREATE

        public async Task<ContractModel> AddContractAsync(ContractModel contract,
            IReadOnlyCollection<Guid>? regulationIds = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidContractModel(contract);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Creating contract.");

            var regulations = await ResolveRegulationsAsync(regulationIds, true, cancellationToken);
            await _contractRepository.AddAsync(contract, cancellationToken);

            if (regulations.Count > 0)
            {
                await _contractRegulationRepository.AddRangeAsync(
                [
                    .. regulations.Select(regulation => new ContractRegulationModel(regulation.RegulationId,
                        contract.ContractId))
                ], cancellationToken);
            }

            await SaveChangesOrThrowAsync("create", cancellationToken);
            contract.SetContractRegulations(regulations);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Contract created.");
            return contract;
        }

        #endregion

        #region UPDATE

        public async Task<ContractModel> UpdateContractAsync(ContractModel contract,
            IReadOnlyCollection<Guid>? regulationIds = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidContractModel(contract);
            if (contract.ContractId == Guid.Empty)
            {
                throw new ArgumentException("Contract id is required.", nameof(contract));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating contract.");
            var existing = await _contractRepository.GetTrackedByIdAsync(contract.ContractId, cancellationToken);
            if (existing is null)
            {
                throw new ArgumentException("Contract not found.", nameof(contract));
            }

            // Null means leave associations unchanged; an empty collection removes all regulations.
            IReadOnlyList<RegulationModel>? regulations = null;
            if (regulationIds is not null)
            {
                regulations = await ResolveRegulationsAsync(regulationIds, false, cancellationToken);
                await _contractRegulationRepository.DeleteByFirstForeignIdAsync(contract.ContractId, cancellationToken);
                if (regulations.Count > 0)
                {
                    await _contractRegulationRepository.AddRangeAsync(
                    [
                        .. regulations.Select(regulation => new ContractRegulationModel(regulation.RegulationId,
                            contract.ContractId))
                    ], cancellationToken);
                }
            }

            await _contractRepository.UpdateAsync(contract, cancellationToken);
            await SaveChangesOrThrowAsync("update", cancellationToken);

            if (regulations is not null)
            {
                contract.SetContractRegulations(regulations);
            }
            _logger.LogLayerInformation(_logPool, Module, Layer, "Contract updated.");
            return contract;
        }

        #endregion

        #region TERMINATED

        public async Task<int> TerminateContractAsync(Guid contractId, DateTime terminationDate,
            CancellationToken cancellationToken = default)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract id is required.", nameof(contractId));
            }
            if (terminationDate < DateTime.UtcNow)
            {
                throw new ArgumentException("Termination date cannot be in the past.", nameof(terminationDate));
            }
            _logger.LogLayerDebug(_logPool, Module, Layer, "Terminating contract.");
            await _contractRepository.SoftDeleteAsync(contractId, terminationDate, cancellationToken);
            return await SaveChangesOrThrowAsync("terminate", cancellationToken);
        }

        #endregion

        #region GET

        public async Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingAsync(Guid? cursor, int pageSize,
            bool isGetViolations = false, bool isGetInvoices = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract page with navigation properties.");
            var result = await _contractRepository.GetApplyPagingAsync(cursor, pageSize,
                isGetViolations, isGetInvoices, true,
                cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, $"Contract page loaded: {result.Items.Count} records.");
            return result;
        }

        public async Task<RecordBaseCursorPage<ContractModel>> GetApplyPagingByContractStatusAsync(Guid? cursor,
            int pageSize, EContractStatus status, bool isGetViolations = false, bool isGetInvoices = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract page by status.");
            var result = await _contractRepository.GetApplyPagingByStatusAsync(cursor, pageSize,
                isGetViolations, isGetInvoices, true, status,
                cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, $"Contract page by status loaded: {result.Items.Count} records.");
            return result;
        }

        public async Task<ContractModel?> GetByContractIdAsync(Guid contractId, bool isGetViolations = false,
            bool isGetInvoices = false, CancellationToken cancellationToken = default)
        {
            if (contractId == Guid.Empty)
            {
                throw new ArgumentException("Contract id is required.", nameof(contractId));
            }

            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading contract by id with navigation properties.");
            var result = await _contractRepository.GetContractAndNavigationByIdAsync(contractId,
                isGetViolations, isGetInvoices, true, cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, $"Contract by id loaded: {result?.ContractId}.");
            return result;
        }

        #endregion

        #region PRIVATE

        private async Task<IReadOnlyList<RegulationModel>> ResolveRegulationsAsync(
            IReadOnlyCollection<Guid>? regulationIds, bool useAllWhenEmpty, CancellationToken cancellationToken)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer,
                "Resolving regulations for contract operation.");
            var allRegulations = regulationIds is null || useAllWhenEmpty && regulationIds.Count == 0;
            var regulations = allRegulations
                ? await _regulationRepository.GetAllAsync(cancellationToken)
                : await _regulationRepository.GetByIdsAsync(regulationIds!, cancellationToken);

            if (allRegulations)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer,
                    $"Regulations resolved: {regulations.Count} records.");
                return regulations;
            }
            var requestedIds = regulationIds!.Distinct().ToHashSet();
            if (regulations.Count != requestedIds.Count)
            {
                throw new ArgumentException("One or more regulation ids were not found.", nameof(regulationIds));
            }
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Requested regulations resolved: {regulations.Count} records.");
            return regulations;
        }

        private async Task<int> SaveChangesOrThrowAsync(string operation, CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (affectedRows > 0)
            {
                _logger.LogLayerInformation(_logPool, Module, Layer, $"Contract {operation} successful. Affected rows: {affectedRows}.");
                return affectedRows;
            }

            var exception = new InvalidOperationException($"Failed to {operation} contract.");
            _logger.LogLayerError(_logPool, Module, Layer, exception, $"Contract {operation} failed.");
            throw exception;
        }

        private static void ThrowIfInvalidContractModel(ContractModel contract)
        {
            ArgumentNullException.ThrowIfNull(contract);
            if (contract.ContractAccountId == Guid.Empty)
            {
                throw new ArgumentException("Contract account id is required.", nameof(contract));
            }

            if (contract.ContractDeposit <= 0)
            {
                throw new ArgumentException("Contract deposit must be non-negative and larger than 0.", nameof(contract));
            }

            if (contract.ContractRentalPrice <= 0)
            {
                throw new ArgumentException("Contract rental price must be non-negative and larger than 0.", nameof(contract));
            }

            if (contract.ContractPremiseReturnDate <= contract.ContractCreatedAt)
            {
                throw new ArgumentException("Contract premise return date must be after the contract creation date.", nameof(contract));
            }
        }

        #endregion
    }
}