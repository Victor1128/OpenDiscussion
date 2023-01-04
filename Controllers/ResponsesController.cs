using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenDiscussion.Data;
using OpenDiscussion.Models;

namespace OpenDiscussion.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ResponsesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Delete(int id)
        {
            Response resp = db.Responses.Find(id);

            if (resp.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Responses.Remove(resp);
                db.SaveChanges();
                return Redirect("/Topics/Show/" + resp.TopicId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul!";
                return RedirectToAction("Show", "Categories", new { id = db.Topics.Find(resp.TopicId).CategoryId });
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id)
        {
            Response resp = db.Responses.Find(id);

            if (resp.UserId == _userManager.GetUserId(User) || User.IsInRole("Moderator") || User.IsInRole("Admin"))
            {
                return View(resp);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul!";
                return RedirectToAction("Show", "Categories", new { id = db.Topics.Find(resp.TopicId).CategoryId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id, Response requestResponse)
        {
            Response resp = db.Responses.Find(id);

            if (ModelState.IsValid)
            {
                if (resp.UserId == _userManager.GetUserId(User) || User.IsInRole("Moderator")  || User.IsInRole("Admin"))
                {
                    resp.Content = requestResponse.Content;

                    db.SaveChanges();

                    return Redirect("/Topics/Show/" + resp.TopicId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa editati comentariul!";
                    return RedirectToAction("Show", "Categories", new { id = db.Topics.Find(resp.TopicId).CategoryId});
                }
            }
            else
            {
                return View(requestResponse);
            }

        }
    }
}