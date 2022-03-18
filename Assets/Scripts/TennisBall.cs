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
    public SpriteRenderer ballShadow;
    public GameObject hitParticlePrefab;
    public float speed;
    public Transform displayBallParent;
    public Transform displayBallLookatTarget;

    private Transform holdFollow;
    private PlayerController servingPlayer;
    private float lastHitXDirection;
    private bool curveBall;
    private float xCurveMultiplier;
    private float xCurve;
    private bool canBeHit = true;

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

    public void HitBall(float lastMoveXAxis, float lastMoveYAxis, Transform hitAimerDirection, HitBy hitBy, HitType hitType = HitType.Serving, float hitPower = 1)
    {
        if (ballState == BallState.Ground)
        {
            return;
        }

        if (lastHitBy == hitBy)
        {
            return;
        }

        if (!canBeHit)
        {
            return;
        }

        MainUI.Instance.servingBar.gameObject.SetActive(false);
        float moveXAxis = Mathf.Clamp(lastMoveXAxis, -1.0f, 1.0f);
        float moveYAxis = Mathf.Clamp(lastMoveYAxis, -1.0f, 1.0f);

        if (hitType == HitType.Serving)
        {
            lastHitXDirection = moveXAxis * 0.2f;
        }
        else
        {
            lastHitXDirection = moveXAxis * 0.4f;
        }

        Utilities.Instance.PlayHitStop();
        transform.position = new Vector3(transform.position.x, 0.8f, transform.position.z);
        holdFollow = null;
        ballState = BallState.Moving;
        TennisMatchManager.Instance.IncrementLethalSpeed();
        TennisMatchManager.Instance.servingPositionTeam1.SetActive(false);
        TennisMatchManager.Instance.servingPositionTeam2.SetActive(false);
        MainUI.Instance.servingSign.gameObject.SetActive(false);
        TennisMatchManager.Instance.ballBouncedOnCourt = false;
        trail.emitting = true;
        curveBall = false;
        xCurve = 0;
        xCurveMultiplier = 0;
        lastHitBy = hitBy;
        if (moveYAxis > 0.5f || moveYAxis < -0.5f)
        {
            displayBallAnimator.Play("Topspin", 0, 0.2f);
            if (hitType != HitType.Hard)
            {
                hitType = HitType.Topspin;
            }
        }
        else
        {
            displayBallAnimator.Play("Normal Bounce", 0, 0.2f);
        }
        transform.rotation = hitAimerDirection.rotation;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
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
            case HitType.Topspin:
                ballTrailColor = Color.blue;
                //hitPower = 0.75f;
                //curveBall = true;
                break;
        }

        trail.startColor = ballTrailColor;
        trail.endColor = ballTrailColor;
        speed = (6 * hitPower) * TennisMatchManager.Instance.totalBallSpeedModifier;
        moveYAxis = moveYAxis * -1;
        float yTiltSpeedModifier = 1 + (moveYAxis / 4);
        displayBallAnimator.speed = (1.25f * hitPower) * (TennisMatchManager.Instance.totalBallSpeedModifier * yTiltSpeedModifier);
        MainUI.Instance.UpdateTexts();
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
        servingPlayer = TennisMatchManager.Instance.GetPlayer(serving);
        holdFollow = servingPlayer.ballServePosition;
        CanBeHit();
    }

    public void ServeBall()
    {
        displayBallAnimator.Play("Normal Bounce", 0, 0.1f);
    }

    public void DeadBall()
    {
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        ballState = BallState.Ground;
        displayBallAnimator.Play("Ground", 0, 0);
    }

    public void CannotBeHit()
    {
        canBeHit = false;
        ballShadow.color = new Color32(0, 0, 0, 60);
        ballShadow.transform.localScale = new Vector3(2, 2, 2);
    }

    public void CanBeHit()
    {
        canBeHit = true;
        ballShadow.color = new Color32(0, 0, 0, 200);
        ballShadow.transform.localScale = new Vector3(4, 4, 4);
    }

    public void BallBounce()
    {
        Instantiate(hitParticlePrefab, displayBall.transform.position, transform.rotation);
        if (servingPlayer == null)
        {
            return;
        }
        if (servingPlayer.playState == PlayState.Serving)
        {
            TennisMatchManager.Instance.Score(TeamID.team1, ScoreType.DroppedDuringServe);
            MainUI.Instance.servingSign.gameObject.SetActive(false);
            MainUI.Instance.servingBar.gameObject.SetActive(false);
        }
    }

    public void ReturnToNormalBounce()
    {
        displayBallAnimator.Play("Normal Bounce", 0, 0f);
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
    Topspin,
}

public enum BallState
{
    Ground,
    Held,
    Moving,
}
