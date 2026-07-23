using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Domain.Entities
{
    public class SearchTaskDto
    {
        public SearchTaskDto()
        {           
            ListTasksSummarized = new List<SummarizedTasksDto>();
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
        public List<SummarizedTasksDto> ListTasksSummarized { get; set; }
    }
}
