using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Models
{
    public class TestAspNetFunctionsModel
    {

        [NotMapped]
        [Display(Name = "InputA")]
        public string InputA { get; set; }

        [NotMapped]
        [Display(Name = "InputB")]
        public string InputB { get; set; }

        [NotMapped]
        [Display(Name = "OutPut")]
        public string OutPut { get; set; }
           
    }
}
