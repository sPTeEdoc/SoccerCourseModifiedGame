using Godot;
using System;
using System.Threading.Tasks;

public partial class AwaitRoot : Control
{
    public override void _Ready()
    {

    }

    public async Task DoSomethingAsync()
    {
        // await GetTree().CreateTimer(3.0).Timeout();
    }
}
