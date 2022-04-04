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
    public int team1Special;
    public int team2Special;
    [Space]
    public List<PlayerController> players;
    [HideInInspector] public GameTypeID currentGameType = GameTypeID.Normal;
    [Space]
    public TennisBall tennisBall;
    public bool ballBouncedOnCourt = false;
    [HideInInspector] public bool leftSideServe = true;
    public TeamID servingTeam = TeamID.team1;
    public GameObject servingPositionTeam1;
    public GameObject servingPositionTeam2;
    public List<BounceChecker> courtSideBoundaries;
    [Space]
    public List<CourtStage> courts;
    public Renderer courtRenderer;
    public int currentPlayersAmountID;
    public float ballSpeedModifier = 1;
    public float playerSpeedModifier = 1;
    [Space]
    public float lethalModeSpeedModifier = 1;
    [HideInInspector] public float totalBallSpeedModifier = 1;

    public void Score(TeamID teamID, ScoreType scoreType)
    {
        switch (scoreType)
        {
            case ScoreType.HitScoreArea:
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
                            MainUI.Instance.PlayDisplayMessage("OUT!", Color.white);
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
                            MainUI.Instance.PlayDisplayMessage("OUT!", Color.white);
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
                break;
            case ScoreType.HitNet:
                MainUI.Instance.PlayDisplayMessage("Hit The Net...", Color.white);
                break;
            case ScoreType.DroppedDuringServe:
                MainUI.Instance.PlayDisplayMessage("Dropped The Ball...", Color.white);
                break;
            case ScoreType.ServeNeedsToBounceFirst:
                MainUI.Instance.PlayDisplayMessage("Serve Needs To Bounce First!", Color.white);
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
        MainUI.Instance.UpdateTexts();
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
        Debug.Log("Change Team");
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
                tennisBall.ResetBall(PlayerID.Player1);
                MainUI.Instance.PlayDisplayMessage("Team 1 Serves!", Color.green);
                break;
            case TeamID.team2:
                tennisBall.ResetBall(PlayerID.Player3);
                MainUI.Instance.PlayDisplayMessage("Team 2 Serves!", Color.green);
                break;
        }

        SetUpPlayers(servingTeam);
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

        currentPlayersAmountID = playersAmountID;
        SetUpPlayers(TeamID.team1);
    }

    void SetUpPlayers(TeamID servingTeam)
    {
        List<Transform> playersToAddToCamera = new List<Transform>();
        foreach (BounceChecker boundary in courtSideBoundaries)
        {
            boundary.outsideOfCourt = false;
        }

        switch (currentPlayersAmountID)
        {
            case 0:
                players[0].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);

                playersToAddToCamera.Add(players[0].transform);
                playersToAddToCamera.Add(players[2].transform);
                MainUI.Instance.mainCamera.AddPlayersToCamera(playersToAddToCamera, new Vector3(0, -1, 0));

                if (servingTeam == TeamID.team1)
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                }
                else
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.ServingMove);
                }

                players[0].SetCourtSide(false);
                players[2].SetCourtSide(false);

                foreach (BounceChecker boundary in courtSideBoundaries)
                {
                    boundary.outsideOfCourt = true;
                }
                break;
            case 1:
                if (servingTeam == TeamID.team1)
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player2).SetState(PlayState.SameTeamAsServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player4).SetState(PlayState.OpposingServe);
                }
                else
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player2).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player4).SetState(PlayState.SameTeamAsServe);
                }

                foreach (PlayerController player in players)
                {
                    player.gameObject.SetActive(true);
                    player.SetCourtSide(false);
                    playersToAddToCamera.Add(player.transform);
                }
                MainUI.Instance.mainCamera.AddPlayersToCamera(playersToAddToCamera, new Vector3(0,-1,0));
                break;
            case 2:
                players[0].gameObject.SetActive(true);
                players[1].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);

                playersToAddToCamera.Add(players[0].transform);
                playersToAddToCamera.Add(players[1].transform);
                playersToAddToCamera.Add(players[2].transform);
                MainUI.Instance.mainCamera.AddPlayersToCamera(playersToAddToCamera, new Vector3(0, 0, 0));

                if (servingTeam == TeamID.team1)
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player2).SetState(PlayState.SameTeamAsServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                }
                else
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player2).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.ServingMove);
                }

                players[0].SetCourtSide(false);
                players[1].SetCourtSide(false);
                players[2].SetCourtSide(true);
                break;
            case 3:
                players[0].gameObject.SetActive(true);
                players[2].gameObject.SetActive(true);
                players[3].gameObject.SetActive(true);

                playersToAddToCamera.Add(players[0].transform);
                playersToAddToCamera.Add(players[2].transform);
                playersToAddToCamera.Add(players[3].transform);
                MainUI.Instance.mainCamera.AddPlayersToCamera(playersToAddToCamera, new Vector3(0, -2, 0));
                if (servingTeam == TeamID.team1)
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player4).SetState(PlayState.OpposingServe);
                }
                else
                {
                    GetPlayer(PlayerID.Player1).SetState(PlayState.OpposingServe);
                    GetPlayer(PlayerID.Player3).SetState(PlayState.ServingMove);
                    GetPlayer(PlayerID.Player4).SetState(PlayState.SameTeamAsServe);
                }

                players[0].SetCourtSide(true);
                players[2].SetCourtSide(false);
                players[3].SetCourtSide(false);
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

    public void IncrementSpecial(TeamID teamID, int amount)
    {
        if (currentGameType == GameTypeID.Classic)
        {
            return;
        }

        switch (teamID)
        {
            case TeamID.team1:
                team1Special += amount;
                team1Special = Mathf.Clamp(team1Special, 0, 10);
                break;
            case TeamID.team2:
                team2Special += amount;
                team2Special = Mathf.Clamp(team2Special, 0, 10);
                break;
        }

        MainUI.Instance.UpdateTexts();
    }

    public void ResetSpecials()
    {
        team1Special = 0;
        team2Special = 0;
        MainUI.Instance.UpdateTexts();
    }
}

public enum TeamID
{
    None,
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
    Classic = 1,
    Lethal = 2,
}

public enum ScoreType
{
    HitScoreArea,
    HitNet,
    DroppedDuringServe,
    ServeNeedsToBounceFirst,
}

[Serializable]
public class CourtStage
{
    public string courtName;
    public Material tennisCourtMaterial;
    public float ballSpeedModifier = 1;
    public float playerSpeedModifier = 1;
}
