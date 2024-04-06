
using System.Collections.Generic;

public partial class DebugSyntaxHighlighter : Godot.SyntaxHighlighter
{
    public DebugUI callback;

    private Dictionary<Godot.Color, Godot.Collections.Dictionary> dictLookup = new Dictionary<Godot.Color, Godot.Collections.Dictionary>();

    public override Godot.Collections.Dictionary _GetLineSyntaxHighlighting(int line)
    {
        Godot.Color color = callback.GetLineColor((int)line);

        var lookup = dictLookup.TryGetValue(color);

        if (lookup == null)
        {
            lookup = new Godot.Collections.Dictionary();

            var formatinfo = new Godot.Collections.Dictionary();
            formatinfo["color"] = color;

            lookup[0] = formatinfo;

            dictLookup[color] = lookup;
        }

        return lookup;
    }
}
