using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Domain.Entities
{
    public class TaskListDto
    {

        public TaskListDto()
        {
            TaskList = new List<TaskDto>();
        }

        [NotMapped]
        public List<TaskDto> TaskList { get; set; }

       
    }
}
