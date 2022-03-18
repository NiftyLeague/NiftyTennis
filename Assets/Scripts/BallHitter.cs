using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitter : MonoBehaviour
{
    public HitBy belongsToPlayer;
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
            if (canHitTimer <= 0)
            {
                playerController.UnReady();
                gameObject.SetActive(false);
            }
        }
    }

    public void TurnOnHitter(HitType hitType = HitType.Normal)
    {
        if (playerController.playState == PlayState.Serving)
        {
            hitType = HitType.Serving;
        }

        gameObject.SetActive(true);
        canHitTimer = 0.8f;
        currentHitType = hitType;
        playerController.ReadyToHitBall();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            other.GetComponent<TennisBall>().HitBall(playerController.lastMoveXAxis, playerController.lastMoveYAxis, ballHitAimer.transform, belongsToPlayer, currentHitType, hitPower + playerController.chargePower);
            playerController.HitBall();
        }
    }
}
