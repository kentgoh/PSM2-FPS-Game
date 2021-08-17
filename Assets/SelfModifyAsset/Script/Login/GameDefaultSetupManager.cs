using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameDefaultSetupManager : MonoBehaviour
{
    public int threshValue = 100;
    public Slider threshSlider;
    public bool FPSGameMode = false;

    private AudioSource BGM1;
    private AudioSource BGM2;
    private AudioSource MouseClick;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        BGM1 = transform.Find("BGM1").GetComponent<AudioSource>();
        BGM2 = transform.Find("BGM2").GetComponent<AudioSource>();
        MouseClick = transform.Find("MouseClick").GetComponent<AudioSource>();

        BGM1.Play();
    }

    void Update()
    {
        settingManager();

        if (FPSGameMode && BGM1.isPlaying)
        {
            BGM1.Stop();
            BGM2.Play();
        }
        else if(!FPSGameMode && BGM2.isPlaying)
        {
            BGM1.Play();
            BGM2.Stop();
        }

    }

    public void changeThreshValue()
    {
        threshValue = (int)threshSlider.value;
    }

    public void settingManager()
    {
        if (FPSGameMode)
        {
            if (GameObject.FindGameObjectWithTag("GameController").GetComponent<FPSGameManager>().settingMode)
            {
                threshSlider = GameObject.FindGameObjectWithTag("ThreshValueSlider").GetComponent<Slider>();

                if (threshSlider != null)
                    changeThreshValue();
            }

        }
        else if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManager>().settingMode)
            {
                threshSlider = GameObject.FindGameObjectWithTag("ThreshValueSlider").GetComponent<Slider>();

                if (threshSlider != null)
                    changeThreshValue();
            }
        }
    }

    public void playMouseClickSound()
    {
        MouseClick.Play();
    }
}
