using Contract.Models.Contact;
using Shared.Interfaces;

namespace Contract.Interfaces.IRepository
{
    public interface IContactInfoRepository
        : IBaseReadRepository<ContactInfoModel, Guid>, IBaseUpsertRepository<ContactInfoModel>
    {
        Task DeleteAsync(Guid contactInfoId, CancellationToken cancellationToken = default);
    }
}