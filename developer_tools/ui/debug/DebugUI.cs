
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using runemonger.ui.debug;

[AttributeUsage(AttributeTargets.Method)]
public class DebugCommandLocalAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class DebugCommandRPCAttribute : Attribute { }

public partial class DebugUI : Godot.Control
{
    const int LineMax = 100;

    public struct InfoLine
    {
        internal enum Type
        {
            Info,
            Warning,
            Error,
        }

        internal Type type;
        internal string header;
        internal string fulltext;
    }

    private static List<InfoLine> InfoLines = new List<InfoLine>();
    private static Mutex InfoLinesMutex = new Mutex();

    public static void AddInfoLine(InfoLine infoLine)
    {
        using var l = new MutexLock(InfoLinesMutex);

        infoLine.header = infoLine.header.Split("\n").FirstOrDefault("[unknown error]");
        infoLine.header.Replace("[", "[\u200B");    // zero-width space to stop parsing as bbcode tags

        var stackEntries = System.Environment.StackTrace.ToString().Split("\n").Reverse().TakeWhile(line => !line.Contains("Dbg.cs")).Reverse();
        infoLine.fulltext = infoLine.fulltext + "\n\n" + string.Join("\n", stackEntries);

        InfoLines.Add(infoLine);
    }



    // Console debug infrastructure
    [Godot.Export]
    public Godot.Window consoleWindow;

    [Godot.Export]
    public Godot.TextEdit errorList;

    [Godot.Export]
    public Godot.TextEdit callstack;

    [Godot.Export] public Godot.Color ColorInf;
    [Godot.Export] public Godot.Color ColorWrn;
    [Godot.Export] public Godot.Color ColorErr;

    private int toProcess = 0;
    private int lastCaret = -1;

    // Debug button infrastructure
    [Godot.Export]
    public DebugButtonWindow debugWindow;

    [Godot.Export]
    public DebugBlackboardWindow debugBlackboardWindow;

    [Godot.Export]
    public DebugProfWindow debugProfWindow;

    [Godot.Export]
    public Godot.Label statusLabel;

    [Godot.Export]
    public Godot.VBoxContainer networkDebugContainer;

    private bool debugWindowInitialized = false;

    // Debug button inflight
    private object[] debugInflightParameters;
    private Type[] debugInflightParameterTypes;
    private int debugInflightParameterIndex;
    private Action<object[]> debugInflightMethod;

    // Debug pause
    public static bool s_Paused = false;

    public DebugUI()
    {
        Find.DebugUI = this;
    }

    public void _on_debug_console_pressed()
    {
        consoleWindow.Visible = !consoleWindow.Visible;
    }

