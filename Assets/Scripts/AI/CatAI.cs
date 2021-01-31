using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class CatAI : AIAgent
{
    public Methodology methodology;
    public float walkSpeed=1;
    public float runSpeed=2;
    public float jumpPower = 10f;
    public float moveAcceleration;
    public float agroRange = 3f;
    public float deAgroRange = 9f;
    public float attackRange = .7f;
    public float attackDuration = .4f;

    public Physics physics { get; private set; }
    public PlayerController player { get; private set; }
    public StateSelector stateSelector { get; private set; }

    public const float TARGET_SATISFACTION = .2f;
    public  const float TARGET_SATISFACTION_SQR = TARGET_SATISFACTION * TARGET_SATISFACTION;

    public Vector2 deltaToPlayer
    {
        get { return player.transform.position - transform.position; }
    }
    
    public float sqrDistanceToPlayer
    {
        get { return deltaToPlayer.sqrMagnitude; }
    }

    public enum Methodology
    {
        Standard,
        Hunter
    }

    public enum Animation
    {
        Idle = 0,
        Walk = 1,
        Attack = 2,
        Run = 3,
    }

    public override void Awake()
    {
        base.Awake();

        switch(methodology)
        {
            case Methodology.Standard: stateSelector = new StateSelector()
            {
                Idle = () => ChangeState<CatNoopState>(),
                Passive = () => ChangeState<CatWanderState>(),
                Chase = () => ChangeState<CatChaseState>(),
                Attack = () => ChangeState<CatAttackState>(),
            }; break;

            case Methodology.Hunter: stateSelector = new StateSelector()
            {
                Idle = () => ChangeState<CatNoopState>(),
                Passive = () => ChangeState<CatHuntingState>(),
                Chase = () => ChangeState<CatChaseState>(),
                Attack = () => ChangeState<CatAttackState>(),
            }; break;
        }
    }

    private void Start()
    {
        player = GameObject.FindObjectOfType<PlayerController>();
        physics = GetComponent<Physics>();

        //TODO: set this properly
        //Currently just picks the closest zone
        //walkZone = GameObject.FindObjectsOfType<AIZone>()
        //                     .Where(it => it.walkable)
        //                     .OrderBy(it => (it.transform.position - this.transform.position).sqrMagnitude)
        //                     .First();

        stateSelector.Passive();
    }

    new public void ChangeState<T>() where T : CatAIState
    {
        CatAIState nextState = GetState<T>();
        nextState.catAI = this;
        CurrentState = nextState;
    }

    public void Animate(Animation animation)
    {
        _animator.SetInteger("State", (int)animation);
    }

    public void WalkToTarget(Vector2 target)
    {
        MoveToTarget(target, walkSpeed);
    }

    public void RunToTarget(Vector2 target)
    {
        MoveToTarget(target, runSpeed);
    }

    private void MoveToTarget(Vector2 target, float speed)
    {
        if ((transform.position2() - target).sqrMagnitude < TARGET_SATISFACTION_SQR)
            return;

        Vector2 waypoint = WaypointToTarget(target);

        float xDist = Mathf.Abs(waypoint.x - transform.position.x);
        float yDelta= waypoint.y - transform.position.y;

        if (physics.Grounded && xDist > TARGET_SATISFACTION)
        {
            Vector2 direction = Mathf.Sign(waypoint.x - transform.position.x) * Vector2.right;
            Move(direction, speed, moveAcceleration);
        }

        if (physics.Grounded 
            && xDist < 1f
            && yDelta > attackRange
            && yDelta < 8)
        {
            physics.Jump(jumpPower);
        }

        if (physics.Grounded
            && xDist < 1f
            && yDelta < 2)
        {
            DropThroughOneWayObstacle();
        }
    }

    public void Stop()
    {
        Move(Vector2.one, 0f, moveAcceleration);
    }

    public bool PlayerInsideKillRadius()
    {
        return sqrDistanceToPlayer < attackRange*attackRange;
    }

    public bool PlayerInsideAgroRadius()
    {
        return sqrDistanceToPlayer < agroRange*agroRange;
    }

    public bool PlayerOutsideDeAgroRadius()
    {
        return sqrDistanceToPlayer > deAgroRange*deAgroRange;
    }

    public override void Update()
    {
        base.Update();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, deAgroRange);
    }

    private void OnDrawGizmosSelected()
    {

    }

    public class StateSelector
    {
        public Action Idle;
        public Action Passive;
        public Action Chase;
        public Action Attack;
    }
}

public abstract class CatAIState : State
{
    public CatAI catAI;
}

public class CatWanderState : CatAIState
{
    private Vector2 target;

    public override void Enter()
    {
        base.Enter();

        catAI.Animate(CatAI.Animation.Walk);
        SelectTarget();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (catAI.PlayerInsideAgroRadius())
        {
            catAI.stateSelector.Chase();
            return;
        }

        var asdf = (target - catAI.transform.position2()).sqrMagnitude;

        while ((target - catAI.transform.position2()).sqrMagnitude < CatAI.TARGET_SATISFACTION_SQR)
        {
            SelectTarget();
        }

        Debug.DrawLine(catAI.transform.position + Vector3.up, target, Color.blue);

        catAI.WalkToTarget(target);
    }

    private void SelectTarget()
    {
        target = catAI.pathWeb.InterpolatedPoints.PickRandom();
    }
}

public class CatChaseState : CatAIState
{
    public override void StateUpdate()
    {
        base.StateUpdate();

        if (catAI.PlayerOutsideDeAgroRadius())
        {
            catAI.stateSelector.Passive();
            return;
        }

        if (catAI.PlayerInsideKillRadius())
        {
            catAI.stateSelector.Attack();
            return;
        }

        catAI.Animate(CatAI.Animation.Run);
        catAI.RunToTarget(catAI.player.transform.position);        

        Debug.DrawLine(catAI.transform.position, catAI.player.transform.position, Color.yellow);
    }
}

public class CatAttackState : CatAIState
{
    public override void Enter()
    {
        base.Enter();
        catAI.Animate(CatAI.Animation.Attack);

        AudioController.Instance.PlaySound(SoundEnum.CatAttack);

        Debug.Log("Cat is Attacking!");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        catAI.Stop();
        Debug.DrawLine(catAI.transform.position, catAI.player.transform.position, Color.red);

        if (_stateTime >= catAI.attackDuration)
        {
            if (catAI.PlayerInsideKillRadius())
            {
                Debug.Log("CAT KILLED THE CROW!");
                catAI.Stop();
                catAI.stateSelector.Idle();
            } 
            else
            {
                Debug.Log("Cat missed its attack");
                catAI.stateSelector.Chase();
            }
        }
    }
}

public class CatHuntingState : CatAIState
{
    private const float X_SATISFACTION = .2f;

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (catAI.PlayerInsideAgroRadius())
        {
            catAI.stateSelector.Chase();
            return;
        }

        catAI.Animate(CatAI.Animation.Run);
        catAI.WalkToTarget(catAI.player.transform.position);
    }
}

public class CatNoopState : CatAIState 
{
    public override void Enter()
    {
        base.Enter();
        catAI.Animate(CatAI.Animation.Idle);
    }
}
