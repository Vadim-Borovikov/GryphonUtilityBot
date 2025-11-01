using GoogleSheetsManager;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using GryphonUtilities.Time;

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

    [UsedImplicitly]
    [Required]
    [SheetField(AddedTitle, "{0:d MMMM yyyy HH:mm:ss}")]
    public DateTimeFull Added;

    protected Record() { }

    protected Record(int id, DateTimeFull added, string? groupId, long? authorId, int? replyToId)
    {
        Id = id;
        Added = added;
        GroupId = groupId;
        AuthorId = authorId;
        ReplyToId = replyToId;
    }

    private const string IdTitle = "Id";
    private const string GroupIdTitle = "Группа";
    private const string AuthorIdTitle = "Автор";
    private const string ReplyToIdTitle = "Ответ на";
    private const string AddedTitle = "Добавлено";
}