    public void _on_debug_tool_pressed()
    {
        ParameterReset();

        if (!debugWindowInitialized)
        {
            Dictionary<string, DebugButtonWindow.ButtonSpec> debugButtons = new();

            foreach (var type in Dec.UtilReflection.GetAllUserTypes()) {
                foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (method.GetCustomAttributes(typeof(DebugCommandLocalAttribute), true).Length == 0) {
                        // nope
                        continue;
                    }

                    if (!method.IsStatic)
                    {
                        Dbg.Err($"DebugCommandLocalAttribute found on non-static method {method}");
                        continue;
                    }

                    if (debugButtons.ContainsKey(method.Name))
                    {
                        Dbg.Err($"Conflicting DebugCommand tag found on methods named {method.Name}, skipping {method}");
                        continue;
                    }

                    if (method.GetParameters().Length == 0)
                    {
                        debugButtons[method.Name] = new DebugButtonWindow.ButtonSpec {
                            directMethod = method,
                            userAction = () => method.Invoke(null, null)};
                    }
                    else
                    {
                        debugButtons[method.Name] = new DebugButtonWindow.ButtonSpec {
                            directMethod = method,
                            userAction = () => {
                                debugInflightParameterTypes = method.GetParameters().Select(param => param.ParameterType).ToArray();
                                debugInflightParameters = new object[debugInflightParameterTypes.Length];
                                debugInflightMethod = parms => method.Invoke(null, parms);

                                ParameterPrep(0);
                            }};
                    }
                }
            }

            foreach (var type in Dec.UtilReflection.GetAllUserTypes()) {
                foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    if (method.GetCustomAttributes(typeof(DebugCommandRPCAttribute), true).Length == 0) {
                        // nope
                        continue;
                    }

                    if (!method.IsStatic)
                    {
                        Dbg.Err($"DebugCommandRPCAttribute found on non-static method {method}");
                        continue;
                    }

                    if (debugButtons.ContainsKey(method.Name))
                    {
                        Dbg.Err($"Conflicting DebugCommand tag found on methods named {method.Name}, skipping {method}");
                        continue;
                    }

                    if (method.GetParameters().Length == 0 || method.GetParameters()[0].ParameterType != typeof(int))
                    {
                        Dbg.Err($"DebugCommandRPC on method {method.Name} must take an `int playerId` as the first parameter");
                        continue;
                    }

                    if (method.GetParameters().Length == 1)
                    {
                        debugButtons[method.Name] = new DebugButtonWindow.ButtonSpec {
                            directMethod = method,
                            userAction = () =>
                            {
                                Dbg.Inf("RPC called without parameters!");
                            }
                        };
                    }
                    else
                    {
                        debugButtons[method.Name] = new DebugButtonWindow.ButtonSpec {
                            directMethod = method,
                            userAction = () => {
                                debugInflightParameterTypes = method.GetParameters().Select(param => param.ParameterType).ToArray();
                                debugInflightParameters = new object[debugInflightParameterTypes.Length];
                                debugInflightMethod = parms =>
                                {
                                    Dbg.Inf("RPC called with parameters!");
                                };

                                ParameterPrep(0);
                            }};
                    }
                }
            }

            debugWindow.SetButtons(debugButtons);

