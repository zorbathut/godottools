using System.Linq;
using Godot;

namespace runemonger.ui.debug;

using System.Collections.Generic;

public partial class DebugProfWindow : Godot.Window
{
    [Godot.Export]
    public Godot.Label Path;

    [Godot.Export]
    public Godot.Tree Data;

    [Godot.Export]
    public Godot.Texture2D ForwardIcon;

    private List<Prof.ProfUnit> stack = new List<Prof.ProfUnit>();
    private Dictionary<Godot.TreeItem, Prof.ProfUnit> next = new Dictionary<Godot.TreeItem, Prof.ProfUnit>();

    private void _on_back_pressed()
    {
        Dbg.Inf("bak");
        if (stack.Count > 0)
        {
            stack.RemoveAt(stack.Count - 1);
        }
    }

    private void _on_tree_button_clicked(Godot.TreeItem item, long column, long id, long mouse_button_index)
    {
        stack.Add(next[item]);
    }

    private Godot.TreeItem CreateItem(Godot.TreeItem parent, Prof.ProfUnit unit)
    {
        var item = Data.CreateItem(parent);

        item.SetTextAlignment(1, HorizontalAlignment.Right);
        item.SetTextAlignment(2, HorizontalAlignment.Right);
        item.SetTextAlignment(3, HorizontalAlignment.Right);

        if (unit.GetId().line == -1)
        {
            item.SetText(0, $"{unit.GetId().name} ({unit.GetId().file}:{unit.GetId().line})");
            item.SetText(1, "n/a");
            item.SetText(2, $"{unit.GetChildren().Values.Sum(c => c.GetAccumulatedUs()):F0}us");
            item.SetText(3, "n/a");
        }
        else
        {
            item.SetText(0, $"{unit.GetId().name} ({unit.GetId().file}:{unit.GetId().line})");
            item.SetText(1, $"{unit.GetAccumulatedUsSelf():F0}us");
            item.SetText(2, $"{unit.GetAccumulatedUs():F0}us");
            item.SetText(3, $"{unit.GetCalls():F0}");
        }

        if (parent != null)
        {
            item.AddButton(0, ForwardIcon);
        }

        next[item] = unit;

        return item;
    }

    private void RefreshAll()
    {
        Path.Text = string.Join(" > ", stack.Select(pu => pu.GetId().name));

        Data.Clear();
        next.Clear();

        Data.SetColumnTitle(0, "Name");
        Data.SetColumnTitle(1, "Self");
        Data.SetColumnTitle(2, "Aggregate");
        Data.SetColumnTitle(3, "Calls");

        if (stack.Count == 0)
        {
            var trueRoot = Data.CreateItem();
            Data.HideRoot = true;

            // find the roots
            foreach (var root in Prof.GetRoots())
            {
                CreateItem(trueRoot, root);
            }
        }
        else
        {
            var current = stack.Last();

            var trueRoot = CreateItem(null, current);
            Data.HideRoot = false;

            // find the chrilden
            foreach (var child in stack.Last().GetChildren().Values.OrderByDescending(c => c.GetAccumulatedUs()))
            {
                CreateItem(trueRoot, child);
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        RefreshAll();
    }
}
