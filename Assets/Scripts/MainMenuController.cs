using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Màn hình bắt đầu (F5): nút Bắt đầu và Thoát.</summary>
public class MainMenuController : MonoBehaviour
{
    public string museumScene = "Museum";

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnStartClicked() { SceneManager.LoadScene(museumScene); }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
