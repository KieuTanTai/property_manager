using Contract.Infrastructure.Persistence.DbContext;
using Contract.Interfaces.IRepository;
using Contract.Models.Contact;
using Microsoft.EntityFrameworkCore;
using Shared.Logging;

namespace Contract.Infrastructure.Repository.ContactRepository
{
    public class ContactInfoRepository(
        ContractDbContext context,
        ILogger<ContactInfoRepository> logger,
        ILogPool logPool) : IContactInfoRepository
    {
        private const string Module = "Contract";
        private const string Layer = "Infrastructure/Repository/ContactRepository";
        private readonly ContractDbContext _db = context;
        private readonly ILogger<ContactInfoRepository> _logger = logger;
        private readonly ILogPool _logPool = logPool;

        public async Task<IReadOnlyList<ContactInfoModel>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<ContactInfoModel?> GetByIdAsync(Guid id,
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.AsNoTracking()
                .FirstOrDefaultAsync(contactInfo => contactInfo.ContactInfoId == id, cancellationToken);

        public async Task<IReadOnlyList<ContactInfoModel>> GetByIdsAsync(IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.AsNoTracking()
                .Where(contactInfo => ids.Contains(contactInfo.ContactInfoId))
                .ToListAsync(cancellationToken);

        public async Task<ContactInfoModel?> GetTrackedByIdAsync(Guid id,
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.FirstOrDefaultAsync(
                contactInfo => contactInfo.ContactInfoId == id, cancellationToken);

        public async Task<bool> ExistsAsync(Guid id,
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.AnyAsync(contactInfo => contactInfo.ContactInfoId == id, cancellationToken);

        public async Task AddAsync(ContactInfoModel entity,
            CancellationToken cancellationToken = default)
        {
            await EnsureUniqueAsync(entity, cancellationToken);
            await _db.ContactInfos.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<ContactInfoModel> entities,
            CancellationToken cancellationToken = default) =>
            await _db.ContactInfos.AddRangeAsync(entities, cancellationToken);

        public async Task UpdateAsync(ContactInfoModel entity,
            CancellationToken cancellationToken = default)
        {
            if (entity.ContactInfoId == Guid.Empty)
                throw new ArgumentException("Contact info id is required.", nameof(entity.ContactInfoId));

            if (!await ExistsAsync(entity.ContactInfoId, cancellationToken))
                throw new InvalidOperationException("Contact info not found.");

            await EnsureUniqueAsync(entity, cancellationToken);
            _db.ContactInfos.Update(entity);
        }

        public void UpdateRange(IEnumerable<ContactInfoModel> entities,
            CancellationToken cancellationToken = default) =>
            _db.ContactInfos.UpdateRange(entities);

        public async Task DeleteAsync(Guid contactInfoId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.ContactInfos.FirstOrDefaultAsync(
                contactInfo => contactInfo.ContactInfoId == contactInfoId, cancellationToken);
            if (entity is null)
                throw new InvalidOperationException("Contact info not found.");

            _db.ContactInfos.Remove(entity);
        }

        private async Task EnsureUniqueAsync(ContactInfoModel entity,
            CancellationToken cancellationToken)
        {
            var duplicate = await _db.ContactInfos.AnyAsync(contactInfo =>
                contactInfo.ContactInfoId != entity.ContactInfoId &&
                (contactInfo.ContactInfoEmail == entity.ContactInfoEmail ||
                 contactInfo.ContactInfoPhone == entity.ContactInfoPhone), cancellationToken);
            if (duplicate)
                throw new InvalidOperationException("Contact info email or phone already exists.");
        }
    }
}
