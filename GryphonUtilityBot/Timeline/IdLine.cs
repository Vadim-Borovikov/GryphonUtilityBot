using System.ComponentModel.DataAnnotations;
using GoogleSheetsManager;
using JetBrains.Annotations;

namespace GryphonUtilityBot.Timeline;

internal sealed class IdLine
{
    [UsedImplicitly]
    [SheetField]
    [Required]
    public int Id;

    [UsedImplicitly]
    [SheetField]
    [Required]
    public int InputId;
    public IdLine() { }

    public IdLine(int id, int inputId)
    {
        Id = id;
        InputId = inputId;
    }
}