using Godot;
using System;

public partial class Backgrounds : Node2D
{
    public Rect2 PitchBounds { get; private set; }

    public override void _Ready()
    {
        // Find your child Sprite2D (adjust the node path if named differently, e.g., "Background")
        var fieldSprite = GetNode<Sprite2D>("Grass");

        if (fieldSprite != null && fieldSprite.Texture != null)
        {
            // 1. Get raw size in pixels (e.g., 800 x 600)
            Vector2 textureSize = fieldSprite.Texture.GetSize();
            
            // 2. Account for any stretching or scaling applied in the editor
            Vector2 scaledSize = textureSize * fieldSprite.Scale;

            // 3. Account for Centered sprites (Godot defaults to Centered)
            Vector2 topLeftPos;
            if (fieldSprite.Centered)
            {
                topLeftPos = fieldSprite.GlobalPosition - (scaledSize / 2f);
            }
            else
            {
                topLeftPos = fieldSprite.GlobalPosition;
            }

            // 4. Store the final bounding box arena
            PitchBounds = new Rect2(topLeftPos, scaledSize);

            GD.Print($"Pitch Dimensions Calculated! Width: {PitchBounds.Size.X}px, Height: {PitchBounds.Size.Y}px");
            GD.Print($"Top-Left Corner: {PitchBounds.Position}");
        }
    }

    // Quick helper methods to use across your project
    public Vector2 GetCenterSpot() => PitchBounds.Position + (PitchBounds.Size / 2f);
    public float GetLeftBoundary() => PitchBounds.Position.X;
    public float GetRightBoundary() => PitchBounds.End.X; // Position.X + Size.X
}
