using System;
using System.ComponentModel.DataAnnotations;

namespace TaskProject.Domain.Entities
{
    public class UserLoginDto
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "User")]
        public string User { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
