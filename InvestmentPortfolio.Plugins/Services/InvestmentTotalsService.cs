using InvestmentPortfolio.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

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

        public void UpdateInvestorTotalInvested(
            Entity investment)
        {
            UpdateConfirmedInvestmentTotal(
                investment,
                sourceLookupField: "mv_investor",
                targetEntityName: "mv_investor",
                targetTotalField: "mv_totalinvested",
                traceLabel: "Investor");
        }

        public void UpdateOpportunityTotalRaised(
            Entity investment)
        {
            UpdateConfirmedInvestmentTotal(
                investment,
                sourceLookupField: "mv_investmentopportunity",
                targetEntityName: "mv_investmentopportunity",
                targetTotalField: "mv_totalconfirmedraised",
                traceLabel: "Opportunity");
        }

        private void UpdateConfirmedInvestmentTotal(
            Entity investment,
            string sourceLookupField,
            string targetEntityName,
            string targetTotalField,
            string traceLabel)
        {
            var parentRef =
                investment.GetAttributeValue<EntityReference>(
                    sourceLookupField);

            if (parentRef == null)
                return;

            _tracing.Trace(
                $"Updating {traceLabel} totals for {parentRef.Id}");

            var total =
                GetConfirmedInvestmentsTotal(
                    sourceLookupField,
                    parentRef.Id);

            UpdateMoneyField(
                targetEntityName,
                parentRef.Id,
                targetTotalField,
                total);

            _tracing.Trace(
                $"{traceLabel} total updated to {total}");
        }

        private decimal GetConfirmedInvestmentsTotal(
            string lookupField,
            Guid relatedId)
        {
            // Totals are calculated from confirmed investments only, so Draft records are excluded.
            var fetch = $@"
            <fetch aggregate='true'>
                <entity name='mv_investment'>
                    <attribute name='mv_amount'
                               alias='total'
                               aggregate='sum' />

                    <filter>
                        <condition attribute='{lookupField}'
                                   operator='eq'
                                   value='{relatedId}' />

                        <condition attribute='mv_lifecycle'
                                   operator='eq'
                                   value='{InvestmentStatus.Confirmed}' />
                    </filter>
                </entity>
            </fetch>";

            var result =
                _service.RetrieveMultiple(
                    new FetchExpression(fetch));

            return ExtractMoneyAggregate(
                result,
                "total");
        }

        private static decimal ExtractMoneyAggregate(
            EntityCollection result,
            string alias)
        {
            if (result.Entities.Count == 0 ||
                !result.Entities[0].Contains(alias))
            {
                return 0;
            }

            var aliasedValue =
                result.Entities[0].GetAttributeValue<AliasedValue>(
                    alias);

            return aliasedValue?.Value is Money money
                ? money.Value
                : 0;
        }

        private void UpdateMoneyField(
            string entityName,
            Guid recordId,
            string moneyField,
            decimal value)
        {
            var update =
                new Entity(entityName)
                {
                    Id = recordId
                };

            update[moneyField] =
                new Money(value);

            _service.Update(update);
        }
    }
}