using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public PlayState playState;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    public float moveSpeed;
    private float moveSpeedMultiplier = 1;
    private float dashTimer;
    private float dashCooldown;
    private float dashKeypressTimer;
    private LastDashKeyPressed lastDashKeyPressed = LastDashKeyPressed.None;
    public float dashPower = 4;
    public float chargePower = 0;
    public ParticleSystem chargeParticle;
    public Transform ballServePosition;
    public ServingPosition playerServeStartPosition;
    [Space]
    public SpriteRenderer spriteRenderer;
    public Sprite standingSprite;
    public List<Sprite> runningSprites;
    public List<Sprite> readyToHitSprites;
    public List<Sprite> hittingBallSprites;
    private float spriteAnimationTimer;
    private int spriteRunAnimationCurrent = -1;
    private int spriteHittingBallAnimationCurrent = -1;
    private int spriteAboutToHitBallAnimationCurrent = -1;
    [HideInInspector] public bool isHittingBall;
    [HideInInspector] public bool isAboutToHitBall;
    public bool secondPlayerInTeam;

    void FixedUpdate()
    {
        //MOVE INPUT
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (playState == PlayState.Serving)
        {
            input = new Vector2(Input.GetAxis("Horizontal"), 0);
        }

        var normal = input.normalized;
        var magnitude = input.magnitude;

        magnitude = Mathf.Clamp01(magnitude);
        input = normal * magnitude;
        Vector3 movement = Vector3.zero;
        movement = new Vector3(input.x, 0.0f, input.y);
        spriteRenderer.flipX = Input.GetAxis("Horizontal") > 0 ? false : true;

        float moveSpeedWithModifier = moveSpeed * TennisMatchManager.Instance.playerSpeedModifier;

        if (dashTimer > 0)
        {
            rigidBody.velocity = movement * (moveSpeedWithModifier * moveSpeedMultiplier) * dashPower;
            dashTimer -= Time.deltaTime;
        }
        else
        {
            rigidBody.velocity = movement * (moveSpeedWithModifier * moveSpeedMultiplier);
        }

        if (isHittingBall)
        {
            spriteAnimationTimer -= Time.deltaTime * TennisMatchManager.Instance.playerSpeedModifier;

            if (spriteAnimationTimer <= 0)
            {
                spriteHittingBallAnimationCurrent++;
                if (spriteHittingBallAnimationCurrent >= hittingBallSprites.Count)
                {
                    spriteHittingBallAnimationCurrent = -1;
                    spriteAnimationTimer = 0;
                    isHittingBall = false;
                    return;
                }
                spriteAnimationTimer = 0.05f;
                spriteRenderer.sprite = hittingBallSprites[spriteHittingBallAnimationCurrent];
            }

            return;
        }

        if (isAboutToHitBall)
        {
            spriteAnimationTimer -= Time.deltaTime * TennisMatchManager.Instance.playerSpeedModifier;

            if (spriteAnimationTimer <= 0)
            {
                spriteAboutToHitBallAnimationCurrent++;
                if (spriteAboutToHitBallAnimationCurrent >= readyToHitSprites.Count)
                {
                    spriteAboutToHitBallAnimationCurrent = 0;
                }
                spriteAnimationTimer = 0.1f;
                spriteRenderer.sprite = readyToHitSprites[spriteAboutToHitBallAnimationCurrent];
            }

            return;
        }

        if (magnitude > 0 || magnitude < 0)
        {
            spriteAnimationTimer -= Time.deltaTime * TennisMatchManager.Instance.playerSpeedModifier;

            if (spriteAnimationTimer <= 0)
            {
                spriteRunAnimationCurrent++;
                if (spriteRunAnimationCurrent >= runningSprites.Count)
                {
                    spriteRunAnimationCurrent = 0;
                }
                spriteAnimationTimer = 0.1f;
                spriteRenderer.sprite = runningSprites[spriteRunAnimationCurrent];
            }
        }
        else
        {
            spriteRunAnimationCurrent = -1;
            spriteRenderer.sprite = standingSprite;
            spriteAnimationTimer = 0;
        }
    }

    private void Update()
    {
        dashCooldown -= Time.deltaTime;
        dashKeypressTimer -= Time.deltaTime;

        if (dashKeypressTimer < 0)
        {
            lastDashKeyPressed = LastDashKeyPressed.None;
        }

        // DASH INPUTS
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            DashAttempt(LastDashKeyPressed.Up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DashAttempt(LastDashKeyPressed.Down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DashAttempt(LastDashKeyPressed.Left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            DashAttempt(LastDashKeyPressed.Right);
        }

        // BALL HIT INPUTS
        if (Input.GetKeyDown(KeyCode.X))
        {
            ballHitter.TurnOnHitter(HitType.Weak);
            chargePower = -0.8f;
        }

        var chargeParticleEmission = chargeParticle.emission;

        if (Input.GetKey(KeyCode.Z))
        {
            chargePower += Time.deltaTime;
            if (chargePower > 0.25f)
            {
                ballHitter.TurnOnHitter(HitType.Hard);
                chargeParticleEmission.rateOverTime = chargePower * 500;
            }
            else
            {
                ballHitter.TurnOnHitter();
            }

            if (chargePower > 1.0f)
            {
                chargePower = 1.0f;
            }
        }
        else
        {
            chargePower = 0;
            chargeParticleEmission.rateOverTime = 0;
        }
        
    }

    public void DashAttempt(LastDashKeyPressed keyPress)
    {
        if (lastDashKeyPressed == keyPress)
        {
            Dash();
            return;
        }
        else
        {
            lastDashKeyPressed = keyPress;
            dashKeypressTimer = 0.5f;
        }
    }

    public void Dash()
    {
        if (dashCooldown > 0)
        {
            return;
        }
        lastDashKeyPressed = LastDashKeyPressed.None;
        dashCooldown = 0.75f;
        dashTimer = 0.15f;
        dashKeypressTimer = 0;
    }

    public void HitBall()
    {
        UnReady();
        isHittingBall = true;
        SetState(PlayState.Playing);
    }

    public void ReadyToHitBall()
    {
        isAboutToHitBall = true;
        moveSpeedMultiplier = 0.5f;
    }

    public void UnReady()
    {
        isAboutToHitBall = false;
        moveSpeedMultiplier = 1;
        spriteAboutToHitBallAnimationCurrent = -1;
        spriteAnimationTimer = 0;
    }

    public void SetState(PlayState playStateToSet)
    {
        switch (playStateToSet)
        {
            case PlayState.Playing:
                playerServeStartPosition.gameObject.SetActive(false);
                MainUI.Instance.servingBar.gameObject.SetActive(false);
                break;
            case PlayState.Serving:
                playerServeStartPosition.SetServingPosition();
                transform.position = playerServeStartPosition.GetStartPosition(false).position;
                MainUI.Instance.servingBar.UpdateBar(transform);
                TennisMatchManager.Instance.tennisBall.ResetBall(playerID);
                break;
            case PlayState.OpposingServe:
                if (secondPlayerInTeam)
                {
                    transform.position = playerServeStartPosition.GetStartPosition(false).position;
                }
                else
                {
                    transform.position = playerServeStartPosition.GetStartPosition(true).position;
                }
                break;
            case PlayState.SameTeamAsServe:
                transform.position = playerServeStartPosition.GetStartPosition(true).position;
                break;
        }

        playState = playStateToSet;
    }
}

public enum PlayState
{
    Playing,
    Serving,
    SameTeamAsServe,
    OpposingServe,
}

public enum LastDashKeyPressed
{
    None,
    Up,
    Down,
    Left,
    Right,
}
