using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Configuration;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;

namespace TestProject
{
    public class Dbests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetTasksTest()
        {
                       
            List<TaskDto> taskList = new List<TaskDto>();
            TasksQueries taskBd = new TasksQueries();
            bool methodReturn = taskBd.GetTasks(ref taskList);

            Assert.IsTrue(methodReturn);
        }
    }
}