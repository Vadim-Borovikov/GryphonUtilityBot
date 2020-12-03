using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Web.Models
{
    public sealed class RecordsManager
    {
        internal RecordsManager(Manager saveManager) { _saveManager = saveManager; }

        internal void SaveRecord(Message message)
        {
            _saveManager.Load();

            Record record = GetRecord(message);
            if (record != null)
            {
                _saveManager.Data.Records.Add(record);
            }

            _saveManager.Save();
        }

        private static Record GetRecord(Message message)
        {
            if (!message.ForwardDate.HasValue)
            {
                return null;
            }

            return new Record
            {
                DateTime = message.ForwardDate.Value,
                MessageId = message.MessageId,
                Type = message.Type,
                AuthorId = message.ForwardFrom.Id
            };
        }

        private readonly Manager _saveManager;
    }
}
