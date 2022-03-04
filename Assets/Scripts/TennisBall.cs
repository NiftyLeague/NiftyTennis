using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TennisBall : MonoBehaviour
{
    public Animator displayBallAnimator;
    public Rigidbody rigidbody;
    public Transform displayBall;
    public HitBy lastHitBy = HitBy.Opponent;
    public TrailRenderer trail;
    public GameObject hitParticlePrefab;
    public float speed;
    private bool beingHeld;
    private Transform holdFollow;
    private float lastHitXDirection;
    private Vector3 directionToMove;
    private bool curveBall;
    private float xCurve;
    public float XCurve {
        get 
        { 
            return xCurve; 
        }
        set
        {
            xCurve = Mathf.Clamp(value, -1.0f, 1.0f);
        }
    }

    private void Start()
    {
        ResetBall(PlayerID.Player1);
    }

    private void FixedUpdate()
    {
        if (beingHeld)
        {
            transform.position = holdFollow.position;
        }
        else
        {

            if (curveBall)
            {
                if (lastHitXDirection > 0)
                {
                    xCurve += Time.deltaTime / 2;
                }
                else if (lastHitXDirection < 0)
                {
                    xCurve -= Time.deltaTime / 2;
                }
            }
            rigidbody.velocity = (directionToMove + new Vector3(lastHitXDirection + xCurve, 0, 0)) * speed;
        }
    }

    public void HitBall(HitBy hitBy, HitType hitType = HitType.None, float hitPower = 1)
    {
        transform.position = new Vector3(transform.position.x, 0.8f, transform.position.z);
        holdFollow = null;
        beingHeld = false;
        TennisMatchManager.Instance.ballBouncedOnCourt = false;
        trail.emitting = true;
        curveBall = false;
        xCurve = 0;
        lastHitBy = hitBy;
        lastHitXDirection = 0;
        displayBallAnimator.Play("Normal Hit", 0, 0.2f);
        displayBallAnimator.speed = 1.0f;

        switch (hitBy)
        {
            case HitBy.Player:
                if (hitType != HitType.None)
                {
                    lastHitXDirection = Input.GetAxis("Horizontal") / 2;
                }
                directionToMove = transform.forward;
                break;
            case HitBy.Opponent:
                directionToMove = -transform.forward;
                break;
        }

        Color ballTrailColor = Color.yellow;

        switch (hitType)
        {
            case HitType.None:
                trail.emitting = false;
                break;
            case HitType.Normal:
                ballTrailColor = Color.yellow;
                break;
            case HitType.Hard:
                ballTrailColor = Color.red;
                //displayBallAnimator.speed = 0.5f;
                break;
            case HitType.Weak:
                ballTrailColor = Color.blue;
                hitPower = 0.75f;
                //displayBallAnimator.speed = 2.0f;
                curveBall = true;
                break;
        }

        trail.startColor = ballTrailColor;
        trail.endColor = ballTrailColor;
        speed = 5 * hitPower;
        

        Instantiate(hitParticlePrefab, transform.position, transform.rotation);
    }

    public void ResetBall(PlayerID serving)
    {
        displayBallAnimator.speed = 2.0f;
        rigidbody.velocity = Vector3.zero;
        trail.emitting = false;
        lastHitXDirection = 0;
        beingHeld = true;
        holdFollow = TennisMatchManager.Instance.GetPlayer(serving).ballServePosition;
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
    None,
    Normal,
    Hard,
    Weak,
}
