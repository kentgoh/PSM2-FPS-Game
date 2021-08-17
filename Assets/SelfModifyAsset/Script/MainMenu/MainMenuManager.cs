using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject MainMenuUI;
    public GameObject ChooseStageUI;

    public GameObject EasyLeaderboardUI;
    public GameObject NormalLeaderboardUI;
    public GameObject HardLeaderboardUI;

    public GameObject TutorialPageOneUI;
    public GameObject SettingUI;

    public Text playerName;
    public int settingCount = 0;
    public bool settingMode = false;

    private GameDefaultSetupManager gameDefaultSetupScript;
    private PlayFabManager playFabScript;

    public void Start()
    {
        playFabScript = GameObject.FindGameObjectWithTag("PlayFabManager").GetComponent<PlayFabManager>();
        gameDefaultSetupScript = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>();
        playerName.text = playFabScript.username;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackToLogin()
    {
        Destroy(GameObject.FindGameObjectWithTag("PlayFabManager"));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void ChooseStage()
    {
        MainMenuUI.SetActive(false);
        ChooseStageUI.SetActive(true);
    }

    public void Tutorial()
    {
        MainMenuUI.SetActive(false);
        TutorialPageOneUI.SetActive(true);
    }

    public void MainMenuToSetting()
    {
        settingMode = true;
        settingCount++;
        MainMenuUI.SetActive(false);
        SettingUI.SetActive(true);
        ImageDetection settingWebcam = SettingUI.GetComponentInChildren<ImageDetection>();
        if (settingCount > 1)
        {
            settingWebcam.startUsingCamera();
        }
    }

    public void SettingToMainMenu()
    {
        settingMode = false;
        ImageDetection settingWebcam = SettingUI.GetComponentInChildren<ImageDetection>();
        settingWebcam.stopUsingCamera();
        MainMenuUI.SetActive(true);
        SettingUI.SetActive(false);
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

    public void PlayMouseClickSound()
    {
        gameDefaultSetupScript.playMouseClickSound();
    }
}
