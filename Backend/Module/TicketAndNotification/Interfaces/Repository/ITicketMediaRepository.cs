using Shared.Interfaces;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface ITicketMediaRepository : IBaseReadRepository<TicketMediaModel, int>, IBaseUpsertRepository<TicketMediaModel>
    {
        Task<IReadOnlyList<TicketMediaModel>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default);

        Task DeleteByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    }
}