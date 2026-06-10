using InvestmentPortfolio.Plugins.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace InvestmentPortfolio.Tests
{
    [TestClass]
    public class AmountUpdateForOpportunityTest
    {
        [TestMethod]
        public void UpdateOpportunityTotalRaised_ShouldUpdateOpportunityTotal_WhenAggregateReturnsValue()
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

            var opportunityId =
                Guid.NewGuid();

            var investment =
                new Entity("mv_investment");

            investment["mv_investmentopportunity"] =
                new EntityReference(
                    "mv_investmentopportunity",
                    opportunityId);

            var aggregateResult =
                new Entity("mv_investment");

            aggregateResult["total"] =
                new AliasedValue(
                    "mv_investment",
                    "mv_amount",
                    new Money(500));

            var collection =
                new EntityCollection();

            collection.Entities.Add(
                aggregateResult);

            serviceMock
                .Setup(s =>
                    s.RetrieveMultiple(
                        It.IsAny<FetchExpression>()))
                .Returns(collection);

            // Act

            totalsService.UpdateOpportunityTotalRaised(
                investment);

            // Assert

            serviceMock.Verify(
                s => s.Update(
                    It.Is<Entity>(e =>
                        e.LogicalName ==
                            "mv_investmentopportunity"
                        &&
                        e.Id == opportunityId
                        &&
                        ((Money)e[
                            "mv_totalconfirmedraised"])
                            .Value == 500
                    )),
                Times.Once);
        }

        [TestMethod]
        public void UpdateInvestorTotalInvested_ShouldUpdateInvestorTotal_WhenAggregateReturnsValue()
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

            var investorId =
                Guid.NewGuid();

            var investment =
                new Entity("mv_investment");

            investment["mv_investor"] =
                new EntityReference(
                    "mv_investor",
                    investorId);

            var aggregateResult =
                new Entity("mv_investment");

            aggregateResult["total"] =
                new AliasedValue(
                    "mv_investment",
                    "mv_amount",
                    new Money(760102));

            var collection =
                new EntityCollection();

            collection.Entities.Add(
                aggregateResult);

            serviceMock
                .Setup(s =>
                    s.RetrieveMultiple(
                        It.IsAny<FetchExpression>()))
                .Returns(collection);

            // Act

            totalsService.UpdateInvestorTotalInvested(
                investment);

            // Assert

            serviceMock.Verify(
                s => s.Update(
                    It.Is<Entity>(e =>
                        e.LogicalName ==
                            "mv_investor"
                        &&
                        e.Id == investorId
                        &&
                        ((Money)e[
                            "mv_totalinvested"])
                            .Value == 760102
                    )),
                Times.Once);
        }
}
}