using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public PlayerInput playerInput;

    private void Awake()
    {
        Instance = this;

        playerInput = new PlayerInput();
        playerInput.Enable();
    }

    public Vector2 GetMovementInput()
    {
        return playerInput.Player.Move.ReadValue<Vector2>();
    }

    public bool IsKickPressed()
    {
        return playerInput.Player.Kick.triggered;
    }
}
