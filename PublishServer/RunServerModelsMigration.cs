using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SyncData.PublishServer
{
    public class RunServerModelsMigration : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
    {
        private readonly ServerContext _serverContext;

        public RunServerModelsMigration(ServerContext serverContext)
        {
            _serverContext = serverContext;
        }

        public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
        {
            IEnumerable<string> pendingMigrations = await _serverContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await _serverContext.Database.MigrateAsync();
            }
        }
    }
}
