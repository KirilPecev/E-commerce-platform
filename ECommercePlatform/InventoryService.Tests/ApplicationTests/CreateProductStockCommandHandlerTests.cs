using ECommercePlatform.Application.Interfaces;

using Moq;

namespace CatalogService.Tests.ApplicationTests
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IEventPublisher> eventPublisherMock;

        public CreateProductCommandHandlerTests()
        {
            eventPublisherMock = new Mock<IEventPublisher>();
        }
    }
}
