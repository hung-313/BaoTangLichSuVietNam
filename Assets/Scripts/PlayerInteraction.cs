using UnityEngine;

/// <summary>F4: nhìn vào hiện vật trong tầm với thì hiện gợi ý, nhấn E để xem thông tin, E hoặc Esc để đóng.</summary>
public class PlayerInteraction : MonoBehaviour
{
    public Camera cam;
    public float interactDistance = 3.5f;

    Exhibit current;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || UIManager.Instance == null) return;

        if (gm.State == GameState.Playing)
        {
            DetectExhibit();
            if (current != null && GameInput.InteractPressed())
            {
                gm.SetState(GameState.ViewingInfo);
                UIManager.Instance.ShowInfo(current.data);
            }
        }
        else if (gm.State == GameState.ViewingInfo &&
                 (GameInput.InteractPressed() || GameInput.EscapePressed()))
        {
            UIManager.Instance.HideInfo();
            gm.SetState(GameState.Playing);
        }
    }

    void DetectExhibit()
    {
        current = null;
        if (cam != null)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
                current = hit.collider.GetComponentInParent<Exhibit>();
        }

        bool show = current != null && current.data != null;
        UIManager.Instance.ShowPrompt(show, show ? current.data.title : "");
    }
}
