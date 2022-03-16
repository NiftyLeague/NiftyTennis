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
    public Transform displayBallParent;
    public Transform displayBallLookatTarget;

    private Transform holdFollow;
    private float lastHitXDirection;
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
                rigidbody.velocity = (transform.forward + new Vector3(lastHitXDirection + xCurve, 0, 0)) * speed;
                break;
        }

        displayBallParent.LookAt(displayBallLookatTarget);
    }

    public void HitBall(float lastMoveXAxis, Transform hitAimerDirection, HitBy hitBy, HitType hitType = HitType.Serving, float hitPower = 1)
    {
        if (ballState == BallState.Ground)
        {
            return;
        }

        if (lastHitBy == hitBy)
        {
            return;
        }

        //Debug.Log(hitType);
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
        displayBallAnimator.Play("Normal Bounce", 0, 0.2f);
        transform.rotation = hitAimerDirection.rotation;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        float moveXAxis = Mathf.Clamp(lastMoveXAxis, -1.0f, 1.0f);

        if (hitType == HitType.Serving)
        {
            lastHitXDirection = moveXAxis * 0.25f;
        }
        else
        {
            lastHitXDirection = moveXAxis * 0.5f;
        }

        Color ballTrailColor = Color.yellow;

        switch (hitType)
        {
            case HitType.Serving:
                hitPower = MainUI.Instance.servingBar.serveBar.fillAmount * 2;
                if (hitPower < 0.5f)
                {
                    hitPower = 0.5f;
                }
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
        speed = (6 * hitPower) * TennisMatchManager.Instance.totalBallSpeedModifier;
        displayBallAnimator.speed = hitPower * TennisMatchManager.Instance.totalBallSpeedModifier;

        Instantiate(hitParticlePrefab, transform.position, transform.rotation);
    }

    public void ResetBall(PlayerID serving)
    {
        lastHitBy = HitBy.None;
        displayBallAnimator.speed = 2.5f * TennisMatchManager.Instance.totalBallSpeedModifier;
        displayBallAnimator.Play("Serve Hold", 0, 0);
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        lastHitXDirection = 0;
        ballState = BallState.Held;
        holdFollow = TennisMatchManager.Instance.GetPlayer(serving).ballServePosition;
    }

    public void ServeBall()
    {
        displayBallAnimator.Play("Normal Bounce", 0, 0);
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
    None,
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
