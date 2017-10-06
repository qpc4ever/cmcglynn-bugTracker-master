using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using cmcglynn_bugTracker.Models;
using cmcglynn_bugTracker.Models.CodeFirst;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using System.IO;

namespace cmcglynn_bugTracker.Controllers
{
    public class TicketsController : Universal
    {
        //private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
           

            var user = db.Users.Find(User.Identity.GetUserId());
            var tickets = db.Tickets.Include(t => t.AssignToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            List<Ticket> Tickets = new List<Ticket>();


            if (User.IsInRole("Project Manager"))
            {
                return View(Tickets.Where(c => c.Project.Users.Any(u => u.Id == user.Id)));
            }

            else if (User.IsInRole("Developer"))
            {
                return View(Tickets.Where(c => c.AssignToUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Submitter"))
            {
                return View(db.Tickets.Where(c => c.OwnerUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Admin"))
            {
                return View(db.Tickets.ToList());
            }
            return View(tickets);
        }

        //// POST: Tickets
        //public IQueryable<Ticket> IndexSearch(string searchStr)
        //{
        //    IQueryable<Ticket> result = null; if (searchStr != null) { result = db.Tickets.AsQueryable(); result = result.Where(t => t.Title.Contains(searchStr) || t.Description.Contains(searchStr) || t.Comments.Any(t => t.Body.Contains(searchStr) || /*t.OwnerUser.FirstName.Contains(searchStr) ||*/ t.Author.LastName.Contains(searchStr) || /*t.Author.DisplayName.Contains(searchStr) ||*/ t.Author.Email.Contains(searchStr))); } else { result = db.Tickets.AsQueryable(); }

        //    return result.OrderByDescending(t => t.Created);
        //}



        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            //ROLE CHECKING SECURITY
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            ViewBag.ProjectId = new SelectList(db.Projects.Where(p => p.Users.Any(u => u.Id == user.Id)), "Id", "Title");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
           
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,")] Ticket ticket)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.TicketStatusId = 1;
                ticket.OwnerUserId = user.Id;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

           
            ViewBag.ProjectId = new SelectList(db.Projects.Where(p => p.Users.Any(u => u.Id == user.Id)), "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
           
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
       
        [Authorize(Roles = "Admin,Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                if(ticket.AssignToUserId != null)
                    {
                    ticket.TicketStatusId = 2;
                }
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //POST: Ticket Attachments/Create
        [Authorize]

        [ValidateAntiForgeryToken]
        public ActionResult AttachmentCreate(IEnumerable<HttpPostedFileBase> files, int ticketId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Ticket ticket = db.Tickets.Find(ticketId);
            if (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id))

                //if (ModelState.IsValid)
                foreach (var file in files)
            {
                TicketAttachment attachment = new TicketAttachment();

                file.SaveAs(Path.Combine(Server.MapPath("~/TicketAttachments/"), Path.GetFileName(file.FileName)));
                attachment.FileUrl = file.FileName;

                attachment.AuthorId = User.Identity.GetUserId();
                attachment.TicketId = ticketId;
                attachment.Created = DateTimeOffset.Now;

                db.TicketAttachments.Add(attachment);
                db.SaveChanges();
            }

            
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        // GET: Comments/Create
        public ActionResult CommentCreate()
        {
            ViewBag.AuthorId = new SelectList(db.ApplicationUser, "Id", "FirstName");
            ViewBag.PostId = new SelectList(db.Tickets, "Id", "Title");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommentCreate([Bind(Include = "Id,TicketId,Body")] TicketComment ticketComment)
        {
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticketComment.Ticket.Project.Users.Any(u => u.Id == userId)) || (User.IsInRole("Developer") && ticketComment.Ticket.AssignToUserId == userId) || (User.IsInRole("Submitter") && ticketComment.Ticket.OwnerUserId == userId))
                if (ModelState.IsValid)
            {
                if (!String.IsNullOrWhiteSpace(userId))
                {
                    ticketComment.Created = DateTime.Now;
                    ticketComment.AuthorId = User.Identity.GetUserId();
                    db.TicketComments.Add(ticketComment);
                    db.SaveChanges();

                    var ticket = db.Tickets.Find(ticketComment.TicketId);
                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                }
            }
            return RedirectToAction("Index", "Tickets");


            ViewBag.AuthorId = new SelectList(db.ApplicationUser, "Id", "FirstName", ticketComment.AuthorId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            return View(ticketComment);
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
