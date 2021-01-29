using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class CatAI : StateMachine
{
    public float walkSpeed=1;
    public float runSpeed=2;
    public float moveAcceleration;

    private Physics physics;
    private AIZone walkZone;

    private void Start()
    {
        walkZone = GameObject.FindObjectsOfType<AIZone>().First(it => it.walkable);
        physics = GetComponent<Physics>();

        ChangeState<CatWanderState>();
    }

    new public void ChangeState<T>() where T : CatAIState
    {
        CatAIState nextState = GetState<T>();
        nextState.Init(this, walkZone);
        CurrentState = nextState;
    }

    public void Walk(Vector2 direction)
    {
        physics.Move(direction, walkSpeed, moveAcceleration);
    }

    public void Run(Vector2 direction)
    {
        physics.Move(direction, runSpeed, moveAcceleration);
    }

}

public abstract class CatAIState : State
{
    protected PlayerController player;
    protected CatAI catAI;
    protected AIZone walkZone;

    public void Init(CatAI catAI, AIZone walkZone)
    {
        this.catAI = catAI;
        this.walkZone = walkZone;
        player = GameObject.FindObjectOfType<PlayerController>();
    }
}

public class CatWanderState : CatAIState
{
    private float target;

    private const float TARGET_SATISFACTION = .25f;

    public override void Enter()
    {
        base.Enter();

        SelectTarget();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        while (Mathf.Abs(target-catAI.transform.position.x) < TARGET_SATISFACTION) 
        {
            SelectTarget();
        }

        Debug.DrawLine(catAI.transform.position, new Vector2(target, catAI.transform.position.y), Color.blue);

        catAI.Walk(Mathf.Sign(target - catAI.transform.position.x) * Vector2.right);
    }

    private void SelectTarget()
    {
        target = Random.Range(walkZone.Rect.xMin, walkZone.Rect.xMax);
    }
}