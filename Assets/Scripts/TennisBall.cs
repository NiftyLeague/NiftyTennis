using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
    public Animator displayBallAnimator;
    public Rigidbody rigidbody;
    public Transform displayBall;
    public HitBy lastHitBy = HitBy.Opponent;
    public BallState ballState = BallState.Held;
    public TrailRenderer trail;
    public GameObject hitParticlePrefab;
    public float speed;
    private Transform holdFollow;
    private float lastHitXDirection;
    private Vector3 directionToMove;
    private bool curveBall;
    private float xCurveMultiplier;
    private float xCurve;

    private void FixedUpdate()
    {
        switch (ballState)
        {
            case BallState.Ground:
                rigidbody.velocity = Vector3.zero;
                break;
            case BallState.Held:
                transform.position = holdFollow.position;
                break;
            case BallState.Moving:
                if (curveBall)
                {
                    xCurveMultiplier += Time.deltaTime;

                    if (lastHitXDirection > 0)
                    {
                        xCurve += Time.deltaTime * xCurveMultiplier;
                    }
                    else if (lastHitXDirection < 0)
                    {
                        xCurve -= Time.deltaTime * xCurveMultiplier;
                    }
                }
                rigidbody.velocity = (directionToMove + new Vector3(lastHitXDirection + xCurve, 0, 0)) * speed;
                break;
        }
    }

    public void HitBall(HitBy hitBy, HitType hitType = HitType.Serving, float hitPower = 1)
    {
        if (ballState == BallState.Ground)
        {
            return;
        }   

        Utilities.Instance.PlayHitStop();
        transform.position = new Vector3(transform.position.x, 0.8f, transform.position.z);
        holdFollow = null;
        TennisMatchManager.Instance.IncrementLethalSpeed();
        ballState = BallState.Moving;
        TennisMatchManager.Instance.ballBouncedOnCourt = false;
        trail.emitting = true;
        curveBall = false;
        xCurve = 0;
        xCurveMultiplier = 0;
        lastHitBy = hitBy;
        lastHitXDirection = 0;
        displayBallAnimator.Play("Normal Hit", 0, 0.2f);
        displayBallAnimator.speed = TennisMatchManager.Instance.totalBallSpeedModifier;
        float servingBonusDirection = 0.4f;

        switch (hitBy)
        {
            case HitBy.Player:
                if (hitType == HitType.Serving)
                {
                    lastHitXDirection = (Input.GetAxis("Horizontal") + servingBonusDirection) / 2;
                    lastHitXDirection = TennisMatchManager.Instance.leftSideServe ? lastHitXDirection : -lastHitXDirection;
                    Debug.Log(TennisMatchManager.Instance.leftSideServe + " | SERVING SHOT DIRECTION: " + lastHitXDirection);
                }
                else
                {
                    lastHitXDirection = Input.GetAxis("Horizontal") / 2;
                }
                directionToMove = transform.forward;
                break;
            case HitBy.Opponent:
                if (hitType == HitType.Serving)
                {
                    lastHitXDirection = (Input.GetAxis("Horizontal") + servingBonusDirection) / 2;
                    lastHitXDirection = TennisMatchManager.Instance.leftSideServe ? lastHitXDirection : -lastHitXDirection;
                    Debug.Log(TennisMatchManager.Instance.leftSideServe + " | SERVING SHOT DIRECTION: " + lastHitXDirection);
                }
                else
                {
                    lastHitXDirection = -Input.GetAxis("Horizontal") / 2;
                }
                directionToMove = -transform.forward;
                break;
        }

        Color ballTrailColor = Color.yellow;

        switch (hitType)
        {
            case HitType.Serving:
                hitPower = 1.5f;
                ballTrailColor = Color.yellow;
                break;
            case HitType.Normal:
                ballTrailColor = Color.yellow;
                break;
            case HitType.Hard:
                ballTrailColor = Color.red;
                break;
            case HitType.Weak:
                ballTrailColor = Color.blue;
                hitPower = 0.75f;
                curveBall = true;
                break;
        }

        trail.startColor = ballTrailColor;
        trail.endColor = ballTrailColor;
        speed = (5 * hitPower) * TennisMatchManager.Instance.totalBallSpeedModifier;

        Instantiate(hitParticlePrefab, transform.position, transform.rotation);
    }

    public void ResetBall(PlayerID serving)
    {
        displayBallAnimator.speed = TennisMatchManager.Instance.totalBallSpeedModifier * 2;
        displayBallAnimator.Play("Normal Bounce", 0, 0);
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        lastHitXDirection = 0;
        ballState = BallState.Held;
        holdFollow = TennisMatchManager.Instance.GetPlayer(serving).ballServePosition;
    }

    public void DeadBall()
    {
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        ballState = BallState.Ground;
        displayBallAnimator.Play("Ground", 0, 0);
    }

    public void BallBounce()
    {
        Instantiate(hitParticlePrefab, displayBall.transform.position, transform.rotation);
    }
}

public enum HitBy
{
    Player,
    Opponent,
}

public enum HitType
{
    Serving,
    Normal,
    Hard,
    Weak,
}

public enum BallState
{
    Ground,
    Held,
    Moving,
}
