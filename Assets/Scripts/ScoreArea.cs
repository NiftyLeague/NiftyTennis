using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreArea : MonoBehaviour
{
    public TeamID teamThisScoresTo;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            if (!TennisMatchManager.Instance.tennisBall.canBeHit)
            {
                return;
            }
            TennisMatchManager.Instance.Score(teamThisScoresTo, ScoreType.HitScoreArea);
        }
    }
}
