using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public PlayState playState;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    private float moveSpeedMultiplier = 1;
    private float dashTimer;
    private float dashCooldown;
    private float dashKeypressTimer;
    private LastDashKeyPressed lastDashKeyPressed = LastDashKeyPressed.None;
    public float dashPower = 4;
    public float chargePower = 0;
    public ParticleSystem chargeParticle;
    public Transform ballServePosition;
    public Transform regularBallAimerLookat;
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
    [HideInInspector] public float lastMoveXAxis;
    public bool secondPlayerInTeam;
    private InputState input = new InputState();
    private Vector2 moveInput;

    void FixedUpdate()
    {
        //MOVE INPUT


        var normal = moveInput.normalized;
        var magnitude = moveInput.magnitude;

        magnitude = Mathf.Clamp01(magnitude);
        moveInput = normal * magnitude;
        Vector3 movement = Vector3.zero;
        movement = new Vector3(moveInput.x, 0.0f, moveInput.y);
        spriteRenderer.flipX = input.xAxis > 0 ? false : true;

        float moveSpeedWithModifier = 4.5f * TennisMatchManager.Instance.playerSpeedModifier;

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
        var device = InputReader.Device.Any;
        InputReader.GetInput(device, input);

        //Move Input
        moveInput = new Vector2(input.xAxis, input.yAxis);
        lastMoveXAxis = moveInput.x;

        if (playState == PlayState.ServingMove)
        {
            moveInput = new Vector2(input.xAxis, 0);
        }

        if (playState == PlayState.Serving)
        {
            moveInput = new Vector2(0, 0);
        }

        dashCooldown -= Time.deltaTime;
        dashKeypressTimer -= Time.deltaTime;

        if (dashKeypressTimer < 0)
        {
            lastDashKeyPressed = LastDashKeyPressed.None;
        }

        // DASH INPUTS
        if (input.PressedUp)
        {
            DashAttempt(LastDashKeyPressed.Up);
        }
        if (input.PressedDown)
        {
            DashAttempt(LastDashKeyPressed.Down);
        }
        if (input.PressedLeft)
        {
            DashAttempt(LastDashKeyPressed.Left);
        }
        if (input.PressedRight)
        {
            DashAttempt(LastDashKeyPressed.Right);
        }

        // BALL HIT INPUTS
        if (input.PressedB)
        {
            ballHitter.TurnOnHitter(HitType.Weak);
            chargePower = -0.8f;
        }

        var chargeParticleEmission = chargeParticle.emission;

        if (input.PressedA)
        {
            if (playState == PlayState.ServingMove)
            {
                SetState(PlayState.Serving);
            }
        }

        if (input.ReleasedA)
        {
            if (playState == PlayState.Serving)
            {
                if (chargePower > 0.25f)
                {
                    ballHitter.TurnOnHitter(HitType.Hard);
                }
                else
                {
                    ballHitter.TurnOnHitter();
                }

                playerServeStartPosition.gameObject.SetActive(false);
                MainUI.Instance.servingBar.gameObject.SetActive(false);
            }
        }

        if (input.aButton)
        {
            chargePower += Time.deltaTime;
            if (chargePower > 0.25f)
            {
                if (playState == PlayState.Playing)
                {
                    ballHitter.TurnOnHitter(HitType.Hard);
                }
                chargeParticleEmission.rateOverTime = chargePower * 500;
            }
            else
            {
                if (playState == PlayState.Playing)
                {
                    ballHitter.TurnOnHitter();
                }
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
        if (dashCooldown > 0 || playState == PlayState.Serving)
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
        ballHitter.ballHitAimer.target = regularBallAimerLookat;

        switch (playStateToSet)
        {
            case PlayState.Playing:

                break;
            case PlayState.ServingMove:
                playerServeStartPosition.SetServingPosition();
                transform.position = playerServeStartPosition.GetStartPosition(false).position;
                ballHitter.ballHitAimer.target = playerServeStartPosition.GetAimerLookatPosition(false);
                TennisMatchManager.Instance.tennisBall.ResetBall(playerID);
                break;
            case PlayState.Serving:
                ballHitter.ballHitAimer.target = playerServeStartPosition.GetAimerLookatPosition(false);
                MainUI.Instance.servingBar.UpdateBar(transform);
                break;
            case PlayState.OpposingServe:
                transform.position = secondPlayerInTeam ? playerServeStartPosition.GetStartPosition(false).position : playerServeStartPosition.GetStartPosition(true).position;
                playStateToSet = PlayState.Playing;
                break;
            case PlayState.SameTeamAsServe:
                transform.position = playerServeStartPosition.GetStartPosition(true).position;
                playStateToSet = PlayState.Playing;
                break;
        }

        playState = playStateToSet;
    }
}

public enum PlayState
{
    Playing,
    ServingMove,
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
