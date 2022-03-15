using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingPosition : MonoBehaviour
{
    public GameObject leftSideWallColliders;
    public GameObject rightSideWallColliders;
    public Transform leftSideStart;
    public Transform rightSideStart;
    public Transform leftSideBallAimerLookat;
    public Transform rightSideBallAimerLookat;

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

    public Transform GetAimerLookatPosition(bool reverseIt)
    {
        Transform newBallAimerLookat = transform;
        if (reverseIt)
        {
            newBallAimerLookat = TennisMatchManager.Instance.leftSideServe ? rightSideBallAimerLookat : leftSideBallAimerLookat;
        }
        else
        {
            newBallAimerLookat = TennisMatchManager.Instance.leftSideServe ? leftSideBallAimerLookat : rightSideBallAimerLookat;
        }
        return newBallAimerLookat;
    }
}
