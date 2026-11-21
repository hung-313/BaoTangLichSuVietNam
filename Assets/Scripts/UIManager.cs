using UnityEngine;
using UnityEngine.UI;

/// <summary>Hiện/ẩn gợi ý "Nhấn E", bảng thông tin hiện vật và menu tạm dừng.</summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject promptObj, infoPanel, pauseMenu;
    public Text promptText, titleText, eraText, descText;
    public Image exhibitImage;
    public RectTransform descRect;

    Vector2 descOffsetWithImage;

    void Awake()
    {
        Instance = this;
        if (descRect != null) descOffsetWithImage = descRect.offsetMin;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void ShowPrompt(bool show, string exhibitName = "")
    {
        if (promptObj == null) return;
        if (show && promptText != null)
            promptText.text = "Nhấn [E] để xem thông tin: " + exhibitName;
        if (promptObj.activeSelf != show) promptObj.SetActive(show);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenu != null) pauseMenu.SetActive(show);
    }

    public void ShowInfo(ExhibitData d)
    {
        if (d == null) return;
        titleText.text = d.title;
        eraText.text = "Niên đại: " + d.era;
        descText.text = d.description;

        bool hasImage = d.image != null;
        exhibitImage.sprite = d.image;
        exhibitImage.gameObject.SetActive(hasImage);
        if (descRect != null)
            descRect.offsetMin = hasImage ? descOffsetWithImage : new Vector2(40f, descOffsetWithImage.y);

        if (promptObj != null) promptObj.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void HideInfo() { infoPanel.SetActive(false); }
}
