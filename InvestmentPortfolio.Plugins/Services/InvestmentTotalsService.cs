using InvestmentPortfolio.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace InvestmentPortfolio.Plugins.Services
{
    public class InvestmentTotalsService
    {

        private readonly IOrganizationService _service;
        private readonly ITracingService _tracing;

        public InvestmentTotalsService(
            IOrganizationService service,
            ITracingService tracing)
        {
            _service = service;
            _tracing = tracing;
        }

        public void UpdateOpportunityTotalRaised(
            Entity investment)
        {
            if (!investment.Contains("mv_investmentopportunity"))
                return;

            var opportunityRef =
                investment.GetAttributeValue<EntityReference>(
                    "mv_investmentopportunity"
                );

            _tracing.Trace(
                $"Updating Opportunity totals for {opportunityRef.Id}");

            var fetch = $@"
            <fetch aggregate='true'>
                <entity name='mv_investment'>
                    <attribute name='mv_amount'
                               alias='total'
                               aggregate='sum' />

                    <filter>
                        <condition attribute='mv_investmentopportunity'
                                   operator='eq'
                                   value='{opportunityRef.Id}' />

                        <condition attribute='mv_lifecycle'
                                   operator='eq'
                                   value='{InvestmentStatus.Confirmed}' />
                    </filter>
                </entity>
            </fetch>";

            var result =
                _service.RetrieveMultiple(
                    new FetchExpression(fetch));

            decimal total = 0;

            if (result.Entities.Count > 0 &&
                result.Entities[0].Contains("total"))
            {
                var aliasedValue =
                    (AliasedValue)result.Entities[0]["total"];

                if (aliasedValue.Value != null)
                    total = ((Money)aliasedValue.Value).Value;
            }

            var update = new Entity("mv_investmentopportunity")
            {
                Id = opportunityRef.Id
            };

            update["mv_totalconfirmedraised"] =
                new Money(total);

            _service.Update(update);

            _tracing.Trace(
                $"Opportunity total updated to {total}");
        }

        public void UpdateInvestorTotalInvested(
            Entity investment)
        {
            if (!investment.Contains("mv_investor"))
                return;

            var investorRef =
                investment.GetAttributeValue<EntityReference>(
                    "mv_investor"
                );

            _tracing.Trace(
                $"Updating Investor totals for {investorRef.Id}");

            var fetch = $@"
            <fetch aggregate='true'>
                <entity name='mv_investment'>
                    <attribute name='mv_amount'
                               alias='total'
                               aggregate='sum' />

                    <filter>
                        <condition attribute='mv_investor'
                                   operator='eq'
                                   value='{investorRef.Id}' />

                        <condition attribute='mv_lifecycle'
                                   operator='eq'
                                   value='{InvestmentStatus.Confirmed}' />
                    </filter>
                </entity>
            </fetch>";

            var result =
                _service.RetrieveMultiple(
                    new FetchExpression(fetch));

            decimal total = 0;

            if (result.Entities.Count > 0 &&
                result.Entities[0].Contains("total"))
            {
                var aliasedValue =
                    (AliasedValue)result.Entities[0]["total"];

                if (aliasedValue.Value != null)
                    total = ((Money)aliasedValue.Value).Value;
            }

            var update = new Entity("mv_investor")
            {
                Id = investorRef.Id
            };

            update["mv_totalinvested"] =
                new Money(total);

            _service.Update(update);

            _tracing.Trace(
                $"Investor total updated to {total}");
        }
    }
}