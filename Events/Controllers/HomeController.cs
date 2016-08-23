using Events.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Events.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index(string message)
        {
            using (var ctx = new EventsEntities())
            {
                ViewBag.LastEvents = EventViewModel.ExecQuery(ctx.FindFinishedEvents());
                ViewBag.UpcomingEvents = EventViewModel.ExecQuery(ctx.FindUpcomingEvents());
                ViewBag.Message = message;

                if (Request.IsAuthenticated)
                {
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ViewBag.UnconfirmedUsers = userManager.Users.Where(user => !user.EmailConfirmed).ToList();
                }

                return View();
            }
        }
    }
}