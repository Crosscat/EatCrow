using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    private void Awake()
    {
        AudioController.Instance.PlayMusic(MusicEnum.LowIntensity, .2f);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Action"))
        {
            SceneManager.LoadScene("GameScene 1");
        }
    }
}
