using TicketAndNotification.Models.Ticket;

namespace TicketAndNotification.Interfaces.Repository
{
    public interface ITicketMediaRepository
    {
        Task AddAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            TicketMediaModel ticketMedia,
            CancellationToken cancellationToken = default);

        void UpdateRange(
            IEnumerable<TicketMediaModel> ticketMedias,
            CancellationToken cancellationToken = default);

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
    }
}
