using Godot;
using System;

public partial class ScreenData : Node
{
    public Tournament Tournament { get; private set; }

    public static ScreenData Build()
    {
        return new ScreenData();
    }

    public ScreenData SetTournament(Tournament contextTournament)
    {
        Tournament = contextTournament;
        return this;
    }
}