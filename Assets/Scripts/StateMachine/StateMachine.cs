using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public virtual State CurrentState
    {
        get { return _currentState; }
        set { Transition(value); }
    }

    protected State _currentState;
    protected bool _inTransition;

    public virtual T GetState<T>() where T : State
    {
        T target = GetExactComponent<T>();
        if (target == null)
            target = gameObject.AddComponent<T>();
        return target;
    }

    private T GetExactComponent<T>()
    {
        var comps = GetComponents<T>();
        if (comps.Length > 0)
        {
            foreach (var comp in comps)
            {
                if (comp.GetType() == typeof(T)) return comp;
            }
        }

        return default;
    }

    public virtual void ChangeState<T>() where T : State
    {
        CurrentState = GetState<T>();
    }

    protected virtual void Transition(State value)
    {
        if (_currentState == value || _inTransition)
            return;
        //_inTransition = true;

        if (_currentState != null)
            _currentState.Exit();

        _currentState = value;

        if (_currentState != null)
            _currentState.Enter();

        _inTransition = false;
    }

    public void Update()
    {
        if (_currentState == null) return;

        _currentState.StateUpdate();
    }
}
