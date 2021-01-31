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

        AudioController.Instance.PlayMusic(MusicEnum.LowIntensity, .2f);
        AudioController.Instance.PlayMusic(MusicEnum.MediumIntensity, 0f);
        AudioController.Instance.PlayMusic(MusicEnum.HighIntensity, 0f);
    }

    public override void Update()
    {
        base.Update();
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
