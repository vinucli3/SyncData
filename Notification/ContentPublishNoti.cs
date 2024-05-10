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
	public class ContentPublish : INotificationHandler<ContentPublishedNotification>
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public ContentPublish(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}

		public void Handle(ContentPublishedNotification notification)
		{
			try
			{
				
				_contentSerialize.HandlerAsync();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
		}
	}
	public class TrashContent : INotificationHandler<ContentMovedToRecycleBinNotification>
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public TrashContent(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}

		public void Handle(ContentMovedToRecycleBinNotification notification)
		{
			try
			{
				_contentSerialize.HandlerAsync();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
		}
	}

	public class RestoreContent : INotificationHandler<ContentSavedNotification>
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public RestoreContent(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}
		public void Handle(ContentSavedNotification notification)
		{
			try
			{
				_contentSerialize.HandlerAsync();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
		}
	}

	public class MovedContent : INotificationHandler<ContentMovedNotification> // restore
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public MovedContent(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}

		public void Handle(ContentMovedNotification notification)
		{
			try
			{
				_contentSerialize.HandlerAsync();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
		}
	}

	public class DeleteContent : INotificationHandler<ContentDeletedNotification> // restore
	{
		private IContentSerialize _contentSerialize;
		private readonly ILogger<UpdateContent> _logger;
		public DeleteContent(IContentSerialize contentSerialize, ILogger<UpdateContent> logger)
		{
			_contentSerialize = contentSerialize;
			_logger = logger;
		}
		public void Handle(ContentDeletedNotification notification)
		{
			try
			{
				Array.ForEach(Directory.GetFiles("cSync\\Content\\"), File.Delete);
				_contentSerialize.HandlerAsync();
				_logger.LogInformation("Export Content Complete");
			}
			catch (Exception ex)
			{
				_logger.LogError("Export error when save {ex}", ex);
			}
		}
	}
}
