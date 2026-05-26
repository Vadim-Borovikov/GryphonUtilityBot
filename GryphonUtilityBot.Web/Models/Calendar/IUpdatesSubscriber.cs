using System.Threading.Tasks;
using GryphonUtilityBot.Web.Models.Calendar.Notion.Updates;

namespace GryphonUtilityBot.Web.Models.Calendar;

public interface IUpdatesSubscriber
{
    Task ProcessAsync(Update update);
}