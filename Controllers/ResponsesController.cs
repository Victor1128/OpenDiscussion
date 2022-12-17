using Microsoft.AspNetCore.Mvc;
using OpenDiscussion.Data;

namespace OpenDiscussion.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly ApplicationDbContext db;

        public ResponsesController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
