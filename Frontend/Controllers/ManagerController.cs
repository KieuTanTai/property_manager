using Frontend.Models.Admin;
using Frontend.Models.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class ManagerController : Controller
    {
        [HttpGet("/manager")]
        [HttpGet("/manager/dashboard")]
        public IActionResult Dashboard()
        {
            var dashboard = new ManagerDashboardViewModel(
                [
                    new("A01", ManagerPremiseStatus.Rented),
                    new("A02", ManagerPremiseStatus.Available),
                    new("B01", ManagerPremiseStatus.Reserved),
                    new("B02", ManagerPremiseStatus.Maintenance),
                    new("C01", ManagerPremiseStatus.Rented),
                    new("C02", ManagerPremiseStatus.Available)
                ],
                ActiveContracts: 2,
                PaidRevenue: 21_300_000m,
                OverdueInvoices: 1,
                UnresolvedViolations: 2,
                OpenTickets: 2);

            return View(dashboard);
        }

        [HttpGet("/manager/accounts/list")]
        public IActionResult AccountList()
        {
            var accounts = Enumerable.Range(1, 20)
                .Select(index => new AdminAccountRow(
                    $"acc-{index:0000}",
                    $"user{index:00}@example.com",
                    index == 1 ? "Administrator" : index % 3 == 0 ? "Manager" : "Customer",
                    index % 5 != 0,
                    DateTime.Today.AddDays(-index * 4)))
                .ToList();

            return View(accounts);
        }

        [HttpGet("/manager/statistics")]
        public IActionResult Statistics() => View(new ManagerStatisticsViewModel(6, 2, 2, 2, 21_300_000m, 2, 2));

        [HttpGet("/manager/premises")]
        public IActionResult Premises() => View(List("Mặt bằng", "Theo dõi tình trạng và vị trí mặt bằng.", new[]
        {
            new ManagerPremise("PRE-001", "A01", 1, "45m²", ManagerPremiseStatus.Rented, "123 Nguyễn Huệ"),
            new ManagerPremise("PRE-002", "A02", 1, "38m²", ManagerPremiseStatus.Available, "123 Nguyễn Huệ"),
            new ManagerPremise("PRE-003", "B01", 2, "60m²", ManagerPremiseStatus.Reserved, "45 Lê Lợi"),
            new ManagerPremise("PRE-004", "B02", 2, "55m²", ManagerPremiseStatus.Maintenance, "45 Lê Lợi"),
            new ManagerPremise("PRE-005", "C01", 3, "80m²", ManagerPremiseStatus.Rented, "78 Đinh Tiên Hoàng"),
            new ManagerPremise("PRE-006", "C02", 3, "72m²", ManagerPremiseStatus.Available, "78 Đinh Tiên Hoàng")
        }));

        [HttpGet("/manager/contracts")]
        public IActionResult Contracts() => View(List("Hợp đồng", "Theo dõi vòng đời hợp đồng thuê.", new[]
        {
            new ManagerContract("CTR-2024-001", "Nguyễn Văn A", "A01", 7_500_000m, new DateTime(2025, 12, 31), ManagerContractStatus.Signed),
            new ManagerContract("CTR-2024-002", "Trần Thị B", "C01", 12_000_000m, new DateTime(2025, 6, 30), ManagerContractStatus.Signed),
            new ManagerContract("CTR-2024-003", "Lê Văn C", "B01", 9_000_000m, new DateTime(2024, 12, 31), ManagerContractStatus.PendingApproval),
            new ManagerContract("CTR-2024-004", "Phạm Thị D", "A02", 5_700_000m, new DateTime(2025, 9, 30), ManagerContractStatus.PendingSignature)
        }));

        [HttpGet("/manager/invoices")]
        public IActionResult Invoices() => View(List("Hóa đơn", "Theo dõi công nợ và thời hạn thanh toán.", new[]
        {
            new ManagerInvoice("INV-2024-001", "Nguyễn Văn A", "A01", 8_200_000m, new DateTime(2024, 9, 30), ManagerInvoiceStatus.Paid),
            new ManagerInvoice("INV-2024-002", "Trần Thị B", "C01", 13_100_000m, new DateTime(2024, 9, 30), ManagerInvoiceStatus.Unpaid),
            new ManagerInvoice("INV-2024-004", "Trần Thị B", "C01", 13_400_000m, new DateTime(2024, 8, 31), ManagerInvoiceStatus.Overdue),
            new ManagerInvoice("INV-2024-005", "Lê Văn C", "B01", 10_200_000m, new DateTime(2024, 10, 15), ManagerInvoiceStatus.Unpaid)
        }));

        [HttpGet("/manager/violations")]
        public IActionResult Violations() => View(List("Vi phạm", "Xử lý các vi phạm phát sinh từ hợp đồng.", new[]
        {
            new ManagerViolation("VIO-001", "Nguyễn Văn A", "Sử dụng loa quá âm lượng sau 22:00", 500_000m, new DateTime(2024, 9, 5), false),
            new ManagerViolation("VIO-002", "Trần Thị B", "Để rác không đúng nơi quy định", 200_000m, new DateTime(2024, 8, 20), true),
            new ManagerViolation("VIO-003", "Nguyễn Văn A", "Tự ý cải tạo mặt bằng", 2_000_000m, new DateTime(2024, 9, 15), false)
        }));

        [HttpGet("/manager/tickets")]
        public IActionResult Tickets() => View(List("Yêu cầu", "Tiếp nhận và xử lý phản hồi từ khách thuê.", new[]
        {
            new ManagerTicket("TKT-001", "nguyen.van.a@gmail.com", "Điều hòa tầng 1 bị hỏng", ManagerTicketType.Complaint, false, new DateTime(2024, 9, 22)),
            new ManagerTicket("TKT-002", "tran.thi.b@gmail.com", "Dịch vụ vệ sinh hành lang chưa tốt", ManagerTicketType.Feedback, true, new DateTime(2024, 9, 18)),
            new ManagerTicket("TKT-003", "le.van.c@company.vn", "Phản ánh vi phạm giờ giấc", ManagerTicketType.Violation, false, new DateTime(2024, 9, 20))
        }));

        private static ManagerListViewModel<T> List<T>(string title, string description, IReadOnlyList<T> items) =>
            new(title, description, items);
    }
}