using InvestmentPortfolio.Plugins.Common;
using InvestmentPortfolio.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;

namespace InvestmentPortfolio.Plugins.Plugins.Investment
{
    public class PreventConfirmedInvestmentUpdatePlugin : PluginBase
    {
        private static readonly string[] ProtectedFields =
        {
            "mv_amount",
            "mv_investor",
            "mv_investmentopportunity",
            "mv_date"
        };

        protected override void ExecutePluginLogic(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracing)
        {            

            tracing.Trace(
                "=== PreventConfirmedInvestmentUpdatePlugin START ==="
            );

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity target))
            {
                return;
            }

            if (!context.PreEntityImages.Contains("PreImage"))
            {
                tracing.Trace("PreImage missing.");
                return;
            }

            var preImage =
                context.PreEntityImages["PreImage"];

            var lifecycle =
                preImage.GetAttributeValue<OptionSetValue>(
                    "mv_lifecycle"
                )?.Value;

            tracing.Trace($"Lifecycle: {lifecycle}");

            if (lifecycle != InvestmentStatus.Confirmed)
            {
                tracing.Trace(
                    "Investment is not Confirmed."
                );

                return;
            }

            // Confirmed investments are locked to preserve approved business data.
            // Users must return the record to Draft before changing core investment details.
            foreach (var field in ProtectedFields)
            {
                if (target.Contains(field))
                {
                    tracing.Trace(
                        $"Blocked field update: {field}"
                    );

                    throw new InvalidPluginExecutionException(
                        "This investment is Confirmed and cannot be edited.\r\nReturn it to Draft before modifying investment details."
                    );
                }
            }

            tracing.Trace(
                "=== PreventConfirmedInvestmentUpdatePlugin END ==="
            );
        }
    }
}