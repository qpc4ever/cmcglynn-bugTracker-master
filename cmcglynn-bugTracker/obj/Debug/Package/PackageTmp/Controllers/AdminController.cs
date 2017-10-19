using cmcglynn_bugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cmcglynn_bugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Universal
    {
        
       
        // GET: Admin
        public ActionResult Index()
        {
            List<AdminUserViewModels> users = new List<AdminUserViewModels>();
            UserRoleHelper helper = new UserRoleHelper();
            foreach (var user in db.Users.ToList())
            {
                var eachUser = new AdminUserViewModels();
                eachUser.User = user;
                eachUser.SelectedRoles = helper.ListUserRoles(user.Id).ToArray();

                users.Add(eachUser);
            }
            return View(users.OrderBy(u => u.User.LastName).ToList());
        }
        //Get: EditUserRoles
        public ActionResult EditUserRoles(string id)
        {
            var user = db.Users.Find(id);
            var helper = new UserRoleHelper();
            var model = new AdminUserViewModels();
            model.User = user;
            model.SelectedRoles = helper.ListUserRoles(id).ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);

            return View(model);
        }

      /* @**///POST: EditUserRoles*@
        [HttpPost]
        public ActionResult EditUserRoles(AdminUserViewModels model)
        {
            var user = db.Users.Find(model.User.Id);
            UserRoleHelper helper = new UserRoleHelper();
            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(user.Id, role);
            }
            if (model.SelectedRoles != null)
            {
                foreach (var role in model.SelectedRoles)
                {
                    helper.AddUserToRole(user.Id, role);
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // GET: EditProjectUsers
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProjectUsers(int id)
        {
            var project = db.Projects.Find(id);
            var user = db.Users.Find(User.Identity.GetUserId());
            var helper = new ProjectAssignHelper();
            var model = new ProjectViewModel();
            model.AssignProject = project;
            model.AssignProjectId = id;
            model.SelectedUsers = helper.ListUsersOnProject(id).Select(u => u.Id).ToArray();
            model.Users = new MultiSelectList(db.Users, "Id", "FirstName", model.SelectedUsers);

            return View(model);
        }

        // POST: EditProjectUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProjectUsers(ProjectViewModel model)
        {
            var project = db.Projects.Find(model.AssignProjectId);
            var helper = new ProjectAssignHelper();                             // ProjectAssignHelper helper = new ProjectAssignHelper();

            foreach (var user in db.Users.Select(u => u.Id).ToList())
            {
                helper.RemoveUserFromProject(user, project.Id);
            }

            if (model.SelectedUsers != null)
            {
                foreach (var user in model.SelectedUsers)
                {
                    helper.AddUserToProject(user, project.Id);
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
    }
