using Shared.Enum;
using Shared.Interfaces;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface ITicketRepository : IBaseReadRepository<TicketModel, Guid>, IBaseUpsertRepository<TicketModel>
    {
        Task<IReadOnlyList<TicketModel>> GetByAccountIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketModel>> GetByTypeAsync(
            ETicketType type,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketModel>> GetByIsResolvedAsync(
            bool isResolved,
            CancellationToken cancellationToken = default);

        Task<string?> GetContentByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default);
    }
}