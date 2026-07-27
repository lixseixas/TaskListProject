using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskProject.Models
{
    public class TaskListModel
    {

        public TaskListModel()
        {
            TaskList = new List<TaskModel>();
        }

        [NotMapped]
        public List<TaskModel> TaskList { get; set; }

       
    }
}
