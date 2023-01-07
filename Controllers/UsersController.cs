using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenDiscussion.Data;
using OpenDiscussion.Models;
using System.Data;
using System.Text.RegularExpressions;
using System.Drawing.Printing;

namespace OpenDiscussion.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(
           ApplicationDbContext context,
           UserManager<ApplicationUser> userManager,
           RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Msg = TempData["message"].ToString();
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string id)
        {
            var user = db.Users.FirstOrDefault(x => x.Id == id);
            var userRole = db.UserRoles
                                    .Where(ur => ur.UserId == id)
                                    .ToList();
            ViewBag.roles = db.Roles.ToList();
            ViewBag.userRole = userRole[0];
            return View(user);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id, ApplicationUser newUser)
        {
            var userRole = db.UserRoles.Where(ur => ur.UserId == id)
                                       .ToList()[0];
            db.Remove(userRole);
            db.SaveChanges();
            //userRole.RoleId = newUser.UserName;
            //userRole.UserId = id;
            db.UserRoles.Add(
                 new IdentityUserRole<string>
                 {
                     RoleId = newUser.UserName,
                     UserId = id
                 }
                 );
             db.SaveChanges();

            TempData["message"] = "Rolul a fost modificat";

            return RedirectToAction("Index");

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var userRole = db.UserRoles.Where(ur => ur.UserId == id).ToList()[0];
            db.Users.Remove(user);
            db.UserRoles.Remove(userRole);
            db.SaveChanges();

            TempData["message"] = "Utilizatorul a fost sters";

            return RedirectToAction("Index");
        }
    }
}
