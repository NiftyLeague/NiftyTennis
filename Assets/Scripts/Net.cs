using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            TennisMatchManager.Instance.Score(TeamID.team1, ScoreType.HitNet);
        }
    }
}
