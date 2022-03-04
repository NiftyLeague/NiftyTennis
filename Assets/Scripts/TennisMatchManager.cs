using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisMatchManager : Singleton<TennisMatchManager>
{
    public int team1MainScore = 0;
    public int team2MainScore = 0;
    public int team1CurrentScore = 0;
    public int team2CurrentScore = 0;
    public string team1CurrentScoreString = "0";
    public string team2CurrentScoreString = "0";
    [Space]
    public List<PlayerController> players;
    [Space]
    public TennisBall tennisBall;
    public bool ballBouncedOnCourt = false;

    public void Score(TeamID teamID)
    {
        switch (teamID)
        {
            case TeamID.team1:
                if (ballBouncedOnCourt)
                {
                    team1CurrentScore++;
                }
                if (team1CurrentScore > 3 && team2CurrentScore < 3)
                {
                    team1MainScore++;
                    team1CurrentScore = 0;
                    team2CurrentScore = 0;
                }
                tennisBall.ResetBall(PlayerID.Player1);
                //tennisBall.HitBall(HitBy.Opponent);
                break;
            case TeamID.team2:
                if (ballBouncedOnCourt)
                {
                    team2CurrentScore++;
                }
                if (team2CurrentScore > 3 && team1CurrentScore < 3)
                {
                    team2MainScore++;
                    team1CurrentScore = 0;
                    team2CurrentScore = 0;
                }
                tennisBall.ResetBall(PlayerID.Player1);
                //tennisBall.HitBall(HitBy.Player);
                break;
        }

        if (team1CurrentScore == 4 && team2CurrentScore == 4)
        {
            team1CurrentScore = 3;
            team2CurrentScore = 3;
        }

        GetCurrentScore();

        if (team1CurrentScore == 3 && team2CurrentScore == 3)
        {
            team1CurrentScoreString = "Deuce";
            team2CurrentScoreString = "";
        }

        MainUI.Instance.UpdateScores();
    }

    public void GetCurrentScore()
    {
        team1CurrentScoreString = GetScoreString(team1CurrentScore) + "-";
        team2CurrentScoreString = GetScoreString(team2CurrentScore);
    }

    string GetScoreString(int score)
    {
        string scoreString = "0";

        switch (score)
        {
            case 1:
                scoreString = "15";
                break;
            case 2:
                scoreString = "30";
                break;
            case 3:
                scoreString = "40";
                break;
            case 4:
                scoreString = "Adv";
                break;
        }

        return scoreString;
    }

    public PlayerController GetPlayer(PlayerID player)
    {
        PlayerController playerControllerToGet = players[0];

        foreach (PlayerController playerController in players)
        {
            if (playerController.playerID == player)
            {
                playerControllerToGet = playerController;
            }
        }

        return playerControllerToGet;
    }
}

public enum TeamID
{
    team1,
    team2,
}

public enum PlayerID
{
    Player1,
    Player2,
    Player3,
    Player4,
}
