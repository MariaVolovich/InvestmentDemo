using InvestmentPortfolio.Plugins.Common.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace InvestmentPortfolio.Plugins.Plugins.Investment
{
    public class PreventConfirmedInvestmentDeletePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context =
                (IPluginExecutionContext)serviceProvider
                    .GetService(typeof(IPluginExecutionContext));

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider
                    .GetService(typeof(IOrganizationServiceFactory));

            var service =
                serviceFactory.CreateOrganizationService(context.UserId);

            var tracing =
                (ITracingService)serviceProvider
                    .GetService(typeof(ITracingService));

            tracing.Trace(
                "=== PreventConfirmedInvestmentDeletePlugin START ==="
            );

            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is EntityReference investmentRef))
            {
                throw new InvalidPluginExecutionException(
                    "Target parameter missing or invalid."
                );
            }

            tracing.Trace($"Target ID: {investmentRef.Id}");

            var investment =
                service.Retrieve(
                    "mv_investment",
                    investmentRef.Id,
                    new ColumnSet("mv_lifecycle")
                );

            var lifecycle =
                investment.GetAttributeValue<OptionSetValue>(
                    "mv_lifecycle"
                )?.Value;

            tracing.Trace($"Lifecycle: {lifecycle}");

            if (lifecycle == InvestmentStatus.Confirmed)
            {
                throw new InvalidPluginExecutionException(
                    "Confirmed investments cannot be deleted."
                );
            }

            tracing.Trace(
                "=== PreventConfirmedInvestmentDeletePlugin END ==="
            );
        }
    }
}