using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using InvestmentPortfolio.Plugins.Common;
using InvestmentPortfolio.Plugins.Common.Constants;

namespace InvestmentPortfolio.Plugins.Plugins.Investment
{
    public class ReturnInvestmentToDraftPlugin : PluginBase
    {
        protected override void ExecutePluginLogic(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracing)
        {                   

            var totalsService =
                new InvestmentTotalsService(service, tracing);

            tracing.Trace("=== ReturnInvestmentToDraftPlugin START ===");

            // Validate input
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is EntityReference investmentRef))
            {
                throw new InvalidPluginExecutionException(
                    "Target parameter missing or invalid."
                );
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
                    "mv_investmentopportunity"
                )
            );

            var lifecycle =
                investment.GetAttributeValue<OptionSetValue>(
                    "mv_lifecycle"
                )?.Value;

            tracing.Trace($"Lifecycle (DB): {lifecycle}");

            // Validate lifecycle
            if (lifecycle != InvestmentStatus.Confirmed)
            {
                throw new InvalidPluginExecutionException(
                    "Only confirmed investments can be returned to Draft."
                );
            }

            // Apply lifecycle change
            var update = new Entity("mv_investment")
            {
                Id = investment.Id
            };

            update["mv_lifecycle"] =
                new OptionSetValue(InvestmentStatus.Draft);

            tracing.Trace("Updating lifecycle to Draft...");

            service.Update(update);

            tracing.Trace("Lifecycle updated.");

            // Recalculate related totals
            totalsService.UpdateOpportunityTotalRaised(investment);

            tracing.Trace("Opportunity total raised updated.");

            totalsService.UpdateInvestorTotalInvested(investment);

            tracing.Trace("Investor total invested updated.");

            tracing.Trace("=== ReturnInvestmentToDraftPlugin END ===");
        }
       
    }
}