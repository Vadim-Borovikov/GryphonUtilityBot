using System.Collections.Generic;
using System.Threading.Tasks;

namespace GryphonUtilityBot.Web.Models.Calendar;

public interface IUpdatesSubscriber
{
    Task OnCreatedAsync(string id);
    Task OnPropertiesUpdatedAsync(string id, List<string> properties);
    Task OnMovedAsync(string id, string parentId);
    Task OnDeletedAsync(string id);
    Task OnUndeletedAsync(string id);
}