using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Models
{
    public class SearchTaskModel
    {
        public SearchTaskModel()
        {           
            ListTasksSummarized = new List<SummarizedTasksModel>();
        }

        [Display(Name = "From:")]
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime InitialDate { get; set; }

        [Display(Name = "To:")]
        [NotMapped]
        [DataType(DataType.Date)]
        public DateTime FinalDate { get; set; }                
       
        [NotMapped]
        public List<SummarizedTasksModel> ListTasksSummarized { get; set; }
    }
}
