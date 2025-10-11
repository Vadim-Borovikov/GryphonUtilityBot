using GryphonUtilityBot.Timeline;

namespace GryphonUtilityBot.Operations.Data;

internal sealed class ConfirmTimelineDeletionData
{
    public readonly int DeleteFrom;
    public readonly int DeleteAmount;

    public static ConfirmTimelineDeletionData? From(string callbackQueryDataCore)
    {
        string[] parts = callbackQueryDataCore.Split(Manager.FieldSeparator);
        if ((parts.Length != 2) || !int.TryParse(parts[0], out int deleteFrom)
                                || !int.TryParse(parts[1], out int deleteAmount))
        {
            return null;
        }
        return new ConfirmTimelineDeletionData(deleteFrom, deleteAmount);
    }

    private ConfirmTimelineDeletionData(int deleteFrom, int deleteAmount)
    {
        DeleteFrom = deleteFrom;
        DeleteAmount = deleteAmount;
    }
}