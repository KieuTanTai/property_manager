using Frontend.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

public class AdminController : Controller
{
    [HttpGet("/admin/accounts/list")]
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
}
