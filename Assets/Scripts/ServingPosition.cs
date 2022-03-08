using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingPosition : MonoBehaviour
{
    public GameObject leftSideWallColliders;
    public GameObject rightSideWallColliders;
    public Transform leftSideStart;
    public Transform rightSideStart;

    public void SetServingPosition()
    {
        gameObject.SetActive(true);
        TennisMatchManager tennisMatchManager = TennisMatchManager.Instance;
        tennisMatchManager.leftSideServe = !tennisMatchManager.leftSideServe;
        leftSideWallColliders.SetActive(tennisMatchManager.leftSideServe);
        rightSideWallColliders.SetActive(!tennisMatchManager.leftSideServe);
    }

    public Transform GetStartPosition(bool reverseIt)
    {
        Transform newStartPosition = transform;
        if (reverseIt)
        {
            newStartPosition = TennisMatchManager.Instance.leftSideServe ? rightSideStart : leftSideStart;
        }
        else
        {
            newStartPosition = TennisMatchManager.Instance.leftSideServe ? leftSideStart : rightSideStart;
        }
        return newStartPosition;
    }
}
