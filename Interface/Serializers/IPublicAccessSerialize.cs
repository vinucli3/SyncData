using Umbraco.Cms.Core.Models;

namespace SyncData.Interface.Serializers
{
    public interface IPublicAccessSerialize
    {
        Task<bool> HandlerAsync();
        Task<bool> SingleHandlerAsync(PublicAccessEntry publicAccessEntry, IContent node);
    }
}