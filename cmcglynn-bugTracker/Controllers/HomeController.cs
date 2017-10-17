using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using cmcglynn_bugTracker.Models;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using static cmcglynn_bugTracker.EmailService;

namespace cmcglynn_bugTracker.Controllers
{
    [Authorize]
    public class HomeController : Universal
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Landing()
        {
            return View();
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                    var from = "MyPortfolio<qpc4ever@gmail.com>";
                    model.Body = "This is a message from your Admin";

                    //var assignedUser = db.Users.Find(ticket).AssignedUserId);
                    //var emailTo = assignedUser.Email;


                    var email = new MailMessage(from,
                        ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "Portfolio Contact Email",
                        Body = string.Format(body, model.FromName, model.FromEmail,
                        model.Body),
                        IsBodyHtml = true
                    };

                    //var svc = new PersonalEmail();
                    //await svc.SendAsync(email);

                    return RedirectToAction("Contact");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }
    }
}
    