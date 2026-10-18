using Contract.Models.Contact;

namespace Contract.Interfaces.IApplication
{
    public interface IContactInfoApplication
    {
        Task<IReadOnlyList<ContactInfoModel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ContactInfoModel> AddAsync(ContactInfoModel contactInfo, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(ContactInfoModel contactInfo, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(Guid contactInfoId, CancellationToken cancellationToken = default);
    }
}