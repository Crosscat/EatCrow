using UnityEngine;

public abstract class State : MonoBehaviour
{
    public float _stateTime;

    public virtual void Enter()
    {
        AddListeners();
        _stateTime = 0;
    }

    public virtual void Exit()
    {
        RemoveListeners();
    }

    public virtual void StateUpdate()
    {
        _stateTime += Time.deltaTime;
    }

    protected virtual void OnDestroy()
    {
        RemoveListeners();
    }

    protected virtual void AddListeners()
    {
    }

    protected virtual void RemoveListeners()
    {
    }
}