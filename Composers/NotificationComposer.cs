using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyncData.Interface;
using SyncData.Notification;
using SyncData.PublishServer;
using SyncData.Repository;
using SyncData.TableCreation.syncMasterServerConfigTable;
using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;

namespace SyncData.Composers
{
    internal class NotificationComposer : IComposer
	{
		
        public void Compose(IUmbracoBuilder builder)
		{
			var conString = builder.Config.GetConnectionString("umbracoDbDSN");
			builder.AddNotificationHandler<ServerVariablesParsingNotification, cSyncServerVariablesHandler>();
			builder.Services.AddScoped<IUpdateContent, UpdateContent>();
			builder.AddNotificationHandler<ContentSavedNotification, ContentPublishNoti>();
			builder.AddNotificationHandler<MenuRenderingNotification, MenuEventHandler>();
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunPublisherModelMigration>();
            builder.Services.AddUmbracoDbContext<ServerContext>(options =>
			{
				options.UseSqlServer(conString);
				//If you are using SQlite, replace UseSqlServer with UseSqlite
			});

            builder.Services.AddUmbracoDbContext<syncMasterPublisherContext>(options =>
            {
                options.UseSqlServer(conString);
                //If you are using SQlite, replace UseSqlServer with UseSqlite
            });
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunServerModelsMigration>();
		}
	}
}
