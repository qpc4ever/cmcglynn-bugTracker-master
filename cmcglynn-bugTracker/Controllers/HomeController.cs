using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using cmcglynn_bugTracker.Models;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Mail;
using static cmcglynn_bugTracker.EmailService;
using cmcglynn_bugTracker.Models.CodeFirst;
using Microsoft.AspNet.Identity;

namespace cmcglynn_bugTracker.Controllers
{
    [Authorize]
    public class HomeController : Universal
    {
        public ActionResult Index()
        {
            ViewBag.AssignedTk = db.Tickets.Where(t => t.TicketStatus.Name == "Assigned").Count();
            ViewBag.UnassignedTk = db.Tickets.Where(t => t.TicketStatus.Name == "Unassigned").Count();
            ViewBag.CompleteTk = db.Tickets.Where(t => t.TicketStatus.Name == "Complete").Count();

            var user = db.Users.Find(User.Identity.GetUserId());
            var tickets = db.Tickets.Include(t => t.AssignToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            List<Ticket> Tickets = new List<Ticket>();

            if (User.IsInRole("Admin"))
            {
                return View(tickets.ToList());
            }
            else if (User.IsInRole("Developer"))
            {
                return View(tickets.Where(c => c.AssignToUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Submitter"))
            {
                return View(tickets.Where(c => c.OwnerUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Project Manager"))
            {
                return View(tickets.Where(c => c.Project.Users.Any(u => u.Id == user.Id)));
            }

            return View(tickets);
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

        public ActionResult CustomErrors()
        {
            return View();
        }
    }
}
    