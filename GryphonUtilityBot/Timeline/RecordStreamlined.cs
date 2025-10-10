using GoogleSheetsManager;
using JetBrains.Annotations;
using System;

namespace GryphonUtilityBot.Timeline;

internal sealed class RecordStreamlined : Record, IComparable<RecordStreamlined>
{
    [UsedImplicitly]
    [SheetField(DateTitle)]
    public DateOnly Date;

    public RecordStreamlined() { }

    public RecordStreamlined(Record data, DateOnly date, string? groupId, int? id = null, int? replyToId = null)
        : base(id ?? data.Id, groupId ?? data.GroupId, data.AuthorId, replyToId ?? data.ReplyToId)
    {
        Date = date;
    }

    public int CompareTo(RecordStreamlined? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return -1;
        }

        int datesCompare = Date.CompareTo(other.Date);
        return datesCompare != 0 ? datesCompare : Id.CompareTo(other.Id);
    }

    private const string DateTitle = "Дата";
}