            debugWindowInitialized = true;
        }

        debugWindow.Visible = !debugWindow.Visible;
    }

    public void _on_debug_prof_pressed()
    {
        debugProfWindow.Visible = !debugProfWindow.Visible;
    }

    public void _on_debug_blackboard_pressed()
    {
        debugBlackboardWindow.Visible = !debugBlackboardWindow.Visible;
    }

    public void _on_debug_pause_pressed()
    {
        s_Paused = !s_Paused;
    }

    public void _on_copy_pressed()
    {
        using var l = new MutexLock(InfoLinesMutex);

        int realLine = RealLineFrom(errorList.GetCaretLine());
        if (realLine != -1)
        {
            Godot.DisplayServer.ClipboardSet(InfoLines[realLine].fulltext.Trim() + "\n");
        }
    }

    public Godot.Color GetLineColor(int line)
    {
        if (line >= InfoLines.Count)
        {
            return ColorInf;
        }

        using var l = new MutexLock(InfoLinesMutex);

        int realLine = RealLineFrom(line);
        if (realLine == -1)
        {
            return ColorInf;
        }

        var infoLine = InfoLines[realLine];

        if (infoLine.type == InfoLine.Type.Error)
        {
            return ColorErr;
        }
        else if (infoLine.type == InfoLine.Type.Warning)
        {
            return ColorWrn;
        }
        else if (infoLine.type == InfoLine.Type.Info)
        {
            return ColorInf;
        }
        else
        {
            Dbg.Err("wutwut");
            return ColorErr;
        }
    }

    public override void _Ready()
    {
        base._Ready();

        (errorList.SyntaxHighlighter as DebugSyntaxHighlighter).callback = this;

        ParameterReset();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (errorList == null)
        {
            Dbg.Inf("wutwut");
            return;
        }

        int caret = errorList.GetCaretLine();

        using var l = new MutexLock(InfoLinesMutex);

        if (toProcess < InfoLines.Count)
        {
            double scroll = errorList.ScrollVertical;

            int lines = Math.Min(InfoLines.Count , LineMax * 2 + 1);
            errorList.Text = string.Join("\n", Enumerable.Range(0, lines).Select(line => {
                int realLine = RealLineFrom(line);
                if (realLine == -1)
                {
                    return $"(Skipped {InfoLines.Count - LineMax * 2})";
                }
                else
                {
                    return InfoLines[RealLineFrom(line)].header;
                }
            }));

            // gets reset when we change the text
            errorList.SetCaretLine(caret);
            errorList.ScrollVertical = scroll;

            for (int i = toProcess; i < InfoLines.Count; ++i)
            {
                if (InfoLines[i].type == InfoLine.Type.Warning || InfoLines[i].type == InfoLine.Type.Error)
                {
                    consoleWindow.Visible = true;
                    break;
                }
            }
            toProcess = InfoLines.Count;
        }

        if (lastCaret != caret && InfoLines.Count > caret)
        {
            int realLine = RealLineFrom(errorList.GetCaretLine());
            if (realLine == -1)
            {
                callstack.Text = "...";
            }
            else
            {
                callstack.Text = InfoLines[realLine].fulltext;
            }
        }

        lastCaret = caret;
    }

    private int RealLineFrom(int line)
    {
        if (InfoLines.Count > LineMax * 2 && line >= LineMax)
        {
            if (line == LineMax)
            {
                return -1;
            }

            --line;
            line = InfoLines.Count - LineMax + (line - LineMax);
        }

        return line;
    }

    private void ParameterPrep(int paramId)
    {
        debugWindow.Visible = false;

        if (paramId == debugInflightParameters.Length)
        {
            // Fire!
            debugInflightMethod(debugInflightParameters);

            // We'll just leave it so we can trigger it again with another click
        }
        else
        {
            debugInflightParameterIndex = paramId;
        }

        ParameterNoteUpdate();
    }

    private void ParameterAdvance()
    {
        ParameterPrep(debugInflightParameterIndex + 1);
    }

    private void ParameterReset()
    {
        debugInflightParameters = null;
        debugInflightParameterTypes = null;
        debugInflightParameterIndex = -1;
        debugInflightMethod = null;

        ParameterNoteUpdate();
    }

    private void ParameterNoteUpdate()
    {
        if (debugInflightParameters == null)
        {
            statusLabel.Text = "";
        }
        else
        {
            statusLabel.Text = $"CurrentlyUnnamed: {debugInflightParameterTypes[debugInflightParameterIndex]} NoName";
        }
    }

    public override void _Input(Godot.InputEvent eve)
    {
        if (debugInflightParameters == null)
        {
            // don't care!
            return;
        }

        var button = eve as Godot.InputEventMouseButton;
        var key = eve as Godot.InputEventKey;

        // if right-click or esc, ParameterReset
        if (key != null && key.Keycode == Godot.Key.Escape && key.Pressed)
        {
            ParameterReset();
        }

        if (button != null && button.ButtonIndex == Godot.MouseButton.Right && button.Pressed)
        {
            ParameterReset();
        }

        if (button != null && button.ButtonIndex == Godot.MouseButton.Left && button.Pressed)
        {
            if (debugInflightParameterTypes[debugInflightParameterIndex] == typeof(Godot.Vector3))
            {
                // match up a thing!
                var direction = Find.Camera.ProjectRayNormal(button.Position);
                Godot.PhysicsRayQueryParameters3D query = new Godot.PhysicsRayQueryParameters3D();
                query.From = Find.Camera.Position;
                query.To = Find.Camera.Position + direction * 1000;
                var result = Find.Camera.GetWorld3D().DirectSpaceState.IntersectRay(query);
                if (result.ContainsKey("collider"))
                {
                    debugInflightParameters[debugInflightParameterIndex] = (Godot.Vector3)result["position"];

                    ParameterAdvance();
                }
            }
            else
            {
                Dbg.Err($"Attempting to set debug parameter of type {debugInflightParameterTypes[debugInflightParameterIndex]}, but I don't know how");
            }
        }
    }

    public void AddToNetworkContainer(Godot.Control control)
    {
        networkDebugContainer.AddChild(control);
    }
}
