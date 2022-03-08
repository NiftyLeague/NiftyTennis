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
    [HideInInspector] public bool ballBouncedOnCourt = false;
    [HideInInspector] public bool leftSideServe = true;
    [HideInInspector] public TeamID servingTeam = TeamID.team1;

    public void Score(TeamID teamID)
    {
        switch (teamID)
        {
            case TeamID.team1:
                if (ballBouncedOnCourt)
                {
                    team1CurrentScore++;
                    MainUI.Instance.PlayDisplayMessage("Team 1 Scores!", Color.cyan);
                }
                else
                {
                    MainUI.Instance.PlayDisplayMessage("Ball Didn't Bounce!", Color.white);
                }
                if (team1CurrentScore > 3 && team2CurrentScore < 3)
                {
                    team1MainScore++;
                    ChangeServingTeam();
                    MainUI.Instance.PlayDisplayMessage("Game, Set, Match: Team 1!", Color.yellow);
                    team1CurrentScore = 0;
                    team2CurrentScore = 0;
                }
                break;
            case TeamID.team2:
                if (ballBouncedOnCourt)
                {
                    team2CurrentScore++;
                    MainUI.Instance.PlayDisplayMessage("Team 2 Scores!", Color.cyan);
                }
                else
                {
                    MainUI.Instance.PlayDisplayMessage("Ball Didn't Bounce!", Color.white);
                }
                if (team2CurrentScore > 3 && team1CurrentScore < 3)
                {
                    team2MainScore++;
                    ChangeServingTeam();
                    MainUI.Instance.PlayDisplayMessage("Game, Set, Match: Team 2!", Color.yellow);
                    team1CurrentScore = 0;
                    team2CurrentScore = 0;
                }
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

            MainUI.Instance.PlayDisplayMessage("DEUCE!", Color.red);
        }

        tennisBall.DeadBall();
        MainUI.Instance.UpdateScores();
        StartCoroutine(NextRoundStarting());
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

    void ChangeServingTeam()
    {
        switch (servingTeam)
        {
            case TeamID.team1:
                servingTeam = TeamID.team2;
                break;
            case TeamID.team2:
                servingTeam = TeamID.team1;
                break;
        }
    }

    public IEnumerator NextRoundStarting()
    {
        yield return new WaitForSeconds(2);
        PlayerID playerToServeNext = PlayerID.Player1;
        PlayerID playerToOpposeServe = PlayerID.Player2;
        switch (servingTeam)
        {
            case TeamID.team1:
                playerToServeNext = PlayerID.Player1;
                playerToOpposeServe = PlayerID.Player2;
                MainUI.Instance.PlayDisplayMessage("Team 1 Serves!", Color.green);
                break;
            case TeamID.team2:
                playerToServeNext = PlayerID.Player2;
                playerToOpposeServe = PlayerID.Player1;
                MainUI.Instance.PlayDisplayMessage("Team 2 Serves!", Color.green);
                break;
        }
        tennisBall.ResetBall(playerToServeNext);
        GetPlayer(playerToServeNext).SetState(PlayState.Serving);
        GetPlayer(playerToOpposeServe).SetState(PlayState.OpposingServe);
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
