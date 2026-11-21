using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, ViewingInfo, Paused }

/// <summary>Quản lý trạng thái game (F5): tham quan, xem thông tin, tạm dừng, thoát.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Playing;

    public string mainMenuScene = "MainMenu";

    int lastChangeFrame = -1;

    void Awake() { Instance = this; }
    void Start() { SetState(GameState.Playing); }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Tránh đổi trạng thái 2 lần trong cùng một frame khi nhấn Esc
        // (ví dụ Esc vừa đóng bảng thông tin thì không mở luôn menu tạm dừng).
        if (lastChangeFrame == Time.frameCount) return;

        if (GameInput.EscapePressed())
        {
            if (State == GameState.Playing) SetState(GameState.Paused);
            else if (State == GameState.Paused) SetState(GameState.Playing);
        }
    }

    public void SetState(GameState s)
    {
        State = s;
        lastChangeFrame = Time.frameCount;

        bool playing = (s == GameState.Playing);
        Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !playing;
        Time.timeScale = (s == GameState.Paused) ? 0f : 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(s == GameState.Paused);
            if (!playing) UIManager.Instance.ShowPrompt(false);
        }
    }

    public void Resume() { SetState(GameState.Playing); }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
