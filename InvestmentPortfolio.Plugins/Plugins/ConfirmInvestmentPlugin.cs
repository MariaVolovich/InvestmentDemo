using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using InvestmentPortfolio.Plugins.Common;
using InvestmentPortfolio.Plugins.Common.Constants;

namespace InvestmentPortfolio.Plugins.Plugins.Investment
{
    public class ConfirmInvestmentPlugin : PluginBase
    {     
        private const decimal MinimumAmount = 1000m;

        protected override void ExecutePluginLogic(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracing)
        {            

            var totalsService =
                new InvestmentTotalsService(
                service,
                tracing);

            tracing.Trace("=== ConfirmInvestmentPlugin START ===");
            tracing.Trace($"Message: {context.MessageName}");
            tracing.Trace($"Depth: {context.Depth}");

            // Validate input
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is EntityReference investmentRef))
            {
                throw new InvalidPluginExecutionException("Target parameter missing or invalid.");
            }

            tracing.Trace($"Target ID: {investmentRef.Id}");

            // Retrieve current DB state
            var investment = service.Retrieve(
                "mv_investment",
                investmentRef.Id,
                new ColumnSet(
                    "mv_lifecycle",
                    "mv_amount",
                    "mv_investor",
                    "mv_investmentopportunity",
                    "mv_date"
                )
            );

            var lifecycle =
                investment.GetAttributeValue<OptionSetValue>("mv_lifecycle")?.Value;

            tracing.Trace($"Lifecycle (DB): {lifecycle}");

            // Safe validation (idempotent)
            if (lifecycle == InvestmentStatus.Confirmed)
            {
                throw new InvalidPluginExecutionException(
                    "Investment is already confirmed."
                );
            }

            // Your business validation
            ValidateInvestment(investment);

            // Apply state change
            var update = new Entity("mv_investment")
            {
                Id = investment.Id
            };

            update["mv_lifecycle"] = new OptionSetValue(InvestmentStatus.Confirmed);

            tracing.Trace("Updating lifecycle to Confirmed...");

            service.Update(update);

            tracing.Trace("Lifecycle updated.");

            // Update related data
            totalsService.UpdateOpportunityTotalRaised(investment);

            tracing.Trace("Opportunity total raised updated.");

            totalsService.UpdateInvestorTotalInvested(investment);

            tracing.Trace("Investor total invested updated.");

            tracing.Trace("=== ConfirmInvestmentPlugin END ===");
        }


        private void ValidateInvestment(Entity investment)
        {
            var lifecycle =
                investment.GetAttributeValue<OptionSetValue>("mv_lifecycle")?.Value;

            if (lifecycle != InvestmentStatus.Draft)
                throw new InvalidPluginExecutionException(
                    "Only Draft investments can be confirmed."
                );

            var amount =
                investment.GetAttributeValue<Money>("mv_amount");

            if (amount == null || amount.Value < MinimumAmount)
                throw new InvalidPluginExecutionException(
                    "Minimum investment amount is 1000."
                );

            if (!investment.Contains("mv_investor"))
                throw new InvalidPluginExecutionException(
                    "Investor is required before confirmation."
                );

            if (!investment.Contains("mv_investmentopportunity"))
                throw new InvalidPluginExecutionException(
                    "Opportunity is required before confirmation."
                );

            if (!investment.Contains("mv_date"))
                throw new InvalidPluginExecutionException(
                    "Investment date is required before confirmation."
                );
        }      
    }
}
