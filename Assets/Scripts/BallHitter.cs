using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitter : MonoBehaviour
{
    public float hitPower = 1.0f;
    public PlayerController playerController;
    public BallHitAimer ballHitAimer;
    private float canHitTimer;
    private HitType currentHitType = HitType.Normal;

    private void Update()
    {
        if (canHitTimer > 0)
        {
            canHitTimer -= Time.deltaTime;
            if (canHitTimer < 0)
            {
                playerController.UnReady();
                TurnOffHitter();
            }
        }
    }

    public void TurnOnHitter(HitType hitType = HitType.Normal, bool isPerformingAnotherAction = false)
    {
        if (playerController.playState == PlayState.Serving || playerController.playState == PlayState.ServingMove)
        {
            hitType = HitType.Serving;
        }

        gameObject.SetActive(true);
        currentHitType = hitType;

        if (isPerformingAnotherAction)
        {
            return;
        }

        canHitTimer = 0.8f;
        playerController.ReadyToHitBall(hitType);
    }

    void TurnOffHitter()
    {
        canHitTimer = 0;
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            other.GetComponent<TennisBall>().HitBall(playerController.lastMoveXAxis, playerController.lastMoveYAxis, ballHitAimer.transform, playerController.teamID, currentHitType, hitPower + (playerController.chargePower * 2));
            playerController.HitBall();
            TurnOffHitter();
        }
    }
}
