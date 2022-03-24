using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUI : Singleton<MainUI>
{
    public GameObject settingsMenu;
    public GameObject howToPlayMenu;
    [Space]
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI mainScoreText;
    public TextMeshProUGUI lethalBallSpeedText;
    public TextMeshProUGUI displayMessageText;
    //public TextMeshProUGUI team1SpecialPercentText;
    //public TextMeshProUGUI team2SpecialPercentText;
    //public Image team1SpecialBar;
    //public Image team2SpecialBar;
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
        var device = InputReader.Device.Any;
        InputReader.GetInput(device, input);

        if (input.PressedStart)
        {
            OpenSettingsMenu();
        }
    }

    public void OpenSettingsMenu()
    {
        TennisMatchManager.Instance.ResetSpecials();
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
        UpdateTexts();
        PlayDisplayMessage("Team 1 Serves!", Color.green);
        CloseSettingsMenu();
    }

    public void HowToPlayButton()
    {
        howToPlayMenu.SetActive(true);
    }

    public void HowToPlayBackButton()
    {
        howToPlayMenu.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void UpdateTexts()
    {
        TennisMatchManager tennisManager = TennisMatchManager.Instance;
        currentScoreText.text = tennisManager.team1CurrentScoreString + tennisManager.team2CurrentScoreString;
        mainScoreText.text = tennisManager.team1MainScore + "-" + tennisManager.team2MainScore;
        lethalBallSpeedText.gameObject.SetActive(tennisManager.currentGameType == GameTypeID.Lethal ? true : false);
        lethalBallSpeedText.text = "Ball Speed: " + tennisManager.tennisBall.speed.ToString("0.0") + " MPH";
        //team1SpecialPercentText.text = tennisManager.team1Special.ToString("0") + "%";
        //team2SpecialPercentText.text = tennisManager.team2Special.ToString("0") + "%";
        //team1SpecialBar.fillAmount = (float)tennisManager.team1Special / 100;
        //team2SpecialBar.fillAmount = (float)tennisManager.team2Special / 100;
        tennisManager.players[0].specialStars.UpdateStars(tennisManager.team1Special);
        tennisManager.players[1].specialStars.UpdateStars(tennisManager.team1Special);
        tennisManager.players[2].specialStars.UpdateStars(tennisManager.team2Special);
        tennisManager.players[3].specialStars.UpdateStars(tennisManager.team2Special);
    }

    public void PlayDisplayMessage(string displayMessageString, Color textColor)
    {
        displayMessage.SetActive(false);
        displayMessage.SetActive(true);
        displayMessageText.text = displayMessageString;
        displayMessageText.color = textColor;
    }
}
