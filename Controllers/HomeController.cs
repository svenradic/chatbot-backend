using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using chatbot_backend.Models;
using chatbot_backend.Data;

namespace chatbot_backend.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    public IActionResult Index()
    {
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
