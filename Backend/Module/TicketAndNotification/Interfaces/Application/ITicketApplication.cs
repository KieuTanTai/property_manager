using Shared.Enum;
using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Application
{
    public interface ITicketApplication
    {
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

        Task<TicketModel> AddAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default);

        Task<int> AddRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default);

        Task<TicketModel> UpdateAsync(
            TicketModel ticket,
            CancellationToken cancellationToken = default);

        Task<int> UpdateRangeAsync(
            IEnumerable<TicketModel> tickets,
            CancellationToken cancellationToken = default);
    }
}
