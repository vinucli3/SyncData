using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

namespace SyncData.PublishServer
{
	[PluginController("Publish")]
	public class PublishServerConfigController : UmbracoApiController
	{
		private readonly IEFCoreScopeProvider<ServerContext> _efCoreScopeProvider;

		public PublishServerConfigController(IEFCoreScopeProvider<ServerContext> efCoreScopeProvider)
			=> _efCoreScopeProvider = efCoreScopeProvider;

		[HttpGet]
		public async Task<IActionResult> All()
		{
			using IEfCoreScope<ServerContext> scope = _efCoreScopeProvider.CreateScope();
			IEnumerable<ServerModel> comments = await scope.ExecuteWithContextAsync(async db => db.serverPublishConfig.ToArray());
			scope.Complete();
			return Ok(comments);
		}

		[HttpGet]
		public async Task<IActionResult> GetComments(Guid umbracoNodeKey)
		{
			using IEfCoreScope<ServerContext> scope = _efCoreScopeProvider.CreateScope();
			IEnumerable<ServerModel> comments = await scope.ExecuteWithContextAsync(async db =>
			{
				return db.serverPublishConfig.Where(x => x.Key == umbracoNodeKey).ToArray();
			});
			scope.Complete();
			return Ok(comments);
		}

		[HttpPost]
		public async Task InsertComment(ServerModel comment)
		{
			using IEfCoreScope<ServerContext> scope = _efCoreScopeProvider.CreateScope();

			await scope.ExecuteWithContextAsync<Task>(async db =>
			{
				db.serverPublishConfig.Add(comment);
				await db.SaveChangesAsync();
			});

			scope.Complete();
		}
	}
}
