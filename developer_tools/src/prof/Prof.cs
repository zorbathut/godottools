
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

static class Prof
{
    private static ConcurrentBag<ProfUnit> Roots = new ConcurrentBag<ProfUnit>();
    private static ThreadLocal<ProfUnit> Active = new ThreadLocal<ProfUnit>(() =>
    {
        var newRoot = new ProfUnit(new ProfUnit.Id() { file = "n/a", line = -1, name = Thread.CurrentThread.Name ?? "unnamed" });
        Roots.Add(newRoot);
        return newRoot;
    });

    private static Stopwatch Stopwatch = new Stopwatch();
    private static long CurrentFrame = 0;

    static Prof()
    {
        Stopwatch.Start();
    }

    public static void BumpFrame()
    {
        Interlocked.Increment(ref CurrentFrame);
    }

    public static ProfUnit[] GetRoots()
    {
        return Roots.ToArray();
    }

    public static ScopedProf Sample(string token = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
    {
        if (token == "")
        {
            return new ScopedProf(file, line, name);
        }
        else
        {
            return new ScopedProf(file, line, $"{name}/{token}");
        }
    }

    public class ScopedProf : IDisposable
    {
        private ProfUnit last;
        private ProfUnit current;

        private long tick_start;

        internal ScopedProf(string file, int line, string name)
        {
            last = Active.Value;
            current = last.Next(file, line, name);
            Active.Value = current;

            tick_start = Stopwatch.ElapsedTicks;
        }

        public void Dispose()
        {
            Assert.IsTrue(Active.Value == current);
            Active.Value = last;

            current.Accumulate(Stopwatch.ElapsedTicks - tick_start);
        }
    }

    public class ProfUnit
    {
        public struct Id
        {
            public string file;
            public int line;
            public string name;

            public override int GetHashCode()
            {
                return file.GetHashCode() ^ line.GetHashCode() ^ name.GetHashCode();
            }
        }
        private Dictionary<Id, ProfUnit> children = new Dictionary<Id, ProfUnit>();

        private long accumulate = 0;
        private float calls = 0;

        private long timers_last_frame = 0;

        private const float Falloff = 0.95f;
        private Id id;

        public ProfUnit(Id id)
        {
            this.id = id;
        }

        public ProfUnit Next(string file, int line, string name)
        {
            var id = new ProfUnit.Id() { file = file, line = line, name = name };
            if (children.TryGetValue(id, out var child) == false)
            {
                child = new ProfUnit(id);
                children[id] = child;
            }

            return child;
        }

        public void Accumulate(long ticks)
        {
            UpdateAccumulate();

            accumulate += (long)(ticks * (1 - Falloff));
            calls += 1 - Falloff;
        }

        private void UpdateAccumulate()
        {
            if (timers_last_frame < CurrentFrame)
            {
                float falloffCalculated = Falloff;
                if (timers_last_frame + 1 < CurrentFrame)
                {
                    accumulate = (long)(accumulate * Math.Pow(falloffCalculated, CurrentFrame - timers_last_frame));
                }

                accumulate = (long)(accumulate * falloffCalculated);
                calls = calls * falloffCalculated;

                timers_last_frame = CurrentFrame;
            }
        }

        public float GetAccumulatedUs()
        {
            UpdateAccumulate();
            return accumulate / (float)Stopwatch.Frequency * 1_000_000;
        }

        public float GetAccumulatedUsSelf()
        {
            return GetAccumulatedUs() - children.Values.Sum(c => c.GetAccumulatedUs());
        }

        public float GetCalls()
        {
            UpdateAccumulate();
            return calls;
        }

        public Id GetId()
        {
            return id;
        }

        public Dictionary<Id, ProfUnit> GetChildren()
        {
            return children;
        }
    }
}
