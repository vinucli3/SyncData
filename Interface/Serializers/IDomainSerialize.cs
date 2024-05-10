using Umbraco.Cms.Core.Models;

namespace SyncData.Interface.Serializers
{
    public interface IDomainSerialize
    {
		public Task<bool> HandlerAsync();
		public Task<bool> SingleHandlerAsync(IDomain domain);

	}
}