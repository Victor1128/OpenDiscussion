using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenDiscussion.Data;
using OpenDiscussion.Models;
using System.Data;
using System.Text.RegularExpressions;

namespace OpenDiscussion.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoriesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        //[Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Index()
        {
            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categories = categories;
            ViewBag.Title = "Categorii";

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"].ToString();
            }
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                ICollection<Topic>? topics = db.Topics.Include("Category")
                                                  .Include("User")
                                                  .OrderBy(top => top.Date)
                                                  .ToList();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"].ToString();
            }
            var search = "";
            // MOTOR DE CAUTARE
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> topicIds = db.Topics.Where
                                        (
                                        top => top.Title.Contains(search)
                                              || top.Content.Contains(search)
                                        ).Select(t => t.Id).ToList();

                List<int> topicIdsOfResponsesWithSearchString =
                            db.Responses.Where
                            (
                            rsp => rsp.Content.Contains(search)
                            ).Select(r => (int)r.TopicId).ToList();
                List<int> mergedIds = topicIds.Union(topicIdsOfResponsesWithSearchString).ToList();
                topics = db.Topics.Include("Category")
                                  .Include("User")
                                  .Where
                                  (
                                  topic => mergedIds.Contains(topic.Id)
                                  )
                                  .OrderBy(t => t.Date)
                                  .ToList();
            
            ViewBag.SearchString = search;
            //AFISARE PAGINATA
            int _perPage = 3;
            int totalItems = topics.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset  = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }
            var paginatedTopics = topics.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Topics = paginatedTopics;
            ViewBag.Title = "Subiectele care se potrivesc cautarii tale";
            ViewBag.PaginationBaseUrl = "/Categories/Index/?search="
                + search + "&page";
        }
            return View();
        }

        [AllowAnonymous]
        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);

            ICollection<Topic>? topics = db.Topics.Include("Category")
                                                  .Include("User")
                                                  .Where(top => top.CategoryId == id)
                                                  .OrderBy(top => top.Date)
                                                  .ToList();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"].ToString();
            }

            var search = "";
            // MOTOR DE CAUTARE


            var search = "";
            
            // MOTOR DE CAUTARE
            

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> topicIds = db.Topics.Where
                                        (
                                        top => top.Title.Contains(search)
                                              || top.Content.Contains(search)
                                        ).Select(t => t.Id).ToList();

                List<int> topicIdsOfResponsesWithSearchString =
                            db.Responses.Where
                            (
                            rsp => rsp.Content.Contains(search)
                            ).Select(r => (int)r.TopicId).ToList();

                List<int> mergedIds = topicIds.Union(topicIdsOfResponsesWithSearchString).ToList();

                topics = db.Topics.Include("Category")
                                  .Include("User")
                                  .Where
                                  (
                                  topic => mergedIds.Contains(topic.Id)
                                  && topic.CategoryId == id
                                  )
                                  .OrderBy(t => t.Date)
                                  .ToList();
            }

            ViewBag.SearchString = search;

            //AFISARE PAGINATA


            int _perPage = 3;
            int totalItems = topics.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset  = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedTopics = topics.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Topics = paginatedTopics;
            ViewBag.CategoryName = category.CategoryName;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?search="
                + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?page";
            }

            //category.Topics = (ICollection<Topic>?) paginatedTopics;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult New(Category cat)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(cat);
                db.SaveChanges();

                TempData["message"] = "Categoria a fost adaugata";

                return RedirectToAction("Index");
            }
            else
            {
                Console.WriteLine("Error New Categ!");
                return View(cat);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);

            if (ModelState.IsValid)
            {
                category.CategoryName = requestCategory.CategoryName;
                db.SaveChanges();

                TempData["message"] = "Categoria a fost modificata";

                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["message"] = "Categoria a fost stearsa";

            return RedirectToAction("Index");
        }
    }
}
