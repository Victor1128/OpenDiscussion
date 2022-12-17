using Microsoft.AspNetCore.Mvc;
using OpenDiscussion.Data;

namespace OpenDiscussion.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext db;

        public TopicsController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var topics = from topic in db.Topics
                         select topic;

            ViewBag.Topics = topics;

            return View();
        }
    }
}
