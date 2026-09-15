using System.Globalization;
using Godot;

[GlobalClass]
public partial class PlayerResource : Resource
{
    [Export] public int PlayerID { get; set; }
    [Export] public string SkinColor { get; set; }
    [Export] public string HairColor { get; set; }
    [Export] public PlayerCharacter.OnFieldPositions Position { get; set; }
    [Export] public int Number { get; set; }
    [Export] public int TeamID { get; set; }
    [Export] public bool IsCaptain { get; set; }
    public string PlayerName
    {
        get
        {
            if (!string.IsNullOrEmpty(AltName))
            {
                return AltName;
            }
            else if (!string.IsNullOrEmpty(LastName))
            {
                return LastName;
            }
            return FirstName;
        }
    }
    [Export] public string FirstName { get; set; }
    [Export] public string LastName { get; set; }
    [Export] public string AltName { get; set; }
    public PlayerAttributes TrueAttributes { get; set; } = new PlayerAttributes();
    public PlayerAttributes GameAttributes { get; set; } = new PlayerAttributes();

    public PlayerResource() { }

    public PlayerResource(int id,  string firstName, string lastName, string altName, string skin, string hairColor, PlayerCharacter.OnFieldPositions role, int number, 
        int teamID, int offense,
        int defense, int awareness, int shooting, int passing, int speed, int dribble, int strength,
        int toughness, int athleticism, int popularity, int header, int save, int reflexes,
        int special, bool isCaptain)
    {
        PlayerID = id;
        FirstName = firstName;
        LastName = lastName;
        AltName = altName;
        SkinColor = skin;
        HairColor = hairColor;
        Position = role;
        Number = number;
        TeamID = teamID;
        IsCaptain = isCaptain;

        this.TrueAttributes.Offense = offense;
        this.TrueAttributes.Defense = defense;
        this.TrueAttributes.Awareness = awareness;
        this.TrueAttributes.Shooting = shooting;
        this.TrueAttributes.Passing = passing;
        this.TrueAttributes.Speed = speed;
        this.TrueAttributes.Dribble = dribble;
        this.TrueAttributes.Strength = strength;
        this.TrueAttributes.Toughness = toughness;
        this.TrueAttributes.Athleticism = athleticism;
        this.TrueAttributes.Popularity = popularity;
        this.TrueAttributes.Header = header;
        this.TrueAttributes.Save = save;
        this.TrueAttributes.Reflexes = reflexes;
        this.TrueAttributes.Special = special;

        this. GameAttributes.Offense = offense;
        this. GameAttributes.Defense = defense;
        this. GameAttributes.Awareness = awareness;
        this. GameAttributes.Shooting = shooting;
        this. GameAttributes.Passing = passing;
        this. GameAttributes.Speed = speed;
        this. GameAttributes.Dribble = dribble;
        this. GameAttributes.Strength = strength;
        this. GameAttributes.Toughness = toughness;
        this. GameAttributes.Athleticism = athleticism;
        this. GameAttributes.Popularity = popularity;
        this. GameAttributes.Header = header;
        this. GameAttributes.Save = save;
        this. GameAttributes.Reflexes = reflexes;
        this. GameAttributes.Special = special;
    }
}
