using Godot;
using System.Collections.Generic;
using System.Linq;

public static class Find
{
    public static Hud Hud;
    public static DebugUI DebugUI;

    public static Camera3D Camera;
    public static Rect2I CameraBounds;

    public static Permaload Permaload;

    public static Node3D NodeRoot;

    public static Godot.Window Window;

    // this is not a good place for this but I'm not sure what a good place is right now
    public static int PlayerId = -1;

    // these can all return null if we're outside the context/environment
    public static Context Context => Context.Current.Value;
    public static Ghi.Environment Environment => Context?.env;

    public static Comp.Global Globals => Context?.env?.Singleton<Comp.Global>();
    public static Ghi.Entity Player
    {
        get
        {
            var players = Globals?.players;
            if (players == null) return new Ghi.Entity();
            if (PlayerId < 0 || PlayerId >= players.Count) return new Ghi.Entity();
            return players[PlayerId];
        }
    }
    public static Ghi.Entity Avatar
    {
        get
        {
            var avatars = Globals?.avatars;
            if (avatars == null) return new Ghi.Entity();
            if (PlayerId < 0 || PlayerId >= avatars.Count) return new Ghi.Entity();
            return avatars[PlayerId];
        }
    }
}
