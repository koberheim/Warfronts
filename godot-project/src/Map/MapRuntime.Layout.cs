using System;
using System.Linq;
using Godot;
using FrontsOfWar.Core;
using FrontsOfWar.Map.Authoring;
using FrontsOfWar.Meta;

namespace FrontsOfWar.Map;

public partial class MapRuntime
{
    private bool IsDeveloperFixture
    {
        get
        {
#if DEBUG
            return DeveloperFixture;
#else
            return false;
#endif
        }
    }

    private MissionDefinition LoadMissionLayout()
    {
        if (IsDeveloperFixture) return null;
        var mission = GD.Load<MissionDefinition>(MissionSession.CurrentMissionPath)
            ?? throw new InvalidOperationException("The selected mission resource is missing.");
        MapDefinition map = null;
        RuntimeMapData data = null;
#if DEBUG
        bool preview = MapRuntimeAuthoringLoader.TryLoadFromCommandLine(out map, out data, out var error);
        if (!preview && !string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
#endif
        map ??= MissionMapResolver.Load(mission);
        MissionMapResolver.ValidateWavePaths(mission, map);
        data ??= MapRuntimeDataFactory.Build(map);
        AuthoringMap = map;
        AuthoringRuntimeData = data;
        Difficulty = MissionSession.SelectedDifficulty;
        PathNetworkPath = MapRuntimeAuthoringBuilder.InstallIntoMission(this, map, data, PathNetworkPath, this);
        AirCorridor = MapRuntimeAuthoringBuilder.BuildAirCorridor(data, GameBalanceConfigAutoload.Config.TilePixelSize);
        MissionSession.LastMissionTitle = mission.Title;
        FitMapCamera();
        GetViewport().SizeChanged += FitMapCamera;
        return mission;
    }

    private void FitMapCamera()
    {
        var camera = GetNodeOrNull<Camera2D>("Camera");
        if (camera == null || AuthoringMap?.Metadata == null) return;
        var bounds = new Vector2(AuthoringMap.Metadata.WidthTiles, AuthoringMap.Metadata.HeightTiles)
            * GameBalanceConfigAutoload.Config.TilePixelSize;
        var viewport = GetViewportRect().Size;
        // Reserve HUD space above/below; presentation pixels, not gameplay tuning.
        var available = new Vector2(Mathf.Max(1, viewport.X - 64), Mathf.Max(1, viewport.Y - 300));
        float zoom = Mathf.Min(available.X / bounds.X, available.Y / bounds.Y);
        camera.Zoom = Vector2.One * zoom;
        camera.Position = bounds / 2f;
    }
}
