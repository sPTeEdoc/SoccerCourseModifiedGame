using Godot;
using System;

public static class PlayerOverallCalculator
{
    public static double CalculateOverall(PlayerCharacter.OnFieldPositions position, PlayerResource player)
    {
        int baseNumberOfAttributes = 2;
        double overall = player.TrueAttributes.Awareness + player.TrueAttributes.Athleticism;
        if (position == PlayerCharacter.OnFieldPositions.DEFENSE)
        {
            overall += player.TrueAttributes.Defense + player.TrueAttributes.Strength + player.TrueAttributes.Toughness;
            baseNumberOfAttributes = 5;
        }
        else if (position == PlayerCharacter.OnFieldPositions.MIDFIELD)
        {
            overall += player.TrueAttributes.Offense + player.TrueAttributes.Defense + player.TrueAttributes.Speed + player.TrueAttributes.Passing + player.TrueAttributes.Dribble;
            baseNumberOfAttributes = 7;
        }
        else if (position == PlayerCharacter.OnFieldPositions.FORWARD)
        {
            overall += player.TrueAttributes.Offense + player.TrueAttributes.Shooting + player.TrueAttributes.Passing + player.TrueAttributes.Dribble;
            baseNumberOfAttributes = 6;
        }
        else if (position == PlayerCharacter.OnFieldPositions.GOALIE)
        {
            overall += player.TrueAttributes.Save + player.TrueAttributes.Reflexes + player.TrueAttributes.Special;
            baseNumberOfAttributes = 5;
        }

        return overall / baseNumberOfAttributes;
    }
}
