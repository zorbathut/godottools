using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public static class Dbg
{
    internal static void Inf(string str)
    {
        str = Decorate(str);

        //Debugger.Log(2, "inf", str);
        GD.Print(str);
    }

    internal static void Wrn(string str)
    {
        str = Decorate(str);

        //Debugger.Log(1, "wrn", str);
        GD.PushWarning(str);
    }

    internal static void Err(string str)
    {
        str = Decorate(str);

        //Debugger.Log(0, "err", str);
        GD.PushError(str);
    }

    internal static void Ex(Exception e)
    {
        string str = Decorate(e.ToString());

        //Debugger.Log(0, "err", str);
        GD.PushError(str);
    }

    private static string Decorate(string input)
    {
        long frame = Context.Current.Value?.TimeFrame ?? -1;
        if (frame < 0)
        {
            return input;
        }
        else
        {
            return $"[{Context.Current.Value.prefix}-{frame}] {input}";
        }
    }

    internal static void HighlightTile(int x, int y, Godot.Color? color = null, float duration = 2f)
    {
        DrawLine(new Godot.Vector2(x, y), new Godot.Vector2(x + 1, y), color, duration);
        DrawLine(new Godot.Vector2(x + 1, y), new Godot.Vector2(x + 1, y + 1), color, duration);
        DrawLine(new Godot.Vector2(x + 1, y + 1), new Godot.Vector2(x, y + 1), color, duration);
        DrawLine(new Godot.Vector2(x, y + 1), new Godot.Vector2(x, y), color, duration);
    }

    internal static void DrawLine(Godot.Vector2 a, Godot.Vector2 b, Godot.Color? color = null, float duration = 2f)
    {
        var debugMesh = Find.Globals?.debugMesh.Get();
        if (debugMesh == null)
        {
            Dbg.Wrn("Attempted to draw line without DebugMesh");
            return;
        }

        debugMesh.DrawLine(a, b, color, duration);
    }

    internal static void DrawCircle(Godot.Vector2 center, float radius, Godot.Color? color = null, float duration = 2f)
    {
        const int pt = 16;
        for (int i = 0; i < 16; ++i)
        {
            float dxa = (float)Math.Sin(Math.PI * 2 * (i + 0) / pt) * radius;
            float dxb = (float)Math.Sin(Math.PI * 2 * (i + 1) / pt) * radius;

            float dya = (float)Math.Cos(Math.PI * 2 * (i + 0) / pt) * radius;
            float dyb = (float)Math.Cos(Math.PI * 2 * (i + 1) / pt) * radius;

            DrawLine(center + new Godot.Vector2(dxa, dya), center + new Godot.Vector2(dxb, dyb), color, duration);
        }
    }

    internal static void DrawRect(Godot.Rect2 rect, Godot.Color? color = null, float duration = 2f)
    {
        DrawLine(rect.Position, rect.Position + new Godot.Vector2(rect.Size.X, 0), color, duration);
        DrawLine(rect.Position + new Godot.Vector2(rect.Size.X, 0), rect.Position + new Godot.Vector2(rect.Size.X, rect.Size.Y), color, duration);
        DrawLine(rect.Position + new Godot.Vector2(rect.Size.X, rect.Size.Y), rect.Position + new Godot.Vector2(0, rect.Size.Y), color, duration);
        DrawLine(rect.Position + new Godot.Vector2(0, rect.Size.Y), rect.Position, color, duration);
    }

    internal static void DrawText(Godot.Vector2 position, string text, Godot.Color? color = null, float duration = 2f)
    {
        var debugMesh = Find.Globals?.debugMesh.Get();
        if (debugMesh == null)
        {
            Dbg.Wrn("Attempted to draw text without DebugMesh");
            return;
        }

        debugMesh.DrawText(position, text, color, duration);
    }
}
