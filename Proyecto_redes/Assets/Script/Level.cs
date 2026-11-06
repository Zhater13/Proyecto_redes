using Unity.VisualScripting;
using UnityEngine;

public class Level : MonoBehaviour

    // Holds one level’s data
{
    public string LevelName;
    public Transform[] SpawnPoints;
    public GameColor[] Player1Colors;
    public GameColor[] Player2Colors;

    public GameColor[] ColorsFor(int slot)
    {
        if (slot == 0) return Player1Colors;
        return Player2Colors;
    }
}
