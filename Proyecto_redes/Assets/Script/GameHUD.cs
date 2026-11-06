using UnityEngine;
using UnityEngine.InputSystem;

public class GameHUD : MonoBehaviour
    // This draws the screen text with Unity’s OnGUI. Really crude but it was the fastest way i could think of to do in less time (for now)
{
    GUIStyle titleStyle;
    GUIStyle textStyle;
    Texture2D panelTexture;

    void Update()
    {
        GameManager game = GameManager.Instance;
        Keyboard keyboard = Keyboard.current;
        if (game == null || keyboard == null) return;
        if ((game.Phase == GamePhase.Victory || game.Phase == GamePhase.Defeat) && keyboard.rKey.wasPressedThisFrame)
            game.RpcRestart();
    }

    void OnGUI()
    {
        CreateStyles();

        GameManager game = GameManager.Instance;
        if (game == null || game.Phase == GamePhase.Waiting)
        {
            DrawMessage("Waiting for players...", "The game starts when " + GameManager.RequiredPlayers + " players are connected.");
            return;
        }

        DrawTopBar(game);

        if (game.Phase == GamePhase.Victory)
            DrawMessage("VICTORY!", "You cleared every level together.\nPress R to play again.");
        else if (game.Phase == GamePhase.Defeat)
            DrawMessage("DEFEAT", "Your team ran out of lives.\nPress R to retry the level.");
    }

    void CreateStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        textStyle.normal.textColor = Color.white;
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.08f, 0.08f, 0.1f, 0.85f));
        panelTexture.Apply();
    }

    void DrawTopBar(GameManager game)
    {
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, 44f), panelTexture);
        GUIStyle left = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleLeft };
        GUIStyle right = new GUIStyle(textStyle) { alignment = TextAnchor.MiddleRight };

        string level = "Level " + (game.LevelIndex + 1) + "/" + LevelManager.Instance.Levels.Length + "  " + game.CurrentLevel.LevelName;
        GUI.Label(new Rect(16f, 0f, Screen.width / 2f, 44f), level, left);
        GUI.Label(new Rect(Screen.width / 2f, 0f, Screen.width / 2f - 16f, 44f), "Lives: " + game.Lives, right);

        PlayerController local = PlayerController.Local;
        if (local == null) return;
        string colors = "";
        foreach (GameColor color in game.CurrentLevel.ColorsFor(local.Slot))
        {
            colors += "  [" + KeyFor(color) + "] " + color;
        }
        GUI.Label(new Rect(0f, 0f, Screen.width, 44f), "Your colors:" + colors, textStyle);
    }

    string KeyFor(GameColor color)
    {
        if (color == GameColor.Red) return "1";
        if (color == GameColor.Blue) return "2";
        return "3";
    }

    void DrawMessage(string title, string body)
    {
        Rect rect = new Rect(Screen.width / 2f - 260f, Screen.height / 2f - 100f, 520f, 200f);
        GUI.DrawTexture(rect, panelTexture);
        GUI.Label(new Rect(rect.x, rect.y + 20f, rect.width, 60f), title, titleStyle);
        GUI.Label(new Rect(rect.x + 20f, rect.y + 80f, rect.width - 40f, 100f), body, textStyle);
    }
}
