using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static event EventHandler<InfoEventArgs<Vector2>> ChangeAxisEvent;
    public static event EventHandler JumpPressedEvent;
    public static event EventHandler ActionPressedEvent;
    public static event EventHandler ActionReleasedEvent;
    public static event EventHandler LiftPressedEvent;
    public static event EventHandler LiftReleasedEvent;

    private static int _xAxis;
    private static int _yAxis;

    public void Update()
    {
        int x = GetXAxis();
        int y = GetYAxis();

        if (x != _xAxis || y != _yAxis)
        {
            _xAxis = x;
            _yAxis = y;
            ChangeAxisEvent?.Invoke(this, new InfoEventArgs<Vector2>(GetAxis()));
        }

        if (Input.GetButtonDown("Jump"))
        {
            JumpPressedEvent?.Invoke(this, null);
        }

        if (Input.GetButtonDown("Action"))
        {
            ActionPressedEvent?.Invoke(this, null);
        }

        if (Input.GetButtonUp("Action"))
        {
            ActionReleasedEvent?.Invoke(this, null);
        }

        if (Input.GetButtonDown("Lift"))
        {
            LiftPressedEvent?.Invoke(this, null);
        }

        if (Input.GetButtonUp("Lift"))
        {
            LiftReleasedEvent?.Invoke(this, null);
        }
    }

    private static int GetXAxis()
    {
        int x = (int)Input.GetAxisRaw("Horizontal");
        return x;
    }

    private static int GetYAxis()
    {
        int y = (int)Input.GetAxisRaw("Vertical");
        return y;
    }

    public static Vector2 GetAxis()
    {
        return new Vector2(GetXAxis(), GetYAxis()).normalized;
    }
}
