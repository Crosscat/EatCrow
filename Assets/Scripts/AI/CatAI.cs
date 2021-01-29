using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class CatAI : StateMachine
{
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

    public Vector2 deltaToPlayer
    {
        get { return player.transform.position - transform.position; }
    }
    
    public float sqrDistanceToPlayer
    {
        get { return deltaToPlayer.sqrMagnitude; }
    }

    private void Start()
    {
        player = GameObject.FindObjectOfType<PlayerController>();
        physics = GetComponent<Physics>();

        //TODO: set this properly
        walkZone = GameObject.FindObjectsOfType<AIZone>().First(it => it.walkable);

        ChangeState<CatWanderState>();
    }

    new public void ChangeState<T>() where T : CatAIState
    {
        CatAIState nextState = GetState<T>();
        nextState.catAI = this;
        CurrentState = nextState;
    }

    private void Walk(Vector2 direction)
    {
        physics.Move(direction, walkSpeed, moveAcceleration);
    }

    private void Run(Vector2 direction)
    {
        physics.Move(direction, runSpeed, moveAcceleration);
    }

    private void Stop()
    {
        physics.Move(Vector2.one, 0f, moveAcceleration);
    }

    private bool PlayerInsideKillRadius()
    {
        return sqrDistanceToPlayer < attackRange*attackRange;
    }

    private bool PlayerInsideAgroRadius()
    {
        return sqrDistanceToPlayer < agroRange*agroRange;
    }

    private bool PlayerInsideWalkzoneX()
    {
        return walkZone.Rect.xMin < player.transform.position.x 
                                 && player.transform.position.x < walkZone.Rect.xMax;
    }

    private bool PlayerOutsideDeAgroRadius()
    {
        return sqrDistanceToPlayer > deAgroRange*deAgroRange;
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

            SelectTarget();
        }

        public override void StateUpdate()
        {
            base.StateUpdate();

            if (catAI.PlayerInsideAgroRadius() && catAI.PlayerInsideWalkzoneX())
            {
                catAI.ChangeState<CatChaseState>();
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
            target = Random.Range(catAI.walkZone.Rect.xMin, catAI.walkZone.Rect.xMax);
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
                catAI.ChangeState<CatWanderState>();
                return;
            }

            if (catAI.PlayerInsideKillRadius())
            {
                catAI.ChangeState<CatAttackState>();
                return;
            }

            //Don't move if already very close to crow's x position, 
            //such as when the crow flies directly above
            if (Mathf.Abs(catAI.deltaToPlayer.x) > xSatisfaction)
            {
                catAI.Run(Mathf.Sign(catAI.deltaToPlayer.x) * Vector2.right);
            }
            else
            {
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
                    catAI.ChangeState<CatNoopState>();
                } 
                else
                {
                    Debug.Log("Cat missed its attack");
                    catAI.ChangeState<CatChaseState>();
                }
            }
        }
    }

    public class CatNoopState : CatAIState { }

}


