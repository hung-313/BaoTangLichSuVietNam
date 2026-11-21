using UnityEngine;

/// <summary>F2: xoay góc nhìn bằng chuột. Chuột ngang xoay thân, chuột dọc ngẩng/cúi camera (giới hạn ±80°).</summary>
public class PlayerLook : MonoBehaviour
{
    public Transform cameraTransform;
    public float sensitivity = 2f;
    public float maxPitch = 80f;

    float pitch;

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        Vector2 look = GameInput.Look() * sensitivity;

        pitch = Mathf.Clamp(pitch - look.y, -maxPitch, maxPitch);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * look.x);
    }
}
