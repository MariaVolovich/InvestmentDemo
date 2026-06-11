using InvestmentPortfolio.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;

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

    private decimal GetConfirmedInvestmentsTotal(
        string lookupField,
        Guid relatedId)
    {
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

        if (result.Entities.Count == 0 ||
            !result.Entities[0].Contains("total"))
        {
            return 0;
        }

        var aliasedValue =
            (AliasedValue)result.Entities[0]["total"];

        return aliasedValue.Value is Money money
            ? money.Value
            : 0;
    }

    public void UpdateInvestorTotalInvested(
    Entity investment)
    {       

        var investorRef =
            investment.GetAttributeValue<EntityReference>(
                "mv_investor");

        if (investorRef == null)
            return;

        _tracing.Trace(
            $"Updating Investor totals for {investorRef.Id}");

        var total =
            GetConfirmedInvestmentsTotal(
                "mv_investor",
                investorRef.Id);

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

    public void UpdateOpportunityTotalRaised(
    Entity investment)
    {        

        var opportunityRef =
            investment.GetAttributeValue<EntityReference>(
                "mv_investmentopportunity");

        if (opportunityRef == null)
            return;

        _tracing.Trace(
            $"Updating Opportunity totals for {opportunityRef.Id}");

        var total =
            GetConfirmedInvestmentsTotal(
                "mv_investmentopportunity",
                opportunityRef.Id);

        var update =
            new Entity("mv_investmentopportunity")
            {
                Id = opportunityRef.Id
            };

        update["mv_totalconfirmedraised"] =
            new Money(total);

        _service.Update(update);

        _tracing.Trace(
            $"Opportunity total updated to {total}");
    }
}