using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Text threshValueText;
    public Text leftHandValueText;
    public Text rightHandValueText;

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
        leftHandValueText.text = imageDetectionScript.rightFingerCount.ToString();
        rightHandValueText.text = imageDetectionScript.leftFingerCount.ToString();

    }
}
