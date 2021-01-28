using System;
using UnityEngine;

public class PlayerController : StateMachine
{
    public float MoveSpeed;
    public float HorizontalMoveAcceleration;

    private Physics _physics;

    private void Awake()
    {
        _physics = GetComponent<Physics>();

        ChangeState<PlayerIdleState>();
    }

    public void Move(Vector2 direction)
    {
        _physics.Move(direction, MoveSpeed, HorizontalMoveAcceleration);
    }

    public void Jump()
    {
        _physics.ForceJump(8);
    }
}

public class PlayerState : State
{
    protected PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    protected override void AddListeners()
    {
        base.AddListeners();

        InputController.ChangeAxisEvent += OnPressMove;
        InputController.JumpPressedEvent += OnPressJump;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputController.ChangeAxisEvent -= OnPressMove;
        InputController.JumpPressedEvent -= OnPressJump;
    }

    protected virtual void OnPressMove(object sender, InfoEventArgs<Vector2> e)
    {
        _player.Move(new Vector2(e.info.x, 0));
    }

    protected virtual void OnPressJump(object sender, EventArgs e)
    {
        _player.Jump();
    }
}

public class PlayerIdleState : PlayerState
{
    public override void Enter()
    {
        base.Enter();

        // change animation
    }
}
