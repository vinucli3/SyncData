using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SyncData.TableCreation.syncMasterServerConfigTable
{
    public class RunPublisherModelMigration : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
    {
        private readonly syncMasterPublisherContext _dataContext;

        public RunPublisherModelMigration(syncMasterPublisherContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
        {
            IEnumerable<string> pendingMigrations = await _dataContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                await _dataContext.Database.MigrateAsync(cancellationToken);
            }
        }
    }
}
