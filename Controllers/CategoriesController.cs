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
        public ActionResult Index(string sortOrder)
        {
            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categories = categories;
            ViewBag.Title = "Categorii";

            if (TempData.ContainsKey("message"))
            {
                ViewBag.AlertMsg = TempData["message"].ToString();
            }

            if (!String.IsNullOrEmpty(HttpContext.Request.Query["search"]) && !String.IsNullOrWhiteSpace(HttpContext.Request.Query["search"]))
            {
                ICollection<Topic>? topics;
                if (sortOrder == "resp")
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .OrderByDescending(
                                                        top =>
                                                        db.Responses
                                                        .Where(resp => resp.TopicId == top.Id)
                                                        .Count()
                                                        )
                                      .ThenByDescending(top => top.Date)
                                      .ToList();
                }
                else
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .OrderByDescending(top => top.Date)
                                      .ToList();
                }

                // MOTOR DE CAUTARE
                string search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> topicIds = db.Topics.Where(
                                                    top => top.Title.Contains(search)
                                                            || top.Content.Contains(search)
                                                    ).Select(t => t.Id).ToList();

                List<int> topicIdsOfResponsesWithSearchString = db.Responses.Where(rsp => rsp.Content.Contains(search))
                                                                            .Select(r => r.TopicId.GetValueOrDefault())
                                                                            .ToList();

                List<int> mergedIds = topicIds.Union(topicIdsOfResponsesWithSearchString).ToList();

                if (sortOrder == "resp")
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .Where(topic => mergedIds.Contains(topic.Id))
                                      .OrderByDescending(
                                                    top =>
                                                    db.Responses
                                                    .Where(resp => resp.TopicId == top.Id)
                                                    .Count()
                                                    )
                                      .ThenByDescending(top => top.Date)
                                      .ToList();
                }
                else
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .Where(
                                             topic => mergedIds.Contains(topic.Id)
                                            )
                                      .OrderByDescending(t => t.Date)
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
                ViewBag.PaginationBaseUrl = "/Categories/Index/?search=" + search + "&sortOrder=" + sortOrder + "&page";
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Show(int id, string sortOrder)
        {
            Category category = db.Categories.Find(id);
            ICollection<Topic>? topics;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.AlertMsg = TempData["message"].ToString();
            }

            if (sortOrder == "resp")
            {
                topics = db.Topics.Include("Category")
                                  .Include("User")
                                  .Where(top => top.CategoryId == id)
                                  .OrderByDescending(
                                                    top =>
                                                    db.Responses
                                                    .Where(resp => resp.TopicId == top.Id)
                                                    .Count()
                                                    )
                                  .ThenByDescending(top => top.Date)
                                  .ToList();
            }
            else
            {
                topics = db.Topics.Include("Category")
                                  .Include("User")
                                  .Where(top => top.CategoryId == id)
                                  .OrderByDescending(top => top.Date)
                                  .ToList();
            }

            string search = null;

            // MOTOR DE CAUTARE

            if (!String.IsNullOrEmpty(HttpContext.Request.Query["search"]) && !String.IsNullOrWhiteSpace(HttpContext.Request.Query["search"]))
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> topicIds = db.Topics.Where(top => top.Title.Contains(search) || top.Content.Contains(search))
                                              .Select(t => t.Id).ToList();


                List<int> topicIdsOfResponsesWithSearchString = db.Responses.Where(rsp => rsp.Content.Contains(search))
                                                                            .Select(r => r.TopicId.GetValueOrDefault())
                                                                            .ToList();

                List<int> mergedIds = topicIds.Union(topicIdsOfResponsesWithSearchString).ToList();

                if(sortOrder == "resp")
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .Where(topic => mergedIds.Contains(topic.Id) && topic.CategoryId == id)
                                      .OrderByDescending(top => db.Responses.Where(resp => resp.TopicId == top.Id).Count())
                                      .ThenByDescending(top => top.Date)
                                      .ToList();
                }
                else
                {
                    topics = db.Topics.Include("Category")
                                      .Include("User")
                                      .Where(topic => mergedIds.Contains(topic.Id) && topic.CategoryId == id)
                                      .OrderByDescending(t => t.Date)
                                      .ToList();
                }

            }

            ViewBag.SearchString = search;

            //AFISARE PAGINATA

            int _perPage = 3;
            int totalItems = topics.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedTopics = topics.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Topics = paginatedTopics;
            ViewBag.CategoryName = category.CategoryName;

            if (!String.IsNullOrEmpty(HttpContext.Request.Query["search"]) && !String.IsNullOrWhiteSpace(HttpContext.Request.Query["search"]))
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?search="
                + search + "&sortOrder="+sortOrder+"&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + id + "?sortOrder="+sortOrder+"&page";
            }

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

                TempData["message"] = "Categoria a fost adaugata cu succes!";

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

                TempData["message"] = "Categoria a fost modificata cu succes!";

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

            TempData["message"] = "Categoria a fost stearsa cu succes!";

            return RedirectToAction("Index");
        }
    }
}
