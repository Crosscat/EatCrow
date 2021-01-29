using System;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;

public class PlayerController : StateMachine
{
    public float MoveSpeed;
    public float HorizontalMoveAcceleration;
    public float JumpPower;

    public float EatingSpeed = 1;
    public float CalorieAccelerationFactor = .05f;
    public float CalorieJumpFactor = .02f;
    public float CalorieSpeedFactor = .01f;

    public float CaloriesEaten = 0;

    private RaycastLauncher _raycastLauncher;
    private List<RaycastTarget> _defaultRaycastTargets;

    private Physics _physics;
    private FoodTracker _foodTracker;
    private float _fallThroughTimer;

    private float AdjustedHorizontalMoveAcceleration
    {
        get { return HorizontalMoveAcceleration / (1 + CalorieAccelerationFactor * CaloriesEaten); }
    }

    private float AdjustedJumpPower
    {
        get { return JumpPower / (1 + CalorieJumpFactor * CaloriesEaten); }
    }



    private void Awake()
    {
        _physics = GetComponent<Physics>();
        _foodTracker = GetComponent<FoodTracker>();
        _raycastLauncher = GetComponent<RaycastLauncher>();
        _defaultRaycastTargets = _raycastLauncher.RaycastTargets.ToList();

        ChangeState<PlayerIdleState>();
    }

    public override void Update()
    {
        base.Update();

        if (_fallThroughTimer > 0)
        {
            _fallThroughTimer -= Time.deltaTime;
            if (_fallThroughTimer <= 0)
            {
                var target = _raycastLauncher.RaycastTargets[1];
                while (target.IgnoredDirections.Contains(Vector2.down))
                {
                    target.IgnoredDirections.Remove(Vector2.down);
                }
            }
        }
    }

    public void Move(Vector2 direction)
    {
        _physics.Move(direction, MoveSpeed, AdjustedHorizontalMoveAcceleration);
    }

    public void Jump()
    {
        _physics.ForceJump(AdjustedJumpPower);
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

    public void DropThroughOneWayObstacle()
    {
        var target = _raycastLauncher.RaycastTargets[1];
        if (!target.IgnoredDirections.Contains(Vector2.down))
        {
            target.IgnoredDirections.Add(Vector2.down);
            _fallThroughTimer = .25f;
        }

        //if (_raycastLauncher.RaycastTargets.Count == 2)
        //{
        //    var target = _raycastLauncher.RaycastTargets.First(x => x.IgnoredDirections.Count != 0);
        //    var replacement = new RaycastTarget
        //    {
        //        IgnoredDirections = target.IgnoredDirections.ToList(),
        //        Mask = target.Mask,
        //    };
        //    replacement.IgnoredDirections.Add(Vector2.down);

        //    _raycastLauncher.RaycastTargets.Remove(replacement);
        //    _raycastLauncher.RaycastTargets.Add(replacement);
        //    _fallThroughTimer = .25f;
        //}
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
        if (e.info.y < 0)
        {
            OnDownPress();
        }

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

    protected virtual void OnDownPress()
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

    protected override void OnDownPress()
    {
        base.OnDownPress();

        _player.DropThroughOneWayObstacle();
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
