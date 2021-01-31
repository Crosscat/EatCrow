using System;
using UnityEngine;

using System.Linq;

public class Entity : StateMachine
{
    public Physics Physics;
    private RaycastLauncher _raycastLauncher;

    private float _fallThroughTimer;

    public virtual void Awake()
    {
        _raycastLauncher = GetComponent<RaycastLauncher>();
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

    public void DropThroughOneWayObstacle()
    {
        var target = _raycastLauncher.RaycastTargets[1];
        if (!target.IgnoredDirections.Contains(Vector2.down))
        {
            target.IgnoredDirections.Add(Vector2.down);
            _fallThroughTimer = .5f;
        }
    }
}

public class PlayerController : Entity
{
    public float MoveSpeed;
    public float HorizontalMoveAcceleration;
    public float JumpPower;

    public float EatingSpeed = 1;
    public float CalorieAccelerationFactor = .05f;
    public float CalorieJumpFactor = .02f;
    public float CalorieSpeedFactor = .01f;

    public float CaloriesEaten = 0;

    private FoodTracker _foodTracker;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private static class ANIMATION
    {
        public static int[] IDLE_BY_FATNESS = { 0, 0, 0, 0, 0 };
        public static int[] FLYING_BY_FATNESS = { 1, 1, 1, 1, 1 };
        public static int[] WALKING_BY_FATNESS = { 0, 0, 0, 0, 0 };
        public static int[] EATING_BY_FATNESS = { 0, 0, 0, 0, 0 };
    }

    private float AdjustedHorizontalMoveAcceleration
    {
        get { return HorizontalMoveAcceleration / (1 + CalorieAccelerationFactor * CaloriesEaten); }
    }

    private float AdjustedJumpPower
    {
        get { return JumpPower / (1 + CalorieJumpFactor * CaloriesEaten); }
    }

    private float AdjustedMoveSpeed
    {
        get { return MoveSpeed / (1 + CalorieSpeedFactor * CaloriesEaten); }
    }

    public Vector2 Velocity
    {
        get { return Physics.Velocity; }
    }

    public bool Grounded
    {
        get { return Physics.Grounded; }
    }

    public int FatnessLevel
    {
        get 
        {
            //TODO apply rule
            return 0;
        }
    }

    public int AnimationSpeed
    {
        set
        {
            _animator.speed = value;
        }
    }

    public override void Awake()
    {
        base.Awake();

        Physics = GetComponent<Physics>();
        _foodTracker = GetComponent<FoodTracker>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        ChangeState<PlayerGroundedState>();
    }

    public override void Update()
    {
        base.Update();

        if (Velocity.x > 0) _spriteRenderer.flipX = true;
        else if (Velocity.x < 0) _spriteRenderer.flipX = false;
    }

    public void Move(Vector2 direction)
    {
        Physics.Move(direction, AdjustedMoveSpeed, AdjustedHorizontalMoveAcceleration);
    }

    public void Jump()
    {
        Physics.ForceJump(AdjustedJumpPower);
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

    public void AnimateIdle()
    {
        _animator.SetInteger("State", ANIMATION.IDLE_BY_FATNESS[FatnessLevel]);
    }

    public void AnimateFlying()
    {
        _animator.SetInteger("State", ANIMATION.FLYING_BY_FATNESS[FatnessLevel]);
    }

    public void AnimateWalking()
    {
        _animator.SetInteger("State", ANIMATION.WALKING_BY_FATNESS[FatnessLevel]);
    }

    public void AnimateEating()
    {
        _animator.SetInteger("State", ANIMATION.EATING_BY_FATNESS[FatnessLevel]);
    }
}

public abstract class PlayerNormalState : State
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
        InputController.LiftPressedEvent += OnLift;
        InputController.LiftReleasedEvent += OnDrop;
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();

        InputController.ChangeAxisEvent -= OnPressMove;
        InputController.JumpPressedEvent -= OnPressJump;
        InputController.ActionPressedEvent -= OnActionPressed;
        InputController.ActionReleasedEvent -= OnActionReleased;
        InputController.LiftPressedEvent -= OnLift;
        InputController.LiftReleasedEvent -= OnDrop;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

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

    protected virtual void OnLift(object sender, EventArgs e)
    {
        Liftable.Lift(_player.Physics);
    }

    protected virtual void OnDrop(object sender, EventArgs e)
    {
        Liftable.Drop(_player.Physics);
    }
}

public class PlayerGroundedState : PlayerNormalState
{
    public override void Enter()
    {
        base.Enter();
        _player.AnimateIdle();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (!_player.Grounded)
        {
            _player.ChangeState<PlayerFlyingState>();
        }
    }

    protected override void OnActionPressed(object sender, EventArgs e)
    {
        Food foodToEat = _player.TryFindFood();
        if (foodToEat != null)
        {
            _player.ChangeState<PlayerEatingState>();
        }
    }

    protected override void OnDownPress()
    {
        _player.DropThroughOneWayObstacle();
    }

    protected override void OnPressJump(object sender, EventArgs e)
    {
        base.OnPressJump(sender, e);
        _player.ChangeState<PlayerFastFlyingState>();
    }
}

public class PlayerFlyingState : PlayerNormalState
{
    public override void Enter()
    {
        base.Enter();
        _player.AnimateFlying();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (_player.Grounded && !Liftable.Lifted)
        {
            _player.ChangeState<PlayerGroundedState>();
        }
    }

    protected override void OnPressJump(object sender, EventArgs e)
    {
        base.OnPressJump(sender, e);
        _player.ChangeState<PlayerFastFlyingState>();
    }
}

public class PlayerFastFlyingState : PlayerFlyingState
{
    private const float fastFlyDuration = .5f;

    private float fastFlyTimer;

    public override void Enter()
    {
        base.Enter();

        fastFlyTimer = 0;
        _player.AnimationSpeed = 2;
    }

    public override void Exit()
    {
        _player.AnimationSpeed = 1;
        base.Exit();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        fastFlyTimer += Time.deltaTime;
        if (fastFlyTimer >= fastFlyDuration)
        {
            _player.ChangeState<PlayerFlyingState>();
        }
    }

    protected override void OnPressJump(object sender, EventArgs e)
    {
        base.OnPressJump(sender, e);
        
        fastFlyTimer = 0;
    }
}

public class PlayerWalkingState : PlayerGroundedState
{
    //TODO
}

public class PlayerEatingState : PlayerGroundedState
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
        if (foodToEat == null)
        {
            _player.ChangeState<PlayerGroundedState>();
            return;
        }

        float calories = foodToEat.eat(Time.deltaTime * _player.EatingSpeed);
        _player.ConsumeCalories(calories);
    }

    protected override void OnActionReleased(object sender, EventArgs e)
    {
        base.OnActionReleased(sender, e);

        _player.ChangeState<PlayerGroundedState>();
    }

    protected override void OnPressJump(object sender, EventArgs e)
    {
        //Disable while eating
    }

    protected override void OnPressMove(object sender, InfoEventArgs<Vector2> e)
    {
        //Disable while eating
    }

    protected override void OnDownPress()
    {
        //Disable while eating
    }
}
