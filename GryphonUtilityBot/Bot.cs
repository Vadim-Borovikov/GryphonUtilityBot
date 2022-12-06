using AbstractBot;

namespace GryphonUtilityBot;

public sealed class Bot : BotBaseCustom<Config>
{
    public Bot(Config config) : base(config) { }
}