using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data
{
    public class DbInitializer
    {       
      
        public static void Initialize(TaskContext context)
        {
            context.Database.EnsureCreated();

            // procura por qualquer agendamento
            if (context.Tasks.Any())
            {              
                return;  //O banco foi inicializado
            }                

        }
    }
}
