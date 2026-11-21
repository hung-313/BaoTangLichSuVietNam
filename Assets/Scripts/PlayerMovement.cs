using UnityEngine;

/// <summary>F1: di chuyển bằng W, A, S, D (đi theo hướng đang nhìn, có trọng lực, không xuyên tường).</summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float gravity = -9.81f;

    CharacterController cc;
    float velocityY;

    void Awake() { cc = GetComponent<CharacterController>(); }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        Vector2 input = GameInput.Move();
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        if (cc.isGrounded && velocityY < 0f) velocityY = -2f;
        velocityY += gravity * Time.deltaTime;

        cc.Move((move * moveSpeed + Vector3.up * velocityY) * Time.deltaTime);
    }
}
