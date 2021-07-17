using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject TutorialPageOne;
    public GameObject TutorialPageTwo;
    public GameObject TutorialPageThree;
    public GameObject TutorialPageFour;
    public GameObject TutorialPageFive;
    public GameObject MainMenuUI;
    public GameObject PauseUI;

    public void GoTutorialPageOne()
    {
        TutorialPageOne.SetActive(true);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(false);
    }

    public void GoTutorialPageTwo()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(true);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(false);
    }

    public void GoTutorialPageThree()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(true);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(false);
    }

    public void GoTutorialPageFour()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(true);
        TutorialPageFive.SetActive(false);
    }

    public void GoTutorialPageFive()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(true);
    }

    public void BackToMainMenu()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void BackToPause()
    {
        TutorialPageOne.SetActive(false);
        TutorialPageTwo.SetActive(false);
        TutorialPageThree.SetActive(false);
        TutorialPageFour.SetActive(false);
        TutorialPageFive.SetActive(false);
        PauseUI.SetActive(true);
    }
}
