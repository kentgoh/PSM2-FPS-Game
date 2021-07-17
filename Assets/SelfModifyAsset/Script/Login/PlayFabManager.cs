using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public InputField loginNameInput;
    public InputField loginPasswordInput;
    public Text loginMessageText;
    public InputField registerNameInput;
    public InputField registerPasswordInput;
    public Text registerMessageText;

    public GameObject rowPrefab;
    public Transform rowsParent;
    public string username;

    //Type 1 = EasyLeaderboard
    //Type 2 = NormalLeaderboard
    //Type 3 = HardLeaderboard
    public int type = 0;
    public Text UsernameText;
    private string leaderboardType;

    void Start()
    {
        switch (type)
        {
            case 0 : { PlayFabLogin();break; }
            case 1 : { leaderboardType = "EasyLeaderboard"; break; }
            case 2: { leaderboardType = "NormalLeaderboard"; break; }
            case 3: { leaderboardType = "HardLeaderboard"; break; }
        }

        if (type == 0)
            DontDestroyOnLoad(this.gameObject);
        //Script use on leaderboard
        if(type > 0 && type < 4)
            GetLeaderboard(leaderboardType);

    }

    //Play Fab Login Functions
    public void PlayFabLogin()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, PlayFabLoginOnSuccess, OnError);
    }

    void PlayFabLoginOnSuccess(LoginResult result)
    {
        Debug.Log("Successful Login To PlayFab Account");
    }

    public void RegisterUser()
    {
        if(registerPasswordInput.text.Length < 6)
        {
            registerMessageText.text = "Password Too Short";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Username = registerNameInput.text,
            Password = registerPasswordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registered and logged in");
        username = registerNameInput.text;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoginUser()
    {
        if (loginPasswordInput.text.Length < 6)
        {
            loginMessageText.text = "Password Too Short";
            return;
        }

        var request = new LoginWithPlayFabRequest
        {
            Username = loginNameInput.text,
            Password = loginPasswordInput.text
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginError);

    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Logged in");
        username = loginNameInput.text;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    //Play Fab Get User Detail Functions
    public void GetUserDetail()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceive, OnError);
    }

    void OnDataReceive(GetUserDataResult result)
    {
            Debug.Log("Player Data Get");
    }

    //Play Fab Send Value to Leaderboard Functions
    public void SendLeaderboard(int score, int type)
    {
        string StatisticName = "";
        switch (type)
        {
            case 1: { StatisticName = "EasyLeaderboard"; break; }
            case 2: { StatisticName = "NormalLeaderboard"; break; }
            case 3: { StatisticName = "HardLeaderboard"; break; }
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = StatisticName,
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, SetupDisplayName, OnError);
    }

    public void SetupDisplayName(UpdatePlayerStatisticsResult result)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = username
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Successful leaderboard send");
    }

    public void GetLeaderboard(string statisticName)
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = 5
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }


    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (Transform item in rowsParent)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            GameObject newGO = Instantiate(rowPrefab, rowsParent);
            Text[] texts = newGO.GetComponentsInChildren<Text>();
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = (item.StatValue * (-1)).ToString();
        }
    }
    void OnError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    void OnRegisterError(PlayFabError error)
    {
        registerMessageText.text = "Username existed";
    }

    void OnLoginError(PlayFabError error)
    {
        loginMessageText.text = "Wrong value";
    }

}
