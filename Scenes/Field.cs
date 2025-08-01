using FunnyOldGame;
using Godot;
using System;
using System.Diagnostics;

// Make sure this script is attached to your Field's root Node2D
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]

public partial class Field : Node2D
{
    public Field()
    {
    }

    // A PackedScene is Godot's way of representing a saved scene file.
    // Use [Export] to make this variable appear in the Godot editor's Inspector

    [Export] public PackedScene PlayerScene { get; set; }

    public override void _Ready()
    {
        // Check if the PlayerScene PackedScene has been assigned in the editor.
        // It's good practice to ensure it's not null before trying to use it.
        if (PlayerScene == null)
        {GD.PrintErr("PlayerScene PackedScene not assigned in the editor!");
            return;
        }

        // --- Step 1 & 2: Instantiate the Player Scene ---
        // Create an instance of the Player scene.
        // The type returned by Instantiate() is a Godot.Node.
        // You'll need to cast it to your Player's specific script type (e.g., Player.cs)
        // if you want to access methods/properties defined in your Player script.
        PlayerCharacter playerInstance = PlayerScene.Instantiate<PlayerCharacter>(); 
        // Or if Player.cs is just a CharacterBody2D directly:
        // CharacterBody2D playerInstance = PlayerScene.Instantiate<CharacterBody2D>();


        // --- Step 3: Add the Player to the Scene Tree ---
        // Add the new player instance as a child of the current node (the Field's Node2D).
        // This makes the player part of the active scene.
        AddChild(playerInstance);

        // --- Step 4: Set the Player's Position ---
        // Now that the player is in the scene tree, you can set its position.
        // Let's place it at a specific coordinate, e.g., (200, 150) pixels.
        playerInstance.Position = new Vector2(200, 150);

        // You can place multiple players:
        PlayerCharacter playerInstance2 = PlayerScene.Instantiate<PlayerCharacter>();
        AddChild(playerInstance2);
        playerInstance2.Position = new Vector2(400, 300);

        GD.Print("Players instantiated and placed on the field!");
    }

    // You could also create a method to spawn a player at a given zone
    public PlayerCharacter SpawnPlayerAtZone(Enums.PitchZone zone, Vector2 offset = new Vector2())
    {
        if (PlayerScene == null)
        {
            GD.PrintErr("PlayerScene PackedScene not assigned for SpawnPlayerAtZone!");
            return null;
        }

        PlayerCharacter playerInstance = PlayerScene.Instantiate<PlayerCharacter>();
        AddChild(playerInstance);

        // Assuming you have a method to convert your enum PitchZone to a Godot Vector2 position
        Vector2 zoneCenterPosition = GetPositionForPitchZone(zone); 
        playerInstance.Position = zoneCenterPosition + offset; // Add an offset for variety if desired

        return playerInstance;
    }

    // You'll need to implement this method based on your zone definitions
    private Vector2 GetPositionForPitchZone(Enums.PitchZone zone)
    {
        // This is where your logic for mapping zones to actual pixel coordinates goes.
        // Example (you'll have a much more complete mapping):
        switch (zone)
        {
            // case Enums.PitchZone.HomeGoal: return new Vector2(50, 400); // Example for a home goal zone
            // case Enums.PitchZone.HomeDefense_Left: return new Vector2(150, 200);
            case Enums.PitchZone.Midfield_Center: return new Vector2(500, 400);
            // ... add all your zones
            default: return new Vector2(0, 0); // Default or throw an error for unmapped zones
        }
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

}