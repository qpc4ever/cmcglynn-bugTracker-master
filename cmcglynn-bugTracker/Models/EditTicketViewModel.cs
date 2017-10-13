using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace cmcglynn_bugTracker.Models
{
    public class EditTicketViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "A title is required")]
        [Display(Name = "Ticket Title")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 6)]
        public string Title { get; set; }
        [Required(ErrorMessage = "A description is required")]
        [Display(Name = "Describe the issue")]
        [StringLength(280, ErrorMessage = "The {0} must be at least {2} characters long", MinimumLength = 6)]
        public string Description { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public string AssignToUserId { get; set; }

        public SelectList TypeList { get; set; }
        public SelectList PriorityList { get; set; }
        public SelectList StatusList { get; set; }
        public SelectList AssignToUserList { get; set; }
    }
}