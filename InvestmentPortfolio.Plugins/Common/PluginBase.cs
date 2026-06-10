using Microsoft.Xrm.Sdk;
using System;

namespace InvestmentPortfolio.Plugins.Common
{
    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service =
                factory.CreateOrganizationService(context.UserId);

            try
            {
                ExecutePluginLogic(context, service, tracingService);
            }
            catch (Exception ex)
            {
                tracingService.Trace(ex.ToString());
                throw;
            }
        }

        protected abstract void ExecutePluginLogic(
            IPluginExecutionContext context,
            IOrganizationService service,
            ITracingService tracingService);
    }
}