
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;

public partial class LineChart : Godot.Control
{
    public string Title { get; set; }
    public string XAxisLabel { get; set; }
    public string YAxisLabel { get; set; }
    public float Width { get; set; }
    public float YAxisMin { get; set; }
    public float YAxisMax { get; set; }
    public float XAxisGridStep { get; set; }
    public float YAxisGridStep { get; set; }

    private struct Series
    {
        public Godot.Color Color;
        public List<Vector2> Values;
    }
    private Dictionary<object, Series> series = new Dictionary<object, Series>();
    private float maxX = 0;

    private System.Threading.Mutex mutex = new System.Threading.Mutex();

    private static Godot.Color[] ItemColors = new Godot.Color[]
    {
        Godot.Color.Color8(255, 255, 255),
        Godot.Color.Color8(255, 255, 0),
        Godot.Color.Color8(0, 255, 255),
        Godot.Color.Color8(255, 0, 255),
        Godot.Color.Color8(255, 0, 0),
        Godot.Color.Color8(0, 255, 0),
        Godot.Color.Color8(0, 0, 255),
    };

    private int newItemIndex = 0;

    public void AddValue(object key, float time, float val)
    {
        using var l = new MutexLock(this.mutex);

        if (!series.TryGetValue(key, out var s))
        {
            s = new Series
            {
                Color = ItemColors[newItemIndex++ % ItemColors.Length],
                Values = new List<Vector2>(),
            };
            series[key] = s;
        }

        s.Values.Add(new Vector2(time, val));
        maxX = Math.Max(maxX, time);

        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Godot.Engine.GetFramesDrawn() % 300 == 0)
        {
            using var l = new MutexLock(this.mutex);

            var deathThreshold = maxX - Width;

            // clean up all our paths every five seconds I guess
            foreach (var item in series)
            {
                var values = item.Value.Values;
                var i = 0;
                while (i < values.Count && values[i].X < deathThreshold)
                {
                    i++;
                }

                // we want to keep one extra just so the line extends properly
                // unless we literally finished them all off, in which case, burn them
                if (i < values.Count)
                {
                    i--;
                }

                if (i > 0)
                {
                    values.RemoveRange(0, i);
                }
            }

            series = series.Where(s => s.Value.Values.Count > 0).ToDictionary();
        }
    }


    private struct Liner
    {
        private List<Vector2> points = new List<Vector2>();
        private List<Color> colors = new List<Color>();

        public Liner() { }

        public void Draw(Godot.Control control)
        {
            control.DrawMultilineColors(points.ToArray(), colors.ToArray(), -1f);
            points.Clear();
            colors.Clear();
        }

        public void Line(Vector2 start, Vector2 end, Color color)
        {
            points.Add(start);
            points.Add(end);
            colors.Add(color);
        }

        public void Line(List<Vector2> points, Color color)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                this.points.Add(points[i]);
                if (i != 0 && i != points.Count - 1)
                {
                    this.points.Add(points[i]);
                }
            }
            this.colors.AddRange(Enumerable.Repeat(color, points.Count - 1));
        }

        public void Circle(Vector2 center, float radius, Color color)
        {
            var steps = 8;
            var step = 2 * Mathf.Pi / steps;
            for (int i = 0; i < steps; ++i)
            {
                var a = i * step;
                var b = (i + 1) * step;
                Line(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, color);
            }
        }
    }

    private Liner liner = new Liner();
    public override void _Draw()
    {
        base._Draw();

        using var l = new MutexLock(this.mutex);

        var numericBounds = new Rect2(maxX - Width, YAxisMin, Width, YAxisMax - YAxisMin);
        var drawBounds = new Rect2(Vector2.Zero, GetRect().Size);
        DrawRect(new Rect2(Vector2.Zero, GetRect().Size), Godot.Color.Color8(0, 0, 0, 128));

        // draw ticks relative to the x axis based on the grid step
        for (float x = maxX - XAxisGridStep; x >= maxX - Width; x -= XAxisGridStep)
        {
            liner.Line(
                new Vector2(x, YAxisMin).Remap(numericBounds, drawBounds),
                new Vector2(x, YAxisMax).Remap(numericBounds, drawBounds),
                Godot.Color.Color8(255, 255, 255, 64));
        }

        // draw ticks relative to the y axis based on the grid step
        for (float y = YAxisMin.TruncateTo(YAxisGridStep); y <= YAxisMax; y += YAxisGridStep)
        {
            liner.Line(
                new Vector2(maxX - Width, y).Remap(numericBounds, drawBounds),
                new Vector2(maxX, y).Remap(numericBounds, drawBounds),
                Godot.Color.Color8(255, 255, 255, (byte)(y == 0 ? 64 : 16)));
        }

        foreach (var line in series.Values)
        {
            var croppedLine = line.Values.Where(val => val.X > maxX - Width)
                .Select(val => val.Remap(numericBounds, drawBounds)).ToList();
            if (croppedLine.Count >= 2)
            {
                liner.Line(croppedLine, line.Color);
            }

            // draw the last point
            if (line.Values.Count > 0)
            {
                var point = line.Values.Last();
                if (point.X > maxX - Width)
                {
                    liner.Circle(point.Remap(numericBounds, drawBounds), 2, line.Color);
                }
            }
        }

        // draw outline around the whole thing using polyline
        var outline = new List<Vector2>();
        outline.Add(new Vector2(1, 1));
        outline.Add(new Vector2(drawBounds.End.X, 0));
        outline.Add(new Vector2(drawBounds.End.X, drawBounds.End.Y));
        outline.Add(new Vector2(1, drawBounds.End.Y));
        outline.Add(new Vector2(1, 1));
        liner.Line(outline, Godot.Color.Color8(255, 255, 255, 128));

        liner.Draw(this);
    }
}
