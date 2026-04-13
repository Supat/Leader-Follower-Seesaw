using UnityEngine;
using UnityEngine.InputSystem;
using SeesawHelper;

public class PlayerMovementController : MonoBehaviour, PlayerController
{
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public GamepadStick stick = GamepadStick.Left;

    private float acceleration = 1.0f;

    private bool isFreeze = false;
    public bool IsFreeze
    {
        get { return isFreeze; }
        set { isFreeze = value; }
    }

    private float perturbation = 1.0f;
    public float Perturbation
    {
        get { return perturbation; }
        set { perturbation = value; }
    }

    void FixedUpdate()
    {
        if (isFreeze)
            return;

        var gamepad = Gamepad.current;

        if (Input.GetKey(upKey))
        {
            AccelerateAndMove(1f);
        }
        else if (Input.GetKey(downKey))
        {
            AccelerateAndMove(-1f);
        }
        else if (gamepad != null)
        {
            Vector2 move = ReadStick(gamepad);
            if (Mathf.Abs(move.y) > 0.05f)
            {
                AccelerateAndMove(move.y);
            }
            else
            {
                acceleration = 1.0f;
            }
        }
        else
        {
            acceleration = 1.0f;
        }
    }

    private void AccelerateAndMove(float direction)
    {
        if (acceleration < PlayerHelper.MaxAcceleration)
            acceleration += PlayerHelper.AccelerationRate;
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + PlayerHelper.BasePlayerSpeed * direction * perturbation * acceleration,
            transform.position.z);
    }

    private Vector2 ReadStick(Gamepad gamepad)
    {
        return stick == GamepadStick.Left
            ? gamepad.leftStick.ReadValue()
            : gamepad.rightStick.ReadValue();
    }
}

public enum GamepadStick
{
    Left,
    Right
}
