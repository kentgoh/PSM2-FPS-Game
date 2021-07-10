using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuUI;
    public GameObject ChooseStageUI;
    public GameObject EasyLeaderboardUI;
    public GameObject NormalLeaderboardUI;
    public GameObject HardLeaderboardUI;
    public GameObject LoginUI;

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackToLogin()
    {
        LoginUI.SetActive(true);
        MainMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChooseStage()
    {
        MainMenuUI.SetActive(false);
        ChooseStageUI.SetActive(true);
    }

    public void ChooseStageBackToMainMenu()
    {
        ChooseStageUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void LeaderboardBackToMainMenu()
    {
        EasyLeaderboardUI.SetActive(false);
        NormalLeaderboardUI.SetActive(false);
        HardLeaderboardUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void GoEasyLeaderboard()
    {
        MainMenuUI.SetActive(false);
        NormalLeaderboardUI.SetActive(false);
        EasyLeaderboardUI.SetActive(true);
    }

    public void GoNormalLeaderboard()
    {
        EasyLeaderboardUI.SetActive(false);
        HardLeaderboardUI.SetActive(false);
        NormalLeaderboardUI.SetActive(true);
    }

    public void GoHardLeaderboard()
    {
        NormalLeaderboardUI.SetActive(false);
        HardLeaderboardUI.SetActive(true);
    }

    public void StartEasyStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void StartNormalStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void StartHardStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
    }
}
