using System.Reflection;

using ECommercePlatform.Application.Interfaces;
using ECommercePlatform.Data;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace ECommercePlatform.Tests
{
    public class TestDbContext : MessageDbContext
    {
        public TestDbContext(
            DbContextOptions<TestDbContext> options,
            IDomainEventDispatcher dispatcher)
            : base(options, dispatcher)
        {
        }

        public DbSet<TestAggregate> TestAggregates { get; set; }

        protected override Assembly ConfigurationsAssembly => Assembly.GetExecutingAssembly();

        public static TestDbContext Create(IDomainEventDispatcher? dispatcher = null)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dispatcher ??= new Mock<IDomainEventDispatcher>().Object;

            var context = new TestDbContext(options, dispatcher);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
