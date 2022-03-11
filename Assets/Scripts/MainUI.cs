using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainUI : Singleton<MainUI>
{
    public GameObject settingsMenu;
    [Space]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI mainScoreText;
    public TextMeshProUGUI displayMessageText;
    public GameObject displayMessage;
    public ServingBarUI servingBar;

    private void Start()
    {
        TennisMatchManager.Instance.GetCurrentScore();
        UpdateScores();

        //Start Match
        TennisMatchManager.Instance.GetPlayer(PlayerID.Player1).SetState(PlayState.Serving);
        TennisMatchManager.Instance.GetPlayer(PlayerID.Player2).SetState(PlayState.OpposingServe);
        PlayDisplayMessage("Team 1 Serves!", Color.green);

        SettingsMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingsMenu();
        }
    }

    public void SettingsMenu()
    {
        settingsMenu.SetActive(!settingsMenu.activeInHierarchy);

        if (settingsMenu.activeInHierarchy)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void UpdateScores()
    {
        currentScoreText.text = TennisMatchManager.Instance.team1CurrentScoreString + TennisMatchManager.Instance.team2CurrentScoreString;
        mainScoreText.text = TennisMatchManager.Instance.team1MainScore + "-" + TennisMatchManager.Instance.team2MainScore;
    }

    public void PlayDisplayMessage(string displayMessageString, Color textColor)
    {
        displayMessage.SetActive(false);
        displayMessage.SetActive(true);
        displayMessageText.text = displayMessageString;
        displayMessageText.color = textColor;
    }
}
