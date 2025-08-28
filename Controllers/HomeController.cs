
using Microsoft.AspNetCore.Mvc;

namespace RoomBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
