﻿using AbstractBot;
using AbstractBot.Bots;
using AbstractBot.Configs;
using AbstractBot.Operations.Data;

namespace GryphonUtilityBot;

public sealed class Bot : Bot<Config, Texts, SaveData, CommandDataSimple>
{
    public Bot(Config config) : base(config) { }
}