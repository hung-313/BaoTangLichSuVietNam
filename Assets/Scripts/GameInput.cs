using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lớp đọc phím/chuột dùng chung. Tự chọn Input Manager cũ hoặc Input System mới
/// tùy theo cài đặt của project, nên script chạy được trên cả hai.
/// </summary>
public static class GameInput
{
    /// <summary>W/S = trục Y, A/D = trục X (độ lớn tối đa 1).</summary>
    public static Vector2 Move()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        Vector2 v = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return Vector2.ClampMagnitude(v, 1f);
#elif ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);
        float y = (k.wKey.isPressed ? 1f : 0f) - (k.sKey.isPressed ? 1f : 0f);
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
#else
        return Vector2.zero;
#endif
    }

    /// <summary>Độ dịch chuyển chuột trong frame này (đã quy về cùng thang với Input Manager cũ).</summary>
    public static Vector2 Look()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#elif ENABLE_INPUT_SYSTEM
        var m = Mouse.current;
        if (m == null) return Vector2.zero;
        return m.delta.ReadValue() * 0.1f;
#else
        return Vector2.zero;
#endif
    }

    public static bool InteractPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#elif ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return false;
#endif
    }

    public static bool EscapePressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#elif ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return false;
#endif
    }
}
