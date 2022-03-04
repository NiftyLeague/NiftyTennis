using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainUI : Singleton<MainUI>
{
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI mainScoreText;

    private void Start()
    {
        UpdateScores();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void UpdateScores()
    {
        currentScoreText.text = TennisMatchManager.Instance.team1CurrentScoreString + TennisMatchManager.Instance.team2CurrentScoreString;
        mainScoreText.text = TennisMatchManager.Instance.team1MainScore + "-" + TennisMatchManager.Instance.team2MainScore;
    }
}
