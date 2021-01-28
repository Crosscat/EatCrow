using System;
using UnityEngine;

using System.Linq;

public class PlayerController : StateMachine
{
    public float MoveSpeed;
    public float HorizontalMoveAcceleration;
    public float EatingSpeed = 1;

    public float CaloriesEaten = 0;

    private Physics _physics;
    private FoodTracker _foodTracker;

    private void Awake()
    {
        _physics = GetComponent<Physics>();
        _foodTracker = GetComponent<FoodTracker>();

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

    public bool AllowedToEat()
    {
        //TODO add rules
        return true;
    }

    public Food TryFindFood()
    {
        //Find closest overlapping food, if any
        return _foodTracker.triggered
                           .OrderBy(it => (this.transform.position - it.transform.position).sqrMagnitude)
                           .FirstOrDefault();
    }

    public void ConsumeCalories(float calories)
    {
        CaloriesEaten += calories;
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
        InputController.ActionPressedEvent += OnActionPressed;
        InputController.ActionReleasedEvent += OnActionReleased;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputController.ChangeAxisEvent -= OnPressMove;
        InputController.JumpPressedEvent -= OnPressJump;
        InputController.ActionPressedEvent -= OnActionPressed;
        InputController.ActionReleasedEvent -= OnActionReleased;
    }

    protected virtual void OnPressMove(object sender, InfoEventArgs<Vector2> e)
    {
        _player.Move(new Vector2(e.info.x, 0));
    }

    protected virtual void OnPressJump(object sender, EventArgs e)
    {
        _player.Jump();
    }

    protected virtual void OnActionPressed(object sender, EventArgs e)
    {

    }

    protected virtual void OnActionReleased(object sender, EventArgs e)
    {
        
    }
}

public class PlayerIdleState : PlayerState
{
    public override void Enter()
    {
        base.Enter();

        // change animation
    }

    protected override void OnActionPressed(object sender, EventArgs e)
    {
        base.OnActionPressed(sender, e);

        Food foodToEat = _player.TryFindFood();
        if (_player.AllowedToEat() && foodToEat != null)
        {
            _player.ChangeState<PlayerEatingState>();
        }
    }
}

public class PlayerEatingState : PlayerState
{
    public override void Enter()
    {
        base.Enter();
        // change animation

        Debug.Log("Nomming...");
    }

    public override void Exit()
    {
        base.Exit();

        Debug.Log("Done Nomming...");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        Food foodToEat = _player.TryFindFood();
        if (!_player.AllowedToEat() || foodToEat == null)
        {
            _player.ChangeState<PlayerIdleState>();
            return;
        }

        float calories = foodToEat.eat(Time.deltaTime * _player.EatingSpeed);
        _player.ConsumeCalories(calories);
    }

    protected override void OnActionReleased(object sender, EventArgs e)
    {
        base.OnActionReleased(sender, e);

        _player.ChangeState<PlayerIdleState>();
    }
}
