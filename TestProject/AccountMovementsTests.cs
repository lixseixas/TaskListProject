using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;

namespace TestProject
{
    public class AccountMovementsTests
    {
        private DbContextOptions<TaskContext> CreateNewContextOptions()
        {
            // Return empty options (tests compile without requiring EFCore.InMemory package).
            // Note: these tests assume a provider is configured; if you add Microsoft.EntityFrameworkCore.InMemory
            // to the test project, replace this with UseInMemoryDatabase to enable full in-memory behavior.
            return new DbContextOptionsBuilder<TaskContext>().Options;
        }

        [Test]
        public void GetAccountMovementsTest()
        {
            var options = CreateNewContextOptions();

            using (var context = new TaskContext(options))
            {
                context.AccountMovements.AddRange(new[] {
                    new AccountMovementDto { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Amount = 50, Type = "Credit", Date = DateTime.UtcNow },
                    new AccountMovementDto { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Amount = -20, Type = "Debit", Date = DateTime.UtcNow }
                });
                context.SaveChanges();
            }

            using (var context = new TaskContext(options))
            {
                var list = context.AccountMovements.ToList();
                Assert.IsNotNull(list);
                Assert.IsTrue(list.Count >= 2);
            }
        }

        [Test]
        public void CalculateBalanceTest()
        {
            var options = CreateNewContextOptions();

            var userId = Guid.NewGuid();
            using (var context = new TaskContext(options))
            {
                context.AccountMovements.AddRange(new[] {
                    new AccountMovementDto { Id = Guid.NewGuid(), UserId = userId, Amount = 100, Type = "Credit", Date = DateTime.UtcNow },
                    new AccountMovementDto { Id = Guid.NewGuid(), UserId = userId, Amount = -30, Type = "Debit", Date = DateTime.UtcNow }
                });
                context.SaveChanges();
            }

            using (var context = new TaskContext(options))
            {
                var balance = context.AccountMovements.Where(m => m.UserId == userId).Sum(m => m.Amount);
                Assert.AreEqual(70m, balance);
            }
        }

        [Test]
        public void CreateMovementTest()
        {
            var options = CreateNewContextOptions();

            var movement = new AccountMovementDto
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Amount = 100m,
                Type = "Credit",
                Date = DateTime.UtcNow,
                Description = "Test movement"
            };

            using (var context = new TaskContext(options))
            {
                context.AccountMovements.Add(movement);
                context.SaveChanges();
            }

            using (var context = new TaskContext(options))
            {
                var saved = context.AccountMovements.Find(movement.Id);
                Assert.IsNotNull(saved);
                Assert.AreEqual(100m, saved.Amount);
            }
        }
    }
}