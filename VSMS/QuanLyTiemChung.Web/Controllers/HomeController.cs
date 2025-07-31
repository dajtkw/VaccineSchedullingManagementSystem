using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuanLyTiemChung.Web.Models;

namespace QuanLyTiemChung.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        // Nếu người dùng đã đăng nhập, chuyển hướng họ đến trang dashboard phù hợp với vai trò của họ.
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            if (User.IsInRole("HealthOfficial"))
            {
                return RedirectToAction("Index", "MedicalStaff");
            }
            if (User.IsInRole("Citizen"))
            {
                // Người dân sẽ được chuyển đến trang quản lý lịch hẹn của họ
                return RedirectToAction("Index", "Appointment");
            }
        }
        // Nếu chưa đăng nhập, hiển thị trang chủ.
        return View();
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
