using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public PlayState playState;
    public ControlledBy controlledBy = ControlledBy.Player;
    public CourtSide courtSide = CourtSide.Both;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    public GameObject dashParticlePrefab;
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
    public List<Sprite> runningUpSprites;
    public List<Sprite> runningDownSprites;
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

    //CPU Related
    private float currentCPUSpeed;
    private float targetCPUSpeed;

    private void Update()
    {
        if (controlledBy == ControlledBy.Player)
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
                    MainUI.Instance.servingSign.gameObject.SetActive(false);
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

        if (controlledBy == ControlledBy.CPU)
        {
            Vector3 tennisBallPositionForCPU = TennisMatchManager.Instance.tennisBall.transform.position;

            if (Vector3.Distance(tennisBallPositionForCPU, transform.position) < 0.8f)
            {
                lastMoveXAxis = Random.Range(-1.0f, 1.0f);
                ballHitter.TurnOnHitter();
            }

            if (courtSide == CourtSide.Both || courtSide == CourtSide.Left && tennisBallPositionForCPU.x < 0 || courtSide == CourtSide.Right && tennisBallPositionForCPU.x > 0)
            {
                if (tennisBallPositionForCPU.x > transform.position.x + 0.2f)
                {
                    targetCPUSpeed = 1f;
                }
                else if (tennisBallPositionForCPU.x < transform.position.x - 0.2f)
                {
                    targetCPUSpeed = -1f;
                }
                else
                {
                    targetCPUSpeed = 0;
                }
            }
            else
            {
                targetCPUSpeed = 0;
            }

            currentCPUSpeed = Mathf.Lerp(currentCPUSpeed, targetCPUSpeed, 0.5f);

            moveInput = new Vector2(currentCPUSpeed, 0);
        }
    }
    void FixedUpdate()
    {
        //MOVE INPUT
        var normal = moveInput.normalized;
        var magnitude = moveInput.magnitude;

        magnitude = Mathf.Clamp01(magnitude);
        moveInput = normal * magnitude;
        Vector3 movement = Vector3.zero;
        movement = new Vector3(moveInput.x, 0.0f, moveInput.y);
        spriteRenderer.flipX = moveInput.x > 0 ? false : true;

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
                if (moveInput.y > 0)
                {
                    spriteRenderer.sprite = runningUpSprites[spriteRunAnimationCurrent];
                }
                else if (moveInput.y < 0)
                {
                    spriteRenderer.sprite = runningDownSprites[spriteRunAnimationCurrent];
                }
                else
                {
                    spriteRenderer.sprite = runningSprites[spriteRunAnimationCurrent];
                }
                
            }
        }
        else
        {
            spriteRunAnimationCurrent = -1;
            spriteRenderer.sprite = standingSprite;
            spriteAnimationTimer = 0;
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
        if (dashCooldown > 0 || playState == PlayState.Serving || playState == PlayState.ServingMove)
        {
            return;
        }
        float dashDirection = 0;
        switch (lastDashKeyPressed)
        {
            case LastDashKeyPressed.Up: dashDirection = 180; break;
            case LastDashKeyPressed.Down: dashDirection = 0; break;
            case LastDashKeyPressed.Left: dashDirection = 90; break;
            case LastDashKeyPressed.Right: dashDirection = -90; break;
        }
        var newDashParticle = Instantiate(dashParticlePrefab, transform.position, transform.rotation);
        newDashParticle.transform.eulerAngles = new Vector3(0, dashDirection);
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
                MainUI.Instance.servingSign.UpdateSign(transform);
                break;
            case PlayState.Serving:
                ballHitter.ballHitAimer.target = playerServeStartPosition.GetAimerLookatPosition(false);
                TennisMatchManager.Instance.tennisBall.ServeBall();
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

    public void SetCourtSide(bool isBoth)
    {
        if (isBoth)
        {
            courtSide = CourtSide.Both;
        }
        else
        {
            if (transform.position.x < 0)
            {
                courtSide = CourtSide.Left;
            }
            else
            {
                courtSide = CourtSide.Right;
            }
        }
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

public enum ControlledBy
{
    Player,
    CPU,
}

public enum CourtSide
{
    Both,
    Left,
    Right,
}
