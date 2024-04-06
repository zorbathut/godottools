
using System;
using System.Collections.Generic;

public partial class DebugButtonWindow : Godot.Window
{
    public struct ButtonSpec
    {
        public System.Reflection.MethodInfo directMethod;
        public Action userAction;
    }

    [Godot.Export]
    Godot.PackedScene buttonScene;

    [Godot.Export]
    Godot.Container buttonContainer;

    public void SetButtons(Dictionary<string, ButtonSpec> buttons)
    {
        foreach (var buttonSpec in buttons)
        {
            var button = buttonScene.Instantiate<Godot.Button>();
            buttonContainer.AddChild(button);

            button.Text = buttonSpec.Key;
            button.Pressed += buttonSpec.Value.userAction;
        }
    }
}
