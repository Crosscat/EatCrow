using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : StateMachine
{
    public List<CatAI> cats;

    public static Game Current { get; private set; }


    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        
    }

    private void InitializeGame()
    {

    }

}

public class MainGameState : State
{

}
