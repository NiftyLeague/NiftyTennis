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
    public ServingSignUI servingSign;
    public CameraBehaviour mainCamera;

    private InputState input = new InputState();

    private void Start()
    {
        OpenSettingsMenu();
    }

    private void Update()
    {
        var device = InputReader.Device.Gamepad1;
        InputReader.GetInput(device, input);

        if (input.PressedStart)
        {
            OpenSettingsMenu();
        }
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void StartNewGame()
    {
        TennisMatchManager.Instance.ResetScores();
        UpdateScores();
        PlayDisplayMessage("Team 1 Serves!", Color.green);
        CloseSettingsMenu();
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
