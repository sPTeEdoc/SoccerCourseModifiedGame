using Godot;
using System.Collections.Generic;

public static class KeyUtils
{
    public enum Action { LEFT, RIGHT, UP, DOWN, SHOOT, PASS }
    public static readonly Dictionary<PlayerCharacter.ControlScheme, Dictionary<Action, string>> ACTIONS_MAP =
        new()
        {
            {
                PlayerCharacter.ControlScheme.P1, new Dictionary<Action, string>
                {
                                        { Action.LEFT, "p1_left" },
                    { Action.RIGHT, "p1_right" },
                    { Action.UP, "p1_up" },
                    { Action.DOWN, "p1_down" },
                    { Action.SHOOT, "p1_shoot" },
                    { Action.PASS, "p1_pass" }
                }
            },
            {
                PlayerCharacter.ControlScheme.P2, new Dictionary<Action, string>
                {
                    { Action.LEFT, "p2_left" },
                    { Action.RIGHT, "p2_right" },
                    { Action.UP, "p2_up" },
                    { Action.DOWN, "p2_down" },
                    { Action.SHOOT, "p2_shoot" },
                    { Action.PASS, "p2_pass" }
                }
            }
        };

    public static Vector2 GetInputVector(PlayerCharacter.ControlScheme scheme)
    {
        var map = ACTIONS_MAP[scheme];

        float x = 0f;
        float y = 0f;

        if (Input.IsActionPressed(map[Action.LEFT])) x -= 1f;
        if (Input.IsActionPressed(map[Action.RIGHT])) x += 1f;
        if (Input.IsActionPressed(map[Action.UP])) y -= 1f;
        if (Input.IsActionPressed(map[Action.DOWN])) y += 1f;

        Vector2 v = new Vector2(x, y);
        return v == Vector2.Zero ? Vector2.Zero : v.Normalized();
    }

    public static bool IsActionPressed(PlayerCharacter.ControlScheme scheme, Action action)
    {
        return Input.IsActionPressed(ACTIONS_MAP[scheme][action]);
    }

    public static bool IsActionJustPressed(PlayerCharacter.ControlScheme scheme, Action action)
    {
        return Input.IsActionJustPressed(ACTIONS_MAP[scheme][action]);
    }

    public static bool IsActionJustReleased(PlayerCharacter.ControlScheme scheme, Action action)
    {
        return Input.IsActionJustReleased(ACTIONS_MAP[scheme][action]);
    }
}