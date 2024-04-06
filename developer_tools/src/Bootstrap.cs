
using Godot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Network;

public partial class Bootstrap : Node
{
	private IEnumerator<bool> runtimeCoroutine;

	[Godot.Export] public PackedScene permaload;

	public Game game;

	public override void _Ready()
	{
		base._Ready();

        // Register log capture; do this first so we have a chance of intercepting errors!
		Godot.LogManager.RegisterLogCaptureNonthreadsafe(Callable.From((Variant var) => {
			DebugUI.InfoLine.Type type = DebugUI.InfoLine.Type.Error;

			var varDict = var.AsGodotDictionary();
			if (varDict["type"].AsString() == "info") type = DebugUI.InfoLine.Type.Info;
			if (varDict["type"].AsString() == "warning") type = DebugUI.InfoLine.Type.Warning;

			string header = varDict["text"].AsString().Trim();
			if (varDict.ContainsKey("rationale"))
			{
				header += $"\n{varDict["rationale"].AsString().Trim()}";
			}

			string fulltextPrefix = header;
			if (varDict.ContainsKey("function"))
			{
				fulltextPrefix += $"\n  at {varDict["function"]} in {varDict["file"]}:{varDict["line"]}";
			}

            if (type == DebugUI.InfoLine.Type.Error)
            {
                System.Diagnostics.Debugger.Break();
            }

			DebugUI.AddInfoLine(new DebugUI.InfoLine
			{
				type = type,
				header = header,
				fulltext = fulltextPrefix,
			});
		}));

        // Set globals
        Find.Window = GetWindow();

        // Init thread ID for our prof system
        System.Threading.Thread.CurrentThread.Name = "main";

		Dec.Config.InfoHandler = Dbg.Inf;
		Dec.Config.WarningHandler = Dbg.Wrn;
		Dec.Config.ErrorHandler = Dbg.Err;
		Dec.Config.ExceptionHandler = Dbg.Ex;

		Dec.Config.UsingNamespaces = new string[] { "Ghi" };
		Dec.Config.ConverterFactory = Dec.RecorderEnumerator.Config.ConverterFactory;

		Arbor.Config.InfoHandler = Dbg.Inf;
		Arbor.Config.WarningHandler = Dbg.Wrn;
		Arbor.Config.ErrorHandler = Dbg.Err;
		Arbor.Config.ExceptionHandler = Dbg.Ex;

		// Spool up dec system
		var parser = new Dec.Parser();
		foreach (var fname in UtilGodot.GetFilesFromDir("res://dec"))
		{
			parser.AddString(Dec.Parser.FileType.Xml, UtilGodot.GetFileAsString(fname), fname);
		}
		parser.Finish();

		Dbg.Inf($"Loaded {Dec.Database.Count} decs");

		Ghi.Environment.Init();

		runtimeCoroutine = Runtime().GetEnumerator();
	}

	private void InitRecording()
	{
		try {
			var obsProcess = new System.Diagnostics.Process();
			obsProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(
				"obs", "--startrecording --scene runemonger_record --minimize-to-tray --disable-shutdown-check"
			);
			obsProcess.Start();

			var watchdogProcess = new System.Diagnostics.Process();
			watchdogProcess.StartInfo = new System.Diagnostics.ProcessStartInfo(
				"bash", $"-c \"while kill -0 {System.Environment.ProcessId} > /dev/null 2>&1; do sleep 1; done; kill -15 {obsProcess.Id}\""
			);
			watchdogProcess.Start();
		}
		catch (System.ComponentModel.Win32Exception e)
		{
			Dbg.Wrn(e.ToString());
		}
	}

	private IEnumerable<bool> Runtime()
	{
		// Get permaload up and running
		Find.Permaload = permaload.Instantiate<Permaload>();

		// Make the main UI
		GetParent().AddChild(Find.Permaload.Hud.Instantiate());

		// Make the debug UI
		GetParent().AddChild(Find.Permaload.DebugUI.Instantiate());

		// Set up a map
		GetParent().AddChild(Find.Permaload.Arena.Instantiate());

        // Add a debug node in 3d space
        Find.NodeRoot = new Node3D();
        GetParent().AddChild(Find.NodeRoot);

		// Per-map stuff
		/*
		foreach (var node in GetTree().GetNodes().OfType<Node3D>())
		{
			var actorDec = ActorDec.FindActorFromPath(node.SceneFilePath);
			if (actorDec != null)
			{
				Dbg.Inf($"Importing actor {actorDec} from node {node}");

				Spawn.FromNode(node, actorDec);

				// Check parents to make sure they don't match
				foreach (var parent in node.GetParents())
				{
					var parentActorDec = ActorDec.FindActorFromPath(parent.SceneFilePath);
					if (parentActorDec != null)
					{
						Dbg.Err($"Actor node {node} of type {actorDec} is a child of actor node {parent} of type {parentActorDec}");
					}
				}
			}
		}*/

		Find.Camera = GetParent().GetChildSingleOrDefault<Godot.Camera3D>();

		// Wait a tick to ensure the navmap is ready.

		yield return false;

		{
			// set up game env
			var args = ParseArgs(OS.GetCmdlineUserArgs());

            bool fakeLag = false;
            bool record = true;

			if (args.ContainsKey("serve"))
            {
				game = new GameServerListen();
				args.Remove("serve");
				DisplayServer.WindowSetTitle(ProjectSettings.GetSetting("application/config/name").ToString() + " (server)");
                UtilMisc.SetScreenQuadrant(0);

                fakeLag = true;
            }
			else if (args.ContainsKey("connect"))
            {
                record = false;

				game = new GameClient();
				args.Remove("connect");
				DisplayServer.WindowSetTitle(ProjectSettings.GetSetting("application/config/name").ToString() + " (client)");
                UtilMisc.SetScreenQuadrant(1);

                fakeLag = true;
            }
			else
			{
				game = new GameSolo();
			}

            if (record)
            {
                // at some point I'll need a better implementation of this
                if (System.Environment.MachineName == "sharon" && System.Environment.UserName == "zorba")
                {
                    InitRecording();
                }
            }

            if (fakeLag)
            {
                var rng = new Rng();

                int pivot = rng.Inclusive(50, 500);
                int range = rng.Inclusive(0, pivot - 10);

                LagGenerator.Config_LatencyMSMin = pivot - range;
                LagGenerator.Config_LatencyMSMax = pivot + range;
            }

			if (args.Count > 0)
			{
				Dbg.Err($"Unrecognized command-line arguments: {string.Join(", ", args.Keys)}");
			}
		}

        while (true)
        {
            {
                using var p = Prof.Sample();
                game.Process();
            }

            yield return false;
        }
	}

	public override void _PhysicsProcess(double delta)
    {
        Prof.BumpFrame();

        using var p = Prof.Sample();

		base._PhysicsProcess(delta);

        runtimeCoroutine.MoveNext();
	}

    private static Regex ArgsRegex = new Regex(@"^--(?<key>[a-zA-Z0-9_\-]+)=(?<value>.+)$", RegexOptions.Compiled);
    private Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>();

        foreach (var arg in args)
        {
            var match = ArgsRegex.Match(arg);

            if (match.Success)
            {
                string key = match.Groups["key"].Value;
                string value = match.Groups["value"].Value;

                if (!result.ContainsKey(key))
                {
                    result[key] = value;
                }
                else
                {
                    Dbg.Err($"Duplicate command-line key detected: {key}. Overwriting previous value.");
                    result[key] = value;
                }
            }
            else
            {
                Dbg.Err($"Invalid argument format: {arg}");
            }
        }

        return result;
    }
}
