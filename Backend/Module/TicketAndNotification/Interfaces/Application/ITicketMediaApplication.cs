using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Application
{
    public interface ITicketMediaApplication
    {
        Task<IReadOnlyList<TicketMediaModel>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<TicketMediaModel?> GetByIdAsync(
            int ticketMediaId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketMediaModel>> GetByIdsAsync(
            IEnumerable<int> ticketMediaIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TicketMediaModel>> GetByTicketIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default);

        Task<TicketMediaModel> AddAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default);

        Task<int> AddRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default);

        Task<TicketMediaModel> UpdateAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default);

        Task<int> UpdateRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default);
    }
}
