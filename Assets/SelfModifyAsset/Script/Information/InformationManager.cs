using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InformationManager : MonoBehaviour
{
    private GameDefaultSetupManager gameDefaultSetupScript;

    public void Start()
    {
        gameDefaultSetupScript = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>();
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
