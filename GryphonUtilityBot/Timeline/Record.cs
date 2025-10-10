using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace GryphonUtilityBot.Timeline;

internal abstract class Record
{
    [UsedImplicitly]
    [Required]
    [SheetField(IdTitle)]
    public int Id;

    [UsedImplicitly]
    [SheetField(GroupIdTitle)]
    public string? GroupId;

    [UsedImplicitly]
    [SheetField(AuthorIdTitle)]
    public long? AuthorId;

    [UsedImplicitly]
    [SheetField(ReplyToIdTitle)]
    public int? ReplyToId;

    protected Record() { }

    protected Record(int id, string? groupId, long? authorId, int? replyToId)
    {
        Id = id;
        GroupId = groupId;
        AuthorId = authorId;
        ReplyToId = replyToId;
    }

    private const string IdTitle = "Id";
    private const string GroupIdTitle = "Группа";
    private const string AuthorIdTitle = "Автор";
    private const string ReplyToIdTitle = "Ответ на";
}