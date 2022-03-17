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
            TennisMatchManager.Instance.Score(teamThisScoresTo, ScoreType.HitScoreArea);
        }
    }
}
