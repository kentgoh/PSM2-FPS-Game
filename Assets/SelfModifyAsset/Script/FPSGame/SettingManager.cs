using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Text threshValueText;
    public Text HandValueText;

    private GameDefaultSetupManager gameDefaultSetupScript;
    private ImageDetection imageDetectionScript;

    void Start()
    {
        gameDefaultSetupScript = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>();
        imageDetectionScript = this.gameObject.GetComponentInChildren<ImageDetection>();
    }

    void Update()
    {
        threshValueText.text = gameDefaultSetupScript.threshValue.ToString();
        HandValueText.text = imageDetectionScript.fingerCount.ToString();

    }
}
