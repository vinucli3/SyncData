using Microsoft.Extensions.DependencyInjection;
using SyncData.Interface.Deserializers;
using SyncData.Interface.Serializers;
using SyncData.Repository.Deserializers;
using SyncData.Repository.Serializers;
using Umbraco.Cms.Core.Composing;

namespace SyncData.Composers
{
	internal class SerialDeserialComposer : IComposer
	{
		public void Compose(IUmbracoBuilder builder)
		{
			//Serialize
			builder.Services.AddScoped<IBlueprintSerialize, BlueprintSerialize>();
			builder.Services.AddScoped<IContentSerialize, ContentSerialize>();
			builder.Services.AddScoped<IDataTypeSerialize, DataTypeSerialize>();
			builder.Services.AddScoped<IDictionarySerialize, DictionarySerialize>();
			builder.Services.AddScoped<IDocTypeSerialize, DocTypeSerialize>();
			builder.Services.AddScoped<IDomainSerialize, DomainSerialize>();
			builder.Services.AddScoped<ILanguageSerialize, LanguageSerialize>();
			builder.Services.AddScoped<IMacroSerialize, MacroSerialize>();
			builder.Services.AddScoped<IMediaSerialize, MediaSerialize>();
			builder.Services.AddScoped<IMediaTypeSerialize, MediaTypeSerialize>();
			builder.Services.AddScoped<IMemberTypeSerialize, MemberTypeSerialize>();
			builder.Services.AddScoped<IRelationSerialize, RelationSerialize>();
			builder.Services.AddScoped<ITemplateSerialize, TemplateSerialize>();
			builder.Services.AddScoped<IMemberSerialize, MemberSerialize>();
			builder.Services.AddScoped<IMemberGroupSerialize, MemberGroupSerialize>();
			builder.Services.AddScoped<IUsersSerialize, UsersSerialize>();
			builder.Services.AddScoped<IUserGroupSerialize, UserGroupSerialize>();
			builder.Services.AddScoped<IPublicAccessSerialize, PublicAccessSerialize>();
			//Deserialize
			builder.Services.AddScoped<IBlueprintDeserialize, BlueprintDeserialize>();
			builder.Services.AddScoped<IContentDeserialize, ContentDeserialize>();
			builder.Services.AddScoped<IDataTypeDeserialize, DataTypeDeserialize>();
			builder.Services.AddScoped<IDictionaryDeserialize, DictionaryDeserialize>();
			builder.Services.AddScoped<IDocTypeDeserialize, DocTypeDeserialize>();
			builder.Services.AddScoped<IDomainDeserialize, DomainDeserialize>();
			builder.Services.AddScoped<ILanguageDeserialize, LanguageDeserialize>();
			builder.Services.AddScoped<IMacroDeserialize, MacroDeserialize>();
			builder.Services.AddScoped<IMediaDeserialize, MediaDeserialize>();
			builder.Services.AddScoped<IMediaTypeDeserialize, MediaTypeDeserialize>();
			builder.Services.AddScoped<IMemberTypeDeserialize, MemberTypeDeserialize>();
			builder.Services.AddScoped<IRelationDeserialize, RelationDeserialize>();
			builder.Services.AddScoped<ITemplateDeserialize, TemplateDeserialize>();
			builder.Services.AddScoped<IMemberDeserialize, MemberDeserialize>();
			builder.Services.AddScoped<IMemberGroupDeserialize, MemberGroupDeserialize>();
			builder.Services.AddScoped<IUsersDeserialize, UsersDeserialize>();
			builder.Services.AddScoped<IUserGroupDeserialize, UserGroupDeserialize>();
		}
	}
}
