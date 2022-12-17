using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenDiscussion.Data;
using OpenDiscussion.Models;
using System.Data;

namespace OpenDiscussion.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public TopicsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int id)
        {
            Topic top = db.Topics.Include("Category")
                                 .Include("User")
                                 .Include("Responses")
                                 .Include("Responses.User")
                                 .Where(top => top.Id == id)
                                 .First();

            SetAccessRights();

            return View(top);
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Moderator"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);
            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [HttpPost]
        public IActionResult Show([FromForm] Response resp) 
        {
            resp.Date = DateTime.Now;
            resp.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Responses.Add(resp);
                db.SaveChanges();
                return Redirect("/Topics/Show/" + resp.TopicId);
            }
            else
            {
                Topic top = db.Topics.Include("Category")
                                     .Include("User")
                                     .Include("Responses")
                                     .Include("Responses.User")
                                     .Where(top => top.Id == resp.TopicId)
                                     .First();

                SetAccessRights();

                return View(top);
            }

        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New()
        {
            Topic top = new Topic();

            top.Categ = GetAllCategories();

            return View(top);
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New(Topic top)
        {
            top.Date = DateTime.Now;
            top.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Topics.Add(top);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                return RedirectToAction("Show", "Categories", new { id = top.CategoryId });
            }
            else
            {
                top.Categ = GetAllCategories();
                return View(top);
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id)
        {
            Topic top = db.Topics.Include("Category")
                                 .Where(top => top.Id == id)
                                 .First();

            top.Categ = GetAllCategories();

            if (top.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(top);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                return RedirectToAction("Show", "Categories", new { id = top.CategoryId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id, Topic requestTopic)
        {
            Topic top = db.Topics.Find(id);

            if (ModelState.IsValid)
            {
                if (top.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    top.Title = requestTopic.Title;
                    top.Content = requestTopic.Content;
                    top.Date = requestTopic.Date;
                    top.CategoryId = requestTopic.CategoryId;
                    db.SaveChanges();

                    TempData["message"] = "Articolul a fost modificat";

                    return RedirectToAction("Show", "Categories", new { id = top.CategoryId });
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va \r\napartine";
                    return RedirectToAction("Show", "Categories", new { id = top.CategoryId });
                }

            }
            else
            {
                requestTopic.Categ = GetAllCategories();
                return View(requestTopic);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Delete(int id)
        {
            Topic top = db.Topics.Include("Responses")
                                 .Where(top => top.Id == id)
                                 .First();

            if (top.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                var categId = top.CategoryId;
                db.Topics.Remove(top);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters";
                return RedirectToAction("Show", "Categories", new { id =  categId});
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                return RedirectToAction("Show", "Categories", new { id = top.CategoryId });
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

    }
}