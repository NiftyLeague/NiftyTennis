using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceChecker : MonoBehaviour
{
    public bool outsideOfCourt;
    public TeamID teamSide;
    public TeamID opposingTeam;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (!TennisMatchManager.Instance.tennisBall.canBeHit)
            {
                return;
            }

            if (outsideOfCourt)
            {
                TennisMatchManager.Instance.Score(TennisMatchManager.Instance.tennisBall.lastHitBy, ScoreType.HitScoreArea);
            }
            else
            {
                if (TennisMatchManager.Instance.ballBouncedOnCourt)
                {
                    TennisMatchManager.Instance.Score(TennisMatchManager.Instance.tennisBall.lastHitBy, ScoreType.HitScoreArea);
                }
                else
                {
                    if (TennisMatchManager.Instance.tennisBall.lastHitBy == teamSide)
                    {
                        TennisMatchManager.Instance.Score(opposingTeam, ScoreType.HitScoreArea);
                    }
                    else
                    {
                        TennisMatchManager.Instance.ballBouncedOnCourt = true;
                    }
                }
            }
        }
    }
}
