using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Models
{
    public class SummarizedTasksModel
    {
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [NotMapped]       
        public string Hours { get; set; }

        [NotMapped]
        public int TotalTasks { get; set; }

        [NotMapped]
        public string AverageHours { get; set; }

        [NotMapped]
        public double PercentualConcludedTasks { get; set; }


    }
}
