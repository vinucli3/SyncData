using Microsoft.Extensions.Logging;
using SyncData.Interface.Serializers;
using SyncData.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace SyncData.Notification
{
	internal class ContentPublishNoti : INotificationHandler<ContentSavedNotification>
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public ContentPublishNoti(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}
		public void Handle(ContentSavedNotification notification)
		{
			try
			{
				
				_contentSerialize.Handler();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
			

		}
	}
}
