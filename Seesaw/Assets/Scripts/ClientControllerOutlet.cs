using System.Collections.Generic;
using LSL4Unity.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using SeesawHelper;

public class ClientControllerOutlet : AFloatOutlet, PlayerController
{
    public KeyCode upKey = KeyCode.O;
    public KeyCode downKey = KeyCode.L;
    public GamepadStick stick = GamepadStick.Right;

    private float acceleration = 1.0f;

    private static bool isFreeze = false;
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

    public void Reset()
    {
        StreamName = "Unity.ClientPlayerDistance";
        StreamType = "Unity.Scalar";
        moment = MomentForSampling.FixedUpdate;
    }

    public override List<string> ChannelNames
    {
        get { return new List<string> { "PlayerShiftDistance" }; }
    }

    protected override bool BuildSample()
    {
        sample[0] = 0.0f;

        if (isFreeze)
            return true;

        var gamepad = Gamepad.current;

        if (Input.GetKey(upKey))
        {
            AccelerateAndSetSample(1f);
        }
        else if (Input.GetKey(downKey))
        {
            AccelerateAndSetSample(-1f);
        }
        else if (gamepad != null)
        {
            Vector2 move = stick == GamepadStick.Left
                ? gamepad.leftStick.ReadValue()
                : gamepad.rightStick.ReadValue();

            if (Mathf.Abs(move.y) > 0.05f)
                AccelerateAndSetSample(move.y);
            else
                acceleration = 1.0f;
        }
        else
        {
            acceleration = 1.0f;
        }

        return true;
    }

    private void AccelerateAndSetSample(float direction)
    {
        if (acceleration < PlayerHelper.MaxAcceleration)
            acceleration += PlayerHelper.AccelerationRate;
        sample[0] = PlayerHelper.BasePlayerSpeed * direction * acceleration;
    }
}
