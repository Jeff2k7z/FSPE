using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FSPE.Models
{
    public class ClubRegistration
    {
        public int Id { get; set; }

        public string RegistrationKey { get; set; }

        [Display(Name = "Parent Name")]
        [Required(ErrorMessage = "Parent Name is required")]
        public string ParentName { get; set; }

        [Display(Name = "Parent Email Address")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Contact Phone Number")]
        [Required(ErrorMessage = "Phone Number is required")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:###-##-####}")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Child Name")]
        [Required(ErrorMessage = "Child's Name is required")]
        public string ChildName { get; set; }

        [Display(Name = "Child Grade")]
        [Required(ErrorMessage = "Child's Grade is required")]
        public string Grade { get; set; }

        [Display(Name = "Teacher Name")]
        [Required()]
        public string Teacher { get; set; }

        public Club Club { get; set; }
        public int ClubId { get; set; }

        public DateTime RegistrationDate { get; set; }
        public bool IsPaid { get; set; }
        public string CouponCode { get; set; }
        
        public ChildDisposition ChildDisposition { get; set; }
        [Display(Name = "How will your child leave Club Activities?")]
        public int ChildDispositionId { get; set; }
    }
}