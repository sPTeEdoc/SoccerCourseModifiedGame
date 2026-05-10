using Godot;

[GlobalClass]
public partial class PlayerResource : Resource
{
    [Export] public string FullName { get; set; }
    [Export] public string SkinColor { get; set; }
    [Export] public PlayerCharacter.Role Role { get; set; }
    [Export] public float Speed { get; set; }
    [Export] public float Power { get; set; }

    public PlayerResource() { }

    public PlayerResource(string name, string skin, PlayerCharacter.Role role, float speed, float power)
    {
        FullName = name;
        SkinColor = skin;
        Role = role;
        Speed = speed;
        Power = power;
    }
}
