using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined

    // It spawns the player on the designated spawn points.
{
    [SerializeField] NetworkObject playerPrefab;
    [SerializeField] NetworkObject gameManagerPrefab;

    void Start()
    {
        GetComponent<NetworkRunner>().StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "ChromaPals",
            PlayerCount = GameManager.RequiredPlayers,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer) return;

        Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);

        if (Runner.IsSharedModeMasterClient && GameManager.Instance == null)
        {
            Runner.Spawn(gameManagerPrefab);
        }
    }
}
