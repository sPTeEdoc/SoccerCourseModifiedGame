using Godot;
using System.Collections.Generic;

public static class KeyUtils
{
    public enum Action { LEFT, RIGHT, UP, DOWN, BButton, AButton, R1Button, YButton, XButton, L1Button }

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
                    { Action.BButton, "B" },
                    { Action.AButton, "A" },
                    { Action.R1Button, "R1"},
                    { Action.YButton, "Y" },
                    { Action.XButton, "X" },
                    { Action.L1Button, "L1"},
                }
            },
            {
                PlayerCharacter.ControlScheme.P2, new Dictionary<Action, string>
                {
                    { Action.LEFT, "p2_left" },
                    { Action.RIGHT, "p2_right" },
                    { Action.UP, "p2_up" },
                    { Action.DOWN, "p2_down" },
                    { Action.BButton, "p2_shoot" },
                    { Action.AButton, "p2_pass" }
                }
            }
        };

    public static Vector2 GetInputVector(PlayerCharacter.ControlScheme scheme)
    {
        var map = ACTIONS_MAP[scheme];
        return Input.GetVector(map[Action.LEFT], map[Action.RIGHT], map[Action.UP], map[Action.DOWN]);
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