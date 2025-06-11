using Godot;
using System;

public partial class SceneTransition : Node
{
    public void ChangeToScene(string sceneName)
    {
        GetTree().ChangeSceneToFile($"res://{sceneName}");
    }
}
