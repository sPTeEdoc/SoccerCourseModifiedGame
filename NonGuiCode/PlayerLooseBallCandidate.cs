// Example: Add this inside your FunnyOldGame namespace, but outside the Game class
using FunnyOldGame;
public class PlayerLooseBallCandidate
{
    public Player Player { get; set; }
    public double Score { get; set; }

    public PlayerLooseBallCandidate(Player player, double score)
    {
        Player = player;
        Score = score;
    }
}