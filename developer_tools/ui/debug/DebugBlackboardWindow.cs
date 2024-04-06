
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DebugBlackboardWindow : Godot.Window
{
    [Godot.Export]
    Godot.PackedScene itemScene;

    [Godot.Export]
    Godot.Container itemContainer;

    Dictionary<string, (Godot.Label, Godot.Label)> items = new Dictionary<string, (Godot.Label, Godot.Label)>();

    public override void _Process(double delta)
    {
        if (Visible)
        {
            UpdateBlackboard();
        }
    }

    public void UpdateBlackboard()
    {
        //bool needsSort = false;

        /*foreach (var kvp in Arbor.Blackboard.Global.GetAll().OrderBy(kvp => kvp.Key))
        {
            Godot.Label keyLabel;
            Godot.Label valueLabel;

            if (items.ContainsKey(kvp.Key))
            {
                (keyLabel, valueLabel) = items[kvp.Key];
            }
            else
            {
                keyLabel = itemScene.Instantiate<Godot.Label>();
                valueLabel = itemScene.Instantiate<Godot.Label>();

                itemContainer.AddChild(keyLabel);
                itemContainer.AddChild(valueLabel);

                items[kvp.Key] = (keyLabel, valueLabel);

                keyLabel.Text = kvp.Key;

                //needsSort = true;
            }

            valueLabel.Text = kvp.Value.ToString();
        }*/

        // todo: sort if we need to
    }
}
