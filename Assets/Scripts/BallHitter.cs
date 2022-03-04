using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHitter : MonoBehaviour
{
    public HitBy belongsToPlayer;
    public float hitPower = 1.0f;
    public PlayerController playerController;
    public Renderer renderer;
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
        gameObject.SetActive(true);
        canHitTimer = 0.8f;
        currentHitType = hitType;
        playerController.ReadyToHitBall();

        switch (hitType)
        {
            case HitType.Normal:
                renderer.material.color = Color.white;
                break;
            case HitType.Hard:
                renderer.material.color = Color.red;
                break;
            case HitType.Weak:
                renderer.material.color = Color.blue;
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            other.GetComponent<TennisBall>().HitBall(belongsToPlayer, currentHitType, hitPower + playerController.chargePower);
            playerController.HitBall();
        }
    }
}
