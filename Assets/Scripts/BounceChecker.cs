using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceChecker : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            TennisMatchManager.Instance.ballBouncedOnCourt = true;
        }
    }
}
