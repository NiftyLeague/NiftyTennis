using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
    public Animator displayBallAnimator;
    public Rigidbody rigidbody;
    public Transform displayBall;
    public TeamID lastHitBy = TeamID.None;
    public HitType lastHitType = HitType.Normal;
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
    [HideInInspector] public bool canBeHit = true;

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

    public void HitBall(float lastMoveXAxis, float lastMoveYAxis, Transform hitAimerDirection, TeamID hitBy, HitType hitType = HitType.Serving, float hitPower = 1)
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
            lastHitXDirection = moveXAxis * 0.1f;
        }
        else
        {
            if (!TennisMatchManager.Instance.ballBouncedOnCourt && lastHitType == HitType.Serving)
            {
                TennisMatchManager.Instance.Score(lastHitBy, ScoreType.ServeNeedsToBounceFirst);
                return;
            }
            else
            {
                lastHitXDirection = moveXAxis * 0.3f;
            }
        }

        if (hitType == HitType.Charge)
        {
            Utilities.Instance.PlayHitStop(hitPower);
        }

        lastHitType = hitType;
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
                displayBallAnimator.Play("Normal Bounce", 0, 0.2f);
                break;
            case HitType.Normal:
                ballTrailColor = Color.yellow;
                displayBallAnimator.Play("Normal Bounce", 0, 0.2f);
                break;
            case HitType.Charge:
                ballTrailColor = Color.red;
                displayBallAnimator.Play("Normal Bounce", 0, 0.2f);
                break;
            case HitType.Lob:
                ballTrailColor = Color.blue;
                displayBallAnimator.Play("Lob", 0, 0.2f);
                break;
            case HitType.Drop:
                ballTrailColor = Color.magenta;
                displayBallAnimator.Play("Drop", 0, 0.2f);
                hitPower = 1.5f;
                break;
        }

        TennisMatchManager.Instance.IncrementSpecial(lastHitBy, 1);

        trail.startColor = ballTrailColor;
        trail.endColor = ballTrailColor;
        speed = (6 * hitPower) * TennisMatchManager.Instance.totalBallSpeedModifier;
        moveYAxis = moveYAxis * -1;
        float yTiltSpeedModifier = 1 + (moveYAxis / 8);
        displayBallAnimator.speed = (1.25f * hitPower) * (TennisMatchManager.Instance.totalBallSpeedModifier * yTiltSpeedModifier);
        MainUI.Instance.UpdateTexts();
        Instantiate(hitParticlePrefab, transform.position, transform.rotation);
    }

    public void ResetBall(PlayerID serving)
    {
        lastHitBy = TeamID.None;
        displayBallAnimator.speed = 2.5f * TennisMatchManager.Instance.totalBallSpeedModifier;
        displayBallAnimator.Play("Serve Hold", 0, 0);
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        lastHitXDirection = 0;
        ballState = BallState.Held;
        servingPlayer = TennisMatchManager.Instance.GetPlayer(serving);
        holdFollow = servingPlayer.ballServePosition;
        TennisMatchManager.Instance.ballBouncedOnCourt = false;
        CanBeHit();
    }

    public void ServeBall()
    {
        displayBallAnimator.Play("Normal Bounce", 0, 0.1f);
    }

    public void DeadBall()
    {
        CannotBeHit();
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
        CanBeHit();
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

public enum HitType
{
    Serving,
    Normal,
    Charge,
    Lob,
    Drop,
}

public enum BallState
{
    Ground,
    Held,
    Moving,
}
