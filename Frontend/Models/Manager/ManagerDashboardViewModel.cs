namespace Frontend.Models.Manager;

public sealed record ManagerDashboardViewModel(
    IReadOnlyList<ManagerPremiseSummary> Premises,
    int ActiveContracts,
    decimal PaidRevenue,
    int OverdueInvoices,
    int UnresolvedViolations,
    int OpenTickets);

public sealed record ManagerPremiseSummary(
    string Name,
    ManagerPremiseStatus Status);

public enum ManagerPremiseStatus
{
    Available,
    Reserved,
    Rented,
    Maintenance
}

public sealed record ManagerPremise(
    string Id, string Name, int Floor, string Area, ManagerPremiseStatus Status, string Location);

public sealed record ManagerContract(
    string Id, string Tenant, string PremiseName, decimal RentalPrice, DateTime ReturnDate, ManagerContractStatus Status);

public enum ManagerContractStatus { PendingApproval, PendingSignature, Signed, Expired, Terminated }

public sealed record ManagerInvoice(
    string Id, string Tenant, string PremiseName, decimal TotalAmount, DateTime DueDate, ManagerInvoiceStatus Status);

public enum ManagerInvoiceStatus { Paid, Unpaid, Overdue }

public sealed record ManagerViolation(
    string Id, string Tenant, string Content, decimal PenaltyAmount, DateTime Date, bool Resolved);

public sealed record ManagerTicket(
    string Id, string Account, string Content, ManagerTicketType Type, bool Resolved, DateTime CreatedAt);

public enum ManagerTicketType { Complaint, Violation, Feedback, Review }

public sealed record ManagerListViewModel<T>(string Title, string Description, IReadOnlyList<T> Items);

public sealed record ManagerStatisticsViewModel(
    int TotalPremises, int RentedPremises, int AvailablePremises, int ActiveContracts,
    decimal PaidRevenue, int OutstandingInvoices, int OpenTickets);
