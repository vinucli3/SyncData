using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SyncData.Controllers;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;

namespace SyncData.Composers
{
	internal class cSyncServerVariablesHandler : INotificationHandler<ServerVariablesParsingNotification>, INotificationHandler
	{
		private readonly LinkGenerator _linkGenerator;
		private readonly IBackOfficeSecurityAccessor _securityAccessor;
        public cSyncServerVariablesHandler(LinkGenerator linkGenerator, IBackOfficeSecurityAccessor securityAccessor)
        {
			this._linkGenerator = linkGenerator;
			this._securityAccessor = securityAccessor;
		}
		public void Handle(ServerVariablesParsingNotification notification)
		{
			var serverVariables= notification.ServerVariables;
			var umrabcoUrlsObject = serverVariables["umbracoUrls"];
			if(umrabcoUrlsObject == null )
			{
				throw new ArgumentException("Null UmrbacoUrls");
			}

			if(!(umrabcoUrlsObject is Dictionary<string, object> umbracoUrls))
			{
				throw new ArgumentException("Invalid umbracoUrls");
			}
			var publish = _linkGenerator.GetUmbracoApiServiceBaseUrl<PublishController>(x => x.HeartBeat(""));
			var export = _linkGenerator.GetUmbracoApiServiceBaseUrl<ExportController>(x => x.HeartBeat());
			var import = _linkGenerator.GetUmbracoApiServiceBaseUrl<ImportController>(x => x.HeartBeat());
			//serverVariables.Add("uSyncHistory", (object)mylink);

			umbracoUrls["publishBaseUrl"] = publish;
			umbracoUrls["exportBaseUrl"] = export;
			umbracoUrls["importBaseUrl"] = import;
		}
	}
}