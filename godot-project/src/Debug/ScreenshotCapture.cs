using Godot;
using System.Collections.Generic;

namespace FrontsOfWar.Debug;

// Dev-only visual verification (see docs/DECISIONS.md D54). Lets an agent
// with no editor open answer "what does this screen actually look like?":
//
//   godot --path . --screenshot-dir=<abs dir> [--screen=briefing|loadout|
//         mission|results] [--screenshot-frames=45,900] [--skip-tutorial]
//
// Boot creates one of these on the scene-tree root (so it survives the
// scene change into the requested screen), it saves the viewport to
// <dir>/<screen>_f<frame>.png at each listed frame, and quits after the
// last one. Needs a real window — the headless DisplayServer has nothing
// to capture — so Run-HeadlessChecks.ps1 never uses it.
public partial class ScreenshotCapture : Node
{
    public string Directory = "";
    public string ScreenName = "screen";
    public List<int> Frames = new() { 45 };

    private int _frame;
    private int _nextIndex;

    public static ScreenshotCapture FromCmdline(string[] args)
    {
        string dir = ArgValue(args, "--screenshot-dir");
        if (string.IsNullOrEmpty(dir)) return null;

        var capture = new ScreenshotCapture { Directory = dir, ProcessMode = ProcessModeEnum.Always };
        string frames = ArgValue(args, "--screenshot-frames");
        if (!string.IsNullOrEmpty(frames))
        {
            capture.Frames.Clear();
            foreach (string part in frames.Split(','))
                if (int.TryParse(part.Trim(), out int f) && f > 0) capture.Frames.Add(f);
            if (capture.Frames.Count == 0) capture.Frames.Add(45);
            capture.Frames.Sort();
        }
        return capture;
    }

    public static string ArgValue(string[] args, string flag)
    {
        foreach (string arg in args)
            if (arg.StartsWith(flag + "="))
                return arg.Substring(flag.Length + 1).Trim('"');
        return null;
    }

    public override void _Ready()
    {
        DirAccess.MakeDirRecursiveAbsolute(Directory);
    }

    public override void _Process(double delta)
    {
        _frame++;
        if (_nextIndex >= Frames.Count) return;
        if (_frame < Frames[_nextIndex]) return;

        int frame = Frames[_nextIndex++];
        string path = System.IO.Path.Combine(Directory, $"{ScreenName}_f{frame}.png");
        var image = GetViewport().GetTexture().GetImage();
        var error = image.SavePng(path);
        GD.Print(error == Error.Ok ? $"[screenshot] {path}" : $"[screenshot] FAILED {error} {path}");

        if (_nextIndex >= Frames.Count)
            GetTree().Quit();
    }
}
