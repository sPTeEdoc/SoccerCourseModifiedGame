using System.Globalization;
using Godot;

[GlobalClass]
public partial class PlayerResource : Resource
{
    [Export] public int PlayerID { get; set; }
    [Export] public string FullName { get; set; }
    [Export] public string SkinColor { get; set; }
    [Export] public string HairColor { get; set; }
    [Export] public PlayerCharacter.Role Role { get; set; }
    [Export] public int Number { get; set; }
    [Export] public float Pass { get; set; }
    [Export] public float Control { get; set; }
    [Export] public float Speed { get; set; }
    [Export] public float Power { get; set; }
    [Export] public int TeamID { get; set; }

    public PlayerResource() { }

    public PlayerResource(int id, string name, string skin, string hairColor, PlayerCharacter.Role role, int number, float pass, float control, float speed, float power,
        int teamID)
    {
        PlayerID = id;
        FullName = name;
        SkinColor = skin;
        HairColor = hairColor;
        Role = role;
        Speed = speed;
        Power = power;
        Number = number;
        Pass = pass;
        Control = control;
        TeamID = teamID;
    }
}
