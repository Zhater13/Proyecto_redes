using System.Linq;
using Fusion;

public enum GamePhase { Waiting, Playing, Victory, Defeat }

public class GameManager : NetworkBehaviour
{
    public const int MaxLives = 5;
    public const int RequiredPlayers = 2;

    public static GameManager Instance { get; private set; }

    [Networked] public GamePhase Phase { get; set; }
    [Networked] public int LevelIndex { get; set; }
    // Increases on each level start; clients compare it to know when to rebuild the level
    [Networked] public int LoadId { get; set; }
    [Networked] public GameColor Background { get; set; }
    [Networked] public int Lives { get; set; }

    public Level CurrentLevel => LevelManager.Instance.Levels[LevelIndex];

    public override void Spawned()
    {
        Instance = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this) Instance = null;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        // Only the host runs the rules

        if (Runner.ActivePlayers.Count() < RequiredPlayers)
        {
            Phase = GamePhase.Waiting;
            return;
        }

        if (Phase == GamePhase.Waiting)
        {
            StartLevel(0);
            return;
        }

        if (Phase == GamePhase.Playing && EveryoneAtExit())
        {
            if (LevelIndex + 1 < LevelManager.Instance.Levels.Length) StartLevel(LevelIndex + 1);
            else Phase = GamePhase.Victory;
        }
    }

    public override void Render()
    // Visuals only, runs on every client
    {
        LevelManager levels = LevelManager.Instance;
        if (LoadId > 0 && levels.LoadedId != LoadId) levels.ShowLevel(LevelIndex, LoadId);
        levels.SetBackground(Background);
    }

    bool EveryoneAtExit()
    {
        return PlayerController.All.Count >= RequiredPlayers && PlayerController.All.All(player => player.ExitReached == LoadId);
    }

    void StartLevel(int index)
    {
        LevelIndex = index;
        LoadId++;
        Background = GameColor.None;
        Lives = MaxLives;
        Phase = GamePhase.Playing;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    // Any player can call the RPCs, but they run on the host
    public void RpcRequestColor(GameColor color)
    {
        if (Phase != GamePhase.Playing) return;
        // Picking the same color again turns it off
        Background = Background == color ? GameColor.None : color;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcPlayerDied()
    {
        if (Phase != GamePhase.Playing) return;
        Lives--;
        if (Lives <= 0) Phase = GamePhase.Defeat;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRestart()
    {
        if (Phase == GamePhase.Defeat) StartLevel(LevelIndex);
        else if (Phase == GamePhase.Victory) StartLevel(0);
    }
}
