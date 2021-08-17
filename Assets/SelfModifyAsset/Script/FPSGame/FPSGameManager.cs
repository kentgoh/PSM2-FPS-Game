using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FPSGameManager : MonoBehaviour
{
    public int currentArea = 1;
    public int lastArea = 3;
    public bool GameIsPaused = false;
    public bool settingMode = false;
    public int settingCount = 0;
    private bool completeChangeArea = false;
    public double time = 0;
    public bool GameIsCompleted = false;
    public int type = 0;

    //player
    public GameObject player;
    public Text shieldCooldownText;

    //area
    public GameObject area1;
    public GameObject area2;
    public GameObject area3;

    //UI
    public GameObject gameUI;
    public GameObject pauseUI;
    public GameObject settingUI;
    public GameObject gameoverUI;
    public GameObject gamecompleteUI;
    public GameObject aoeImage;

    //Timer
    public Text timer;

    //Play Fab Manager
    public PlayFabManager playFabScript;

    //Game Default Setup Manager
    private GameDefaultSetupManager gameDefaultSetupScript;

    //image detection
    private GameObject cameraInput;
    private ImageDetection imageDetectionScript;
    /*
    public Text leftFingerCountText;
    public Text rightFingerCountText;
    */

    public Text fingerCountText;

    private void Start()
    {
        Time.timeScale = 1;
        StartCoroutine("Counter");

        cameraInput = GameObject.FindGameObjectWithTag("WebcamInput");
        imageDetectionScript = cameraInput.GetComponent<ImageDetection>();
        playFabScript = GameObject.FindGameObjectWithTag("PlayFabManager").GetComponent<PlayFabManager>();

        gameDefaultSetupScript = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>();
        gameDefaultSetupScript.FPSGameMode = true;

    }

    void Update()
    {
        CheckGameCondition();
        DisplayShieldCooldownTimer();
        DisplayFingerCount();
        PauseInput();
    }


    void CheckGameCondition()
    {
        AimingManager areaScript = player.transform.GetComponent<AimingManager>();
        if (areaScript.aoeStrike)
        {
            aoeImage.SetActive(true);
        }
        else
        {
            aoeImage.SetActive(false);
        }

        if (player.GetComponent<PlayerManager>().health <= 0)
        {
            GameOver();
        }

        if (currentArea == 3 && player.transform.position.z >= 35)
        {
            if (areaScript.targetCount == 0)
                GameComplete();
        }
        else if (!completeChangeArea)
        {
            if (areaScript.targetCount == 0)
                GoNextArea();

        }

    }

    void PauseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void GoNextArea()
    {
        if (player.transform.position.z < (-5 + (15 * currentArea)))
        {
            player.transform.position += (transform.forward * 5.0f * Time.deltaTime);
        }
        else if(player.transform.position.z >= (-5 + (15 * currentArea)) && player.transform.position.z < (-5 + (20 * currentArea)))
        {
            player.transform.position += (transform.forward * 5.0f * Time.deltaTime);
            if (currentArea == 1)
                area2.SetActive(true);
            else if (currentArea == 2)
                area3.SetActive(true);
        }
        else
        {
            if (currentArea == 1)
                area1.SetActive(false);
            else if (currentArea == 2)
                area2.SetActive(false);

            currentArea++;
        }

    }

    void Resume()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        cameraInput.SetActive(true);
        imageDetectionScript.startUsingCamera();

    }

    void Pause()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        imageDetectionScript.stopUsingCamera();
        cameraInput.SetActive(false);
    }

    public void PauseToSetting()
    {
        settingMode = true;
        settingCount++;
        pauseUI.SetActive(false);
        settingUI.SetActive(true);
        ImageDetection settingWebcam = settingUI.GetComponentInChildren<ImageDetection>();
        if(settingCount > 1)
        {
            settingWebcam.startUsingCamera();
        }
    }

    public void SettingToPause()
    {
        settingMode = false;
        ImageDetection settingWebcam = settingUI.GetComponentInChildren<ImageDetection>();
        settingWebcam.stopUsingCamera();
        pauseUI.SetActive(true);
        settingUI.SetActive(false);
    }

    void GameOver()
    {
        gameoverUI.SetActive(true);
        Time.timeScale = 0f;
        imageDetectionScript.stopUsingCamera();
        cameraInput.SetActive(false);
    }

    void GameComplete()
    {
        if (!GameIsCompleted)
        {
            gamecompleteUI.SetActive(true);
            Time.timeScale = 0f;
            playFabScript.SendLeaderboard((int)(time * (-1)), type);
            imageDetectionScript.stopUsingCamera();
            cameraInput.SetActive(false);
            GameIsCompleted = true;
        }

    }

    IEnumerator Counter()
    {
        while(Time.timeScale == 1)
        {
            time++;
            timer.text = time.ToString();
            yield return new WaitForSeconds(1f);
        }

    }

    void DisplayFingerCount()
    {
        if (imageDetectionScript.fingerCount == 0)
            fingerCountText.text = "X";

        else
            fingerCountText.text = imageDetectionScript.fingerCount.ToString();

    }

    public void DisplayShieldCooldownTimer()
    {
        ShieldManager shieldScript = player.GetComponentInChildren<ShieldManager>();

        if(shieldScript.shieldCooldown == 5)
        {
            shieldCooldownText.text = " ";
        }
        else
            shieldCooldownText.text = shieldScript.shieldCooldown.ToString();
    }

    public void backToMainMenu()
    {
        gameDefaultSetupScript.FPSGameMode = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayMouseClickSound()
    {
        gameDefaultSetupScript.playMouseClickSound();
    }
}
