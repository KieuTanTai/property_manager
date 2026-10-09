using Shared.Enum;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface ITicketRepository
    {
        Task AddAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default);

        void UpdateRange(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TicketModel?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketModel>> GetByIdsAsync(
            IEnumerable<Guid> ticketIds,
            CancellationToken cancellationToken = default);

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