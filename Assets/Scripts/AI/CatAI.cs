using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class CatAI : StateMachine
{
    public Methodology methodology;
    public float walkSpeed=1;
    public float runSpeed=2;
    public float moveAcceleration;
    public float agroRange = 3f;
    public float deAgroRange = 9f;
    public float attackRange = .7f;
    public float attackDuration = .4f;

    public Physics physics { get; private set; }
    public AIZone walkZone { get; private set; }
    public PlayerController player { get; private set; }
    public StateSelector stateSelector { get; private set; }

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public Vector2 deltaToPlayer
    {
        get { return player.transform.position - transform.position; }
    }
    
    public float sqrDistanceToPlayer
    {
        get { return deltaToPlayer.sqrMagnitude; }
    }

    public Vector2 Velocity
    {
        get { return physics.Velocity; }
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
        Run = 1,
        Attack = 1,
    }

    private void Awake()
    {
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
                Passive = () => ChangeState<CatWanderState>(),
                Chase = () => ChangeState<CatChaseState>(),
                Attack = () => ChangeState<CatAttackState>(),
            }; break;
        }
    }

    private void Start()
    {
        player = GameObject.FindObjectOfType<PlayerController>();
        physics = GetComponent<Physics>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //TODO: set this properly
        //Currently just picks the closest zone
        walkZone = GameObject.FindObjectsOfType<AIZone>()
                             .Where(it => it.walkable)
                             .OrderBy(it => (it.transform.position - this.transform.position).sqrMagnitude)
                             .First();

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
        animator.SetInteger("State", (int)animation);
    }

    public void Walk(Vector2 direction)
    {
        physics.Move(direction, walkSpeed, moveAcceleration);
    }

    public void Run(Vector2 direction)
    {
        physics.Move(direction, runSpeed, moveAcceleration);
    }

    public void Stop()
    {
        physics.Move(Vector2.one, 0f, moveAcceleration);
    }

    public bool PlayerInsideKillRadius()
    {
        return sqrDistanceToPlayer < attackRange*attackRange;
    }

    public bool PlayerInsideAgroRadius()
    {
        return sqrDistanceToPlayer < agroRange*agroRange;
    }

    public bool PlayerInsideWalkzoneX()
    {
        return walkZone.Rect.xMin < player.transform.position.x 
                                 && player.transform.position.x < walkZone.Rect.xMax;
    }

    public bool PlayerOutsideDeAgroRadius()
    {
        return sqrDistanceToPlayer > deAgroRange*deAgroRange;
    }

    public override void Update()
    {
        base.Update();

        if (Velocity.x > 0) spriteRenderer.flipX = true;
        else if (Velocity.x < 0) spriteRenderer.flipX = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, deAgroRange);

        if (walkZone != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position + Vector3.up, walkZone.Rect.bottomLeft());
            Gizmos.DrawLine(transform.position + Vector3.up, walkZone.Rect.bottomRight());
        }
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
    private float target;

    private const float TARGET_SATISFACTION = .2f;

    public override void Enter()
    {
        base.Enter();

        catAI.Animate(CatAI.Animation.Walk);
        SelectTarget();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (catAI.PlayerInsideAgroRadius() && catAI.PlayerInsideWalkzoneX())
        {
            catAI.stateSelector.Chase();
            return;
        }

        while (Mathf.Abs(target - catAI.transform.position.x) < TARGET_SATISFACTION)
        {
            SelectTarget();
        }

        Debug.DrawLine(catAI.transform.position, new Vector2(target, catAI.transform.position.y), Color.blue);

        catAI.Walk(Mathf.Sign(target - catAI.transform.position.x) * Vector2.right);
    }

    private void SelectTarget()
    {
        target = UnityEngine.Random.Range(catAI.walkZone.Rect.xMin, catAI.walkZone.Rect.xMax);
    }
}

public class CatChaseState : CatAIState
{
    private const float xSatisfaction = .2f;

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (catAI.PlayerOutsideDeAgroRadius() || !catAI.PlayerInsideWalkzoneX())
        {
            catAI.stateSelector.Passive();
            return;
        }

        if (catAI.PlayerInsideKillRadius())
        {
            catAI.stateSelector.Attack();
            return;
        }

        //Don't move if already very close to crow's x position, 
        //such as when the crow flies directly above
        if (Mathf.Abs(catAI.deltaToPlayer.x) > xSatisfaction)
        {
            catAI.Animate(CatAI.Animation.Run);
            catAI.Run(Mathf.Sign(catAI.deltaToPlayer.x) * Vector2.right);
        }
        else
        {
            catAI.Animate(CatAI.Animation.Idle);
            catAI.Stop();
        }

        Debug.DrawLine(catAI.transform.position, catAI.player.transform.position, Color.yellow);
    }
}

public class CatAttackState : CatAIState
{
    public override void Enter()
    {
        base.Enter();
        catAI.Animate(CatAI.Animation.Attack);

        Debug.Log("Cat is Attacking!");
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

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

public class CatNoopState : CatAIState 
{
    public override void Enter()
    {
        base.Enter();
        catAI.Animate(CatAI.Animation.Idle);
    }
}
