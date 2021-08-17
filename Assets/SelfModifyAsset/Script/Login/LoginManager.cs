using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    private GameDefaultSetupManager gameDefaultSetupScript;

    public void Start()
    {
        gameDefaultSetupScript = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayMouseClickSound()
    {
        gameDefaultSetupScript.playMouseClickSound();
    }
}
