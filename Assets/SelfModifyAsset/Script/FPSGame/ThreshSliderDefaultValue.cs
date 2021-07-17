using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreshSliderDefaultValue : MonoBehaviour
{
    public Text threshValueText;

    void Awake()
    {
        this.gameObject.GetComponent<Slider>().value = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>().threshValue;
        threshValueText.text = this.gameObject.GetComponent<Slider>().value.ToString();
    }

}
