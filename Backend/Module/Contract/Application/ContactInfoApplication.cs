using Contract.Interfaces.IApplication;
using Contract.Interfaces.IRepository;
using Contract.Models.Contact;
using Shared.Interfaces;

namespace Contract.Application
{
    public class ContactInfoApplication(
        IUnitOfWork unitOfWork,
        IContactInfoRepository contactInfoRepository) : IContactInfoApplication
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IContactInfoRepository _contactInfoRepository = contactInfoRepository;

        public Task<IReadOnlyList<ContactInfoModel>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            _contactInfoRepository.GetAllAsync(cancellationToken);

        public async Task<ContactInfoModel> AddAsync(ContactInfoModel contactInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contactInfo);
            await _contactInfoRepository.AddAsync(contactInfo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return contactInfo;
        }

        public async Task<int> UpdateAsync(ContactInfoModel contactInfo,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contactInfo);
            await _contactInfoRepository.UpdateAsync(contactInfo, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteAsync(Guid contactInfoId,
            CancellationToken cancellationToken = default)
        {
            await _contactInfoRepository.DeleteAsync(contactInfoId, cancellationToken);
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
