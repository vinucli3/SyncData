using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;

namespace SyncData.Composers
{
	internal class MenuEventHandler : INotificationHandler<MenuRenderingNotification>
	{
		private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

		public MenuEventHandler(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
		{
			_backOfficeSecurityAccessor = backOfficeSecurityAccessor;
		}

		public void Handle(MenuRenderingNotification notification)
		{
			// this example will add a custom menu item for all admin users
			if(notification.NodeId == "-20")
			{
				return;
			}			// for all content tree nodes
			if (notification.TreeAlias.Equals("content") &&
				_backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin())
			{
				// Creates a menu action that will open /umbraco/currentSection/itemAlias.html
				var menuItemPubTo = new MenuItem("publishto", "Publish To...");
				if (notification.Menu.Items.Contains(menuItemPubTo))
				{
					notification.Menu.Items.Remove(menuItemPubTo);
				}
					// optional, if you don't want to follow the naming conventions, but do want to use a angular view
					// you can also use a direct path "../App_Plugins/my/long/url/to/view.html"
					menuItemPubTo.AdditionalData.Add("actionView", "/App_Plugins/cSyncMaster/backoffice/syncAlias/publish.html");
				menuItemPubTo.AdditionalData.Add("data", notification.NodeId);
				menuItemPubTo.AdditionalData.Add("userEmail", _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Email);
				menuItemPubTo.AdditionalData["jsAction"] = "PublishDatacontroller.openCreateDialog";
				menuItemPubTo.SeparatorBefore = true;
				// sets the icon to icon-wine-glass
				menuItemPubTo.Icon = "arrow-right";
				// insert at index 5
				notification.Menu.Items.Insert(3, menuItemPubTo);

				//var menuItemPulFrm = new MenuItem("pullfrom", "Pull Content...");

				// optional, if you don't want to follow the naming conventions, but do want to use a angular view
				// you can also use a direct path "../App_Plugins/my/long/url/to/view.html"
				//menuItemPulFrm.AdditionalData.Add("actionView", "/App_Plugins/SyncDashboard/backoffice/syncAlias/publish.html");
				////menuItemPulFrm.AdditionalData.Add("data", notification.NodeId);
				//menuItemPulFrm.AdditionalData["jsAction"] = "PublishDatacontroller.openCreateDialog";
				//menuItemPulFrm.SeparatorBefore = true;
				// sets the icon to icon-wine-glass
				
				//menuItemPulFrm.Icon = "arrow-left";
				// insert at index 5
				//notification.Menu.Items.Insert(3, menuItemPulFrm);
			}
		}
	}
}
