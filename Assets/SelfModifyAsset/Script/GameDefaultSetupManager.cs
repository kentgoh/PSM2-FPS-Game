using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDefaultSetupManager : MonoBehaviour
{
    public int threshValue = 100;
    public Slider threshSlider;
    public bool FPSGameMode = false;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        settingManager();

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
        else
        {

        }
    }
}
