using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayLeaderboard : MonoBehaviour
{
    public GameObject PlayFabManager;
    public GameObject rowPrefab;
    public Transform rowsParent;

    void Start()
    {
        PlayFabManager script = PlayFabManager.GetComponent<PlayFabManager>();
        script.GetLeaderboard("EasyLeaderboard");

    }

    void DisplayLeaderboardValue()
    {

    }
}
