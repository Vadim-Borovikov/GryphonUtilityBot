using GoogleSheetsManager;
using JetBrains.Annotations;
using System;

namespace GryphonUtilityBot.Timeline;

internal class RecordInput : Record, IComparable<RecordInput>
{
    [UsedImplicitly]
    [SheetField(TextDateTitle)]
    public DateOnly? TextDate;

    public RecordInput() { }

    public RecordInput(int id, DateOnly? textDate, string? groupId, long? authorId, int? replyToId)
    {
        Id = id;
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