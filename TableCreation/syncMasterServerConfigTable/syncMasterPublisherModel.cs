using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncData.TableCreation.syncMasterServerConfigTable
{
    public class syncMasterPublisherModel
    {

        public int Id { get; set; }

        public Guid? Key { get; set; }

        public string? Alias { get; set; }

        public bool? PushEnabled { get; set; }

        public bool? PullEnabled { get; set; }

        public int? SortOrder { get; set; }

        public string? Url { get; set; }

        public string? BaseUrl { get; set; }

        public string? Name { get; set; }

        public string? Icon { get; set; } = "icon-planet color-blue-grey";

        public string? Description { get; set; }

        public string? Message { get; set; }

        public string? Publisher { get; set; } = "realtime";

        public IEnumerable<usyncAllowedServerModel>? AllowedServers { get; set; }

        public usyncSendModel? SendSettings { get; set; }

        public IDictionary<string, bool>? PublisherSettings { get; set; }
    }

    public class usyncAllowedServerModel
    {
        public string? Name { get; set; }

        public string? Icon { get; set; }

        public string? Alias { get; set; }

        public bool? Push { get; set; }

        public bool? Pull { get; set; }
    }

    public class usyncSendModel
    {
        public string? IncludeAncestors { get; set; } = "no";

        public string? IncludeChildren { get; set; } = "user-yes";

        public string? IncludeFiles { get; set; } = "no";

        public string? IncludeMedia { get; set; } = "no";

        public string? IncludeLinked { get; set; } = "no";

        public string? IncludeDependencies { get; set; } = "no";

        public string? IncludeMediaFiles { get; set; } = "no";

        public string? IncludeConfig { get; set; } = "no";

        public string? DeleteMissing { get; set; } = "user-yes";

        public string[]? Groups { get; set; } = new string[] { "admin", "editor" };

        public string? BaseUrl { get; set; }

        public bool? IncludeHost { get; set; } = true;

        public string? Message { get; set; } = "";

        public bool? HideAdvanced { get; set; } = true;
    }
}
