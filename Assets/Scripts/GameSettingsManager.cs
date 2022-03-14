using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsManager : Singleton<GameSettingsManager>
{
    public List<SettingsButton> gameTypeButtons;
    public List<SettingsButton> playersAmountButtons;
    public List<SettingsButton> courtButtons;
    public int currentSelectedGameTypeButton;
    public int currentSelectedPlayersAmountButton;
    public int currentSelectedCourtButton;

    private void Start()
    {
        UpdateGameSettings();
    }

    public void UpdateGameSettings()
    {
        foreach (SettingsButton button in gameTypeButtons)
        {
            button.SetAsUnselected();
        }

        foreach (SettingsButton button in playersAmountButtons)
        {
            button.SetAsUnselected();
        }

        foreach (SettingsButton button in courtButtons)
        {
            button.SetAsUnselected();
        }

        gameTypeButtons[currentSelectedGameTypeButton].SetAsSelected();
        playersAmountButtons[currentSelectedPlayersAmountButton].SetAsSelected();
        courtButtons[currentSelectedCourtButton].SetAsSelected();
    }

    public void SetGameTypeButton(int gameType)
    {
        currentSelectedGameTypeButton = gameType;
        UpdateGameSettings();
    }

    public void SetPlayersAmountButton(int gameType)
    {
        currentSelectedPlayersAmountButton = gameType;
        UpdateGameSettings();
    }

    public void SetCourtButton(int gameType)
    {
        currentSelectedCourtButton = gameType;
        UpdateGameSettings();
    }

    public void StartNewMatch()
    {
        TennisMatchManager.Instance.SetGameType(currentSelectedGameTypeButton);
        TennisMatchManager.Instance.SetPlayersAmount(currentSelectedPlayersAmountButton);
        TennisMatchManager.Instance.SetCourt(currentSelectedCourtButton);
        MainUI.Instance.StartNewGame();
    }

}
