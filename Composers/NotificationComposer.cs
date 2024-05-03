
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SyncData.Interface;
using SyncData.Notification;
using SyncData.PublishServer;
using SyncData.Repository;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.ApplicationBuilder;


namespace SyncData.Composers
{
	public class NotificationComposer : IComposer
	{

		public const string AllowAnyOriginPolicyName = nameof(AllowAnyOriginPolicyName);
		public void Compose(IUmbracoBuilder builder)
		{
			var conString = builder.Config.GetConnectionString("umbracoDbDSN");

			builder.AddNotificationHandler<ServerVariablesParsingNotification, cSyncServerVariablesHandler>();
			builder.Services.AddScoped<IUpdateContent, UpdateContent>();
			builder.AddNotificationHandler<ContentPublishedNotification, ContentPublish>();
			builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, TrashContent>();
			builder.AddNotificationHandler<ContentSavedNotification, RestoreContent>();
			builder.AddNotificationHandler<ContentMovedNotification, MovedContent>();
			builder.AddNotificationHandler<ContentDeletedNotification, DeleteContent>();
			builder.AddNotificationHandler<MenuRenderingNotification, MenuEventHandler>();
			builder.Services.AddUmbracoDbContext<ServerContext>(options =>
			{
				options.UseSqlServer(conString);
				//If you are using SQlite, replace UseSqlServer with UseSqlite
			});

			//builder.Services.AddCors(options =>
			//{
			//	options.AddPolicy(AllowAnyOriginPolicyName, policy =>
			//	{
			//		policy.AllowAnyOrigin()
			//				.AllowAnyHeader()
			//				.AllowAnyMethod();
			//	});
			//}).Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("Cors", postRouting: app => app.UseCors())));
			builder.Services.Configure<StaticFileOptions>(Options =>
			{
				Options.OnPrepareResponse = ctx =>
				{
					ctx.Context.Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, "*");
				};
			});

			builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunServerModelsMigration>();
		}
	}

}
