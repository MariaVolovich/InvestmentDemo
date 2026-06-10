using InvestmentPortfolio.Plugins.Services;
using Microsoft.Xrm.Sdk;
using Moq;

namespace InvestmentPortfolio.Tests
{
    [TestClass]
    public sealed class OpportunityCheckTest
    {       
        [TestMethod]
        public void UpdateOpportunityTotalRaised_ShouldNotUpdate_WhenOpportunityIsMissing()
        {
            // Arrange

            var serviceMock =
                new Mock<IOrganizationService>();

            var tracingMock =
                new Mock<ITracingService>();

            var totalsService =
                new InvestmentTotalsService(
                    serviceMock.Object,
                    tracingMock.Object);

            var investment =
                new Entity("mv_investment");

            // Act

            totalsService.UpdateOpportunityTotalRaised(
                investment);

            // Assert

            serviceMock.Verify(
                s => s.Update(It.IsAny<Entity>()),
                Times.Never);
        }
    }
}
