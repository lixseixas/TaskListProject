using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Models
{
    public class TaskModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Initial Hour")]
        [DataType(DataType.Time)]
        public DateTime InitialHour { get; set; }

        [Required]
        [Display(Name = "Final Hour")]
        [DataType(DataType.Time)]
        public DateTime FinalHour { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [Display(Name = "Priority")]
        [NotMapped]
        public string PriorityName { get; set; }

        [Required]
        [Display(Name = "Ended")]
        public bool Ended { get; set; }

        [NotMapped]
        public string Inclusion { get; set; }
    }
}
