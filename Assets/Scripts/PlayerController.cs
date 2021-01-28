using UnityEngine;

public class PlayerController : StateMachine
{
    public float MoveSpeed;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        ChangeState<PlayerIdleState>();
    }

    public void Move(Vector2 direction)
    {
        _rigidbody.velocity = direction * MoveSpeed;
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
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputController.ChangeAxisEvent -= OnPressMove;
    }

    protected virtual void OnPressMove(object sender, InfoEventArgs<Vector2> e)
    {
        _player.Move(e.info);
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

public class PlayerMovingState : PlayerState
{
    public override void Enter()
    {
        base.Enter();

        // change animation
    }
}
