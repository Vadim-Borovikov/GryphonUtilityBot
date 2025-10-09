using GoogleSheetsManager;
using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Timeline;

internal sealed class Record
{
    [UsedImplicitly]
    [Required]
    [SheetField(IdTitle)]
    public int Id;

    [UsedImplicitly]
    [SheetField(DateTitle)]
    public DateOnly? Date;

    [UsedImplicitly]
    [SheetField(GroupIdTitle)]
    public string? GroupId;

    [UsedImplicitly]
    [SheetField(AuthorIdTitle)]
    public long? AuthorId;

    [UsedImplicitly]
    [SheetField(ReplyToIdTitle)]
    public int? ReplyToId;

    public Record() { }

    public Record(int id, DateOnly? date, string? groupId, long? authorId, int? replyToId)
    {
        Id = id;
        Date = date;
        GroupId = groupId;
        AuthorId = authorId;
        ReplyToId = replyToId;
    }

    private const string DateTitle = "Дата";
    private const string IdTitle = "Id";
    private const string GroupIdTitle = "Группа";
    private const string AuthorIdTitle = "Автор";
    private const string ReplyToIdTitle = "Ответ на";
}