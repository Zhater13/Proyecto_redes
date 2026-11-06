using UnityEngine;

public class LevelManager : MonoBehaviour

    // Its in charge of changing the Level.
{
    public static LevelManager Instance { get; private set; }

    public Level[] Levels;
    public Camera GameCamera;

    ColorBlock[] colorBlocks = new ColorBlock[0];

    public Level CurrentLevel { get; private set; }
    public int LoadedId { get; private set; }

    void Awake()
    // Saves itself as Instance, turns off every level, and paints the camera with the neutral background.
    {
        Instance = this;
        foreach (Level level in Levels)
        {
            level.gameObject.SetActive(false);
        }
        GameCamera.backgroundColor = Palette.Background;
    }

    public void ShowLevel(int index, int loadId)
    {
        if (CurrentLevel != null) CurrentLevel.gameObject.SetActive(false);

        CurrentLevel = Levels[index];
        CurrentLevel.gameObject.SetActive(true);
        colorBlocks = CurrentLevel.GetComponentsInChildren<ColorBlock>();
        GameCamera.transform.position = CurrentLevel.transform.position + new Vector3(0f, 0f, -10f);
        LoadedId = loadId;
    }

    public void SetBackground(GameColor background) // Paints the camera with that color and calls Apply on every colored block of the current level.
    {
        GameCamera.backgroundColor = Palette.Of(background);
        foreach (ColorBlock block in colorBlocks)
        {
            block.Apply(background);
        }
    }
}
