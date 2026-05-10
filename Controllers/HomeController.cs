using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Diagnostics;
using TechMove.Data;
using TechMove.Models;
using TechMove.Security;
using TechMove.Services;

namespace TechMove.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            ICurrencyService currencyService)
        {
            _logger = logger;
            _context = context;
            _currencyService = currencyService;
        }

        public IActionResult Index()
        {
            if (HttpContext.IsLoggedIn())
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new LoginViewModel());
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!HttpContext.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = HttpContext.GetCurrentUser() ?? "User";
            var role = HttpContext.GetCurrentRole() ?? string.Empty;

            if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Username = username;
                ViewBag.Role = role;
                return View();
            }

            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == "Active");
            var pendingRequests = await _context.ServiceRequests.CountAsync(r => r.Status == "Pending");
            var totalClients = await _context.Clients.CountAsync();
            var rate = await _currencyService.GetUSDToZARRateAsync();

            var recent = await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.Id)
                .Take(3)
                .Select(c => new DashboardViewModel.RecentContractItem
                {
                    ContractId = c.Id,
                    ClientName = c.Client != null ? c.Client.Name : "Unknown",
                    Status = c.Status
                })
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                Username = username,
                Role = role,
                ActiveContracts = activeContracts,
                PendingRequests = pendingRequests,
                TotalClients = totalClients,
                CurrencyRateUsdToZar = rate,
                RecentActivity = recent
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport()
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!HttpContext.HasAnyRole("Admin")) return Forbid();

            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            var generatedAt = DateTime.UtcNow;

            var pdfBytes = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(36);
                    page.Size(PageSizes.A4);

                    page.Header().Column(column =>
                    {
                        column.Item().Text("TechMove GLMS").FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text("Contracts export report").FontSize(12).FontColor(Colors.Grey.Darken2);
                        column.Item().Text($"Generated (UTC): {generatedAt:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(72);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.ConstantColumn(88);
                            columns.ConstantColumn(88);
                            columns.RelativeColumn(1);
                        });

                        static IContainer CellStyle(IContainer container, bool header = false) =>
                            container
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(header ? Colors.Grey.Lighten4 : Colors.White)
                                .PaddingVertical(6)
                                .PaddingHorizontal(8)
                                .AlignMiddle();

                        table.Header(header =>
                        {
                            header.Cell().Element(c => CellStyle(c, true)).Text("ID").SemiBold();
                            header.Cell().Element(c => CellStyle(c, true)).Text("Client").SemiBold();
                            header.Cell().Element(c => CellStyle(c, true)).Text("Status").SemiBold();
                            header.Cell().Element(c => CellStyle(c, true)).Text("Start").SemiBold();
                            header.Cell().Element(c => CellStyle(c, true)).Text("End").SemiBold();
                            header.Cell().Element(c => CellStyle(c, true)).Text("Service level").SemiBold();
                        });

                        foreach (var c in contracts)
                        {
                            table.Cell().Element(c => CellStyle(c)).Text(c.Id.ToString());
                            table.Cell().Element(c => CellStyle(c)).Text(c.Client?.Name ?? "—");
                            table.Cell().Element(c => CellStyle(c)).Text(c.Status);
                            table.Cell().Element(c => CellStyle(c)).Text(c.StartDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(c => CellStyle(c)).Text(c.EndDate.ToString("yyyy-MM-dd"));
                            table.Cell().Element(c => CellStyle(c)).Text(c.ServiceLevel);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "techmove-contracts-report.pdf");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
