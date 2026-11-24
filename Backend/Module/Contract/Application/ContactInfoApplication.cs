using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Contact;
using Shared.Interfaces;
using Shared.Logging;
using Microsoft.Extensions.Logging;

namespace Contract.Application
{
    public class ContactInfoApplication(
        IUnitOfWork unitOfWork,
        IContactInfoRepository contactInfoRepository,
        ILogger<ContactInfoApplication> logger,
        ILogPool logPool) : IContactInfoApplication
    {
        private const string Module = "Contract";
        private const string Layer = "Application";
        private readonly IContactInfoRepository _contactInfoRepository = contactInfoRepository;

        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<ContactInfoApplication> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public Task<IReadOnlyList<ContactInfoModel>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Loading all contact information.");
            return _contactInfoRepository.GetAllAsync(cancellationToken);
        }


        public async Task<ContactInfoModel> AddAsync(ContactInfoModel contactInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contactInfo);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Adding contact information.");
            await _contactInfoRepository.AddAsync(contactInfo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer, "Contact information added.");
            return contactInfo;
        }

        public async Task<int> UpdateAsync(ContactInfoModel contactInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contactInfo);
            _logger.LogLayerDebug(_logPool, Module, Layer, "Updating contact information.");
            await _contactInfoRepository.UpdateAsync(contactInfo, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contact information updated. Affected rows: {affectedRows}.");
            return affectedRows;
        }

        public async Task<int> DeleteAsync(Guid contactInfoId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogLayerDebug(_logPool, Module, Layer, "Deleting contact information.");
            await _contactInfoRepository.DeleteAsync(contactInfoId, cancellationToken);
            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogLayerInformation(_logPool, Module, Layer,
                $"Contact information deleted. Affected rows: {affectedRows}.");
            return affectedRows;
        }
    }
}
