using GoogleSheetsManager;
using JetBrains.Annotations;
using System;
using GryphonUtilities.Time;

namespace GryphonUtilityBot.Timeline;

internal sealed class RecordInput : Record, IComparable<RecordInput>
{
    [UsedImplicitly]
    [SheetField(TextDateTitle)]
    public DateOnly? TextDate;

    public RecordInput() { }

    public RecordInput(int id, DateTimeFull added, DateOnly? textDate, string? groupId, long? authorId, int? replyToId)
    {
        Id = id;
        Added = added;
        TextDate = textDate;
        GroupId = groupId;
        AuthorId = authorId;
        ReplyToId = replyToId;
    }

    public int CompareTo(RecordInput? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return -1;
        }

        return Id.CompareTo(other.Id);
    }

    private const string TextDateTitle = "Дата";
}