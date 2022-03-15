using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    [HideInInspector] public GameTypeID currentGameType = GameTypeID.Normal;
    [Space]
    public TennisBall tennisBall;
    [HideInInspector] public bool ballBouncedOnCourt = false;
    [HideInInspector] public bool leftSideServe = true;
    [HideInInspector] public TeamID servingTeam = TeamID.team1;
    [Space]
    public List<CourtStage> courts;
    public Renderer courtRenderer;
    public float ballSpeedModifier = 1;
    public float playerSpeedModifier = 1;
    [Space]
    public float lethalModeSpeedModifier = 1;
    [HideInInspector] public float totalBallSpeedModifier = 1; 

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
                    MainUI.Instance.PlayDisplayMessage("Ball Didn't Bounce On Court!", Color.white);
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
                    MainUI.Instance.PlayDisplayMessage("Ball Didn't Bounce On Court!", Color.white);
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

        ResetLethalSpeed();
        tennisBall.DeadBall();
        MainUI.Instance.UpdateScores();
        StartCoroutine(NextRoundStarting());
    }

    public void GetCurrentScore()
    {
        team1CurrentScoreString = GetScoreString(team1CurrentScore) + "-";
        team2CurrentScoreString = GetScoreString(team2CurrentScore);
    }

    public void ResetScores()
    {
        team1MainScore = 0;
        team2MainScore = 0;
        team1CurrentScore = 0;
        team2CurrentScore = 0;
        GetCurrentScore();
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
        switch (servingTeam)
        {
            case TeamID.team1:
                GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player2).SetState(PlayState.SameTeamAsServe);
                GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                GetPlayer(PlayerID.Player4).SetState(PlayState.OpposingServe);
                tennisBall.ResetBall(PlayerID.Player1);
                MainUI.Instance.PlayDisplayMessage("Team 1 Serves!", Color.green);
                break;
            case TeamID.team2:
                GetPlayer(PlayerID.Player1).SetState(PlayState.OpposingServe);
                GetPlayer(PlayerID.Player2).SetState(PlayState.OpposingServe);
                GetPlayer(PlayerID.Player3).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player4).SetState(PlayState.SameTeamAsServe);
                tennisBall.ResetBall(PlayerID.Player3);
                MainUI.Instance.PlayDisplayMessage("Team 2 Serves!", Color.green);
                break;
        }
    }

    public void SetGameType(int gameTypeID)
    {
        currentGameType = (GameTypeID)gameTypeID;
        ResetLethalSpeed();
    }

    public void SetPlayersAmount(int playersAmountID)
    {
        foreach (PlayerController player in players)
        {
            player.gameObject.SetActive(false);
        }

        switch (playersAmountID)
        {
            case 0:
                players[0].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);

                GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                break;
            case 1:
                foreach (PlayerController player in players)
                {
                    player.gameObject.SetActive(true);
                }

                GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player2).SetState(PlayState.SameTeamAsServe);
                GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                GetPlayer(PlayerID.Player4).SetState(PlayState.OpposingServe);
                break;
            case 2:
                players[0].gameObject.SetActive(true);
                players[1].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);

                GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player2).SetState(PlayState.SameTeamAsServe);
                GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                break;
            case 3:
                players[0].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);
                players[3].gameObject.SetActive(true);

                GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                GetPlayer(PlayerID.Player4).SetState(PlayState.OpposingServe);
                break;
        }
    }

    public void SetCourt(int courtID)
    {
        courtRenderer.material = courts[courtID].tennisCourtMaterial;
        ballSpeedModifier = courts[courtID].ballSpeedModifier;
        playerSpeedModifier = courts[courtID].playerSpeedModifier;
    }

    public void SetTotalBallSpeedModifier()
    {
        totalBallSpeedModifier = ballSpeedModifier * lethalModeSpeedModifier; 
    }

    public void IncrementLethalSpeed()
    {
        if (currentGameType != GameTypeID.Lethal)
        {
            return;
        }
        lethalModeSpeedModifier += 0.2f;
        SetTotalBallSpeedModifier();
    }

    public void ResetLethalSpeed()
    {
        lethalModeSpeedModifier = 1;
        SetTotalBallSpeedModifier();
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

public enum GameTypeID
{
    Normal = 0,
    Lethal = 1,
}   

[Serializable]
public class CourtStage
{
    public string courtName;
    public Material tennisCourtMaterial;
    public float ballSpeedModifier = 1;
    public float playerSpeedModifier = 1;
}
