using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public TeamID teamID;
    public PlayState playState;
    public ControlledBy controlledBy = ControlledBy.Player;
    public CourtSide courtSide = CourtSide.Both;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    public GameObject diveParticlePrefab;
    public GameObject divingParticle;
    public Animator spriteAnimator;
    private float moveSpeedMultiplier = 1;
    private float diveTimer;
    [HideInInspector] public float actionCooldown;
    [HideInInspector] public float actionCooldownToSet;
    private float divePower = 0;
    [HideInInspector] public float chargePower = 0;
    public ParticleSystem chargeParticle;
    public Transform ballServePosition;
    public Transform regularBallAimerLookat;
    //public GameObject swingCooldownBar;
    public ServingPosition playerServeStartPosition;
    public SpecialStars specialStars;
    [Space]
    public SpriteRenderer spriteRenderer;
    public Sprite standingSprite;
    public List<Sprite> runningSprites;
    public List<Sprite> runningUpSprites;
    public List<Sprite> runningDownSprites;
    public List<Sprite> readyToHitSprites;
    public List<Sprite> hittingBallSprites;
    public List<Sprite> divingSprites;
    public List<Sprite> jumpingSprites;
    private float spriteAnimationTimer;
    private int spriteRunAnimationCurrent = -1;
    private int spriteHittingBallAnimationCurrent = -1;
    private int spriteAboutToHitBallAnimationCurrent = -1;
    private int spriteDivingAnimationCurrent = -1;
    [HideInInspector] public bool isHittingBall;
    [HideInInspector] public bool isAboutToHitBall;
    [HideInInspector] public float lastMoveXAxis;
    [HideInInspector] public float lastMoveYAxis;
    public bool secondPlayerInTeam;
    private InputState input = new InputState();
    private Vector2 moveInput;
    private Vector3 lastMoveInput;
    private bool isJumping;

    //Special Stars Costs
    private int SpecialShotCost = 10;
    private int chargingSpecialCost = 2;
    private int divingSpecialCost = 2;
    private int jumpingSpecialCost = 1;

    //CPU Related
    private float currentCPUSpeed;
    private float targetCPUSpeed;
    private float cpuServeTimer;

    private void Start()
    {
        actionCooldownToSet = 0.4f;
    }

    private void Update()
    {
        if (actionCooldown > 0)
        {
            actionCooldown -= Time.deltaTime;
            if (actionCooldown <= 0)
            {
                CanMoveAgain();
            }
        }

        if (controlledBy == ControlledBy.Player)
        {
            var device = InputReader.Device.Any;
            InputReader.GetInput(device, input);

            //Move Input
            moveInput = new Vector2(input.xAxis * 1000, input.yAxis * 1000);
            lastMoveXAxis = moveInput.x;
            lastMoveYAxis = moveInput.y;

            if (playState == PlayState.ServingMove)
            {
                moveInput = new Vector2(input.xAxis, 0);
            }

            if (playState == PlayState.Serving)
            {
                moveInput = new Vector2(0, 0);
            }
            
            if (input.PressedB)
            {
                if (isJumping || diveTimer > 0)
                {
                    return;
                }
                if (moveInput == Vector2.zero)
                {
                    Jump();
                }
                else
                {
                    Dive();
                }
            }

            var chargeParticleEmission = chargeParticle.emission;

            if (input.PressedA)
            {
                if (playState == PlayState.ServingMove)
                {
                    SetState(PlayState.Serving);
                    return;
                }

                if (playState == PlayState.Serving)
                {
                    //if (chargePower > 0.25f && specialStars.amountOfStars >= 2)
                    //{
                    //    ballHitter.TurnOnHitter(HitType.Hard);
                    //}
                    //else
                    //{
                        
                    //}
                    ballHitter.TurnOnHitter();
                    return;
                }
            }

            //if (input.ReleasedA)
            //{
            //    if (actionCooldown > 0)
            //    {
            //        return;
            //    }


            //}

            if (input.aButton)
            {
                if (actionCooldown > 0)
                {
                    return;
                }

                if (specialStars.amountOfStars >= chargingSpecialCost || TennisMatchManager.Instance.currentGameType == GameTypeID.Classic)
                {
                    chargePower += Time.deltaTime;
                }
                if (chargePower > 0.25f)
                {
                    if (playState == PlayState.Playing)
                    {
                        ballHitter.TurnOnHitter(HitType.Charge);
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
                if (cpuServeTimer > 0 && playState == PlayState.ServingMove)
                {
                    cpuServeTimer -= Time.deltaTime;
                    return;
                }

                if (ballHitter.gameObject.activeInHierarchy)
                {
                    return;
                }

                lastMoveXAxis = Random.Range(-1.0f, 1.0f);
                if (TennisMatchManager.Instance.ballBouncedOnCourt || playState == PlayState.ServingMove || playState == PlayState.Serving)
                {
                    Debug.Log("CAN HIT");
                    ballHitter.TurnOnHitter();
                }
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

            if (playState != PlayState.ServingMove && playState != PlayState.Serving)
            {
                moveInput = new Vector2(currentCPUSpeed, 0);
            }
        }
    }
    void FixedUpdate()
    {
        //MOVE INPUT
        if (actionCooldown > 0)
        {
            moveInput = Vector3.zero;
        }
        var normal = moveInput.normalized;
        var magnitude = moveInput.magnitude;
        magnitude = Mathf.Clamp01(magnitude);
        moveInput = normal * magnitude;
        Vector3 movement = Vector3.zero;
        movement = new Vector3(moveInput.x, 0.0f, moveInput.y);
        spriteRenderer.flipX = moveInput.x > 0 ? false : true;

        float moveSpeedWithModifier = 0.08f * TennisMatchManager.Instance.playerSpeedModifier;

        if (diveTimer > 0)
        {
            Vector3 movementPosition = (lastMoveInput * (moveSpeedWithModifier * moveSpeedMultiplier)) * divePower;
            Vector3 characterPosition = transform.position;
            characterPosition += movementPosition;
            transform.position = characterPosition;

            ballHitter.TurnOnHitter(HitType.Normal, true);

            diveTimer -= Time.deltaTime;
            divePower -= Time.deltaTime * 3f;
            divePower = Mathf.Clamp(divePower, 0, 50);
            if (diveTimer <= 0)
            {
                divingParticle.SetActive(false);
                actionCooldown = actionCooldownToSet;
                //swingCooldownBar.SetActive(true);
                diveTimer = 0;
                divePower = 0;
            }
        }
        else
        {
            if (isJumping)
            {
                ballHitter.TurnOnHitter(HitType.Normal, true);
            }

            lastMoveInput = movement;
            Vector3 movementPosition = movement * (moveSpeedWithModifier * moveSpeedMultiplier);
            Vector3 characterPosition = transform.position;
            characterPosition += movementPosition;
            transform.position = characterPosition;
        }

        //SPRITE ANIMATING
        if (isJumping)
        {
            spriteRenderer.sprite = jumpingSprites[0];
            return;
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

        if (isAboutToHitBall || playState == PlayState.Serving)
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

        if (divePower > 0)
        {
            spriteAnimationTimer -= Time.deltaTime * TennisMatchManager.Instance.playerSpeedModifier;
            if (spriteAnimationTimer <= 0)
            {
                spriteDivingAnimationCurrent++;
                if (spriteDivingAnimationCurrent >= divingSprites.Count)
                {
                    spriteDivingAnimationCurrent = 0;
                }
                spriteAnimationTimer = 0.05f;
                spriteRenderer.sprite = divingSprites[spriteDivingAnimationCurrent];
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
                spriteAnimationTimer = 0.05f;
                if (input.up)
                {
                    spriteRenderer.sprite = runningUpSprites[spriteRunAnimationCurrent];
                }
                else if (input.down)
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

    public void Dive()
    {
        if (!CheckIfCanUseAction(divingSpecialCost))
        { 
            return;
        }

        Vector3 tennisBallPosition = TennisMatchManager.Instance.tennisBall.transform.position;

        if (Vector3.Distance(tennisBallPosition, transform.position) < 5f)
        {            
            float newXDirection = 0;
            if (tennisBallPosition.x > transform.position.x)
            {
                newXDirection = 1f;
            }
            else if (tennisBallPosition.x < transform.position.x)
            {
                newXDirection = -1f;
            }
            float newYDirection = 0;
            if (tennisBallPosition.z > transform.position.z)
            {
                newYDirection = 1f;
            }
            else if (tennisBallPosition.z < transform.position.z)
            {
                newYDirection = -1f;
            }
            lastMoveInput = new Vector3(newXDirection, 0, newYDirection);
        }

        var newDashParticle = Instantiate(diveParticlePrefab, transform.position, transform.rotation);
        float moveAxisForDashParticle = Mathf.Atan2(lastMoveXAxis, lastMoveYAxis) * 180 / Mathf.PI;
        float pointerOffset = -180;
        newDashParticle.transform.eulerAngles = new Vector3(0, moveAxisForDashParticle + pointerOffset, 0);
        TennisMatchManager.Instance.IncrementSpecial(teamID, -divingSpecialCost);

        diveTimer = 0.5f;
        divePower = 2f;
        divingParticle.SetActive(true);
    }

    public void Jump()
    {
        if (!CheckIfCanUseAction(jumpingSpecialCost))
        {
            return;
        }
        TennisMatchManager.Instance.IncrementSpecial(teamID, -jumpingSpecialCost);
        var newDashParticle = Instantiate(diveParticlePrefab, transform.position, transform.rotation);
        newDashParticle.transform.eulerAngles = new Vector3(90, 0, 0);
        spriteAnimator.Play("jump", 0, 0);
        isJumping = true;
        actionCooldown = 0;
        diveTimer = 0;
    }

    public void HitBall()
    {
        UnReady();
        isHittingBall = true;
        SetState(PlayState.Playing);
    }

    public void ReadyToHitBall(HitType hitType)
    {
        isAboutToHitBall = true;
        if (hitType == HitType.Charge)
        {
            moveSpeedMultiplier = 0.5f;
        }
        else
        {
            moveSpeedMultiplier = 0.0f;
        }
        
    }

    public void UnReady()
    {
        if (!isAboutToHitBall)
        {
            return;
        }

        isAboutToHitBall = false;
        spriteAboutToHitBallAnimationCurrent = -1;
        spriteAnimationTimer = 0;
        //swingCooldownBar.SetActive(true);
        actionCooldown = actionCooldownToSet;
    }

    public void CanMoveAgain()
    {
        actionCooldown = 0;
        moveSpeedMultiplier = 1.0f;
        //swingCooldownBar.SetActive(false);
    }

    public void EndJumping()
    {
        isJumping = false;
        //swingCooldownBar.SetActive(true);
        actionCooldown = actionCooldownToSet;
    }

    public void SetState(PlayState playStateToSet)
    {
        ballHitter.ballHitAimer.target = regularBallAimerLookat;

        switch (playStateToSet)
        {
            case PlayState.Playing:
                if (TennisMatchManager.Instance.currentPlayersAmountID == 0)
                {
                    SetCourtSide(true);
                }
                break;
            case PlayState.ServingMove:
                if (controlledBy == ControlledBy.CPU)
                {
                    cpuServeTimer = 1f;
                }
                actionCooldown = 0;

                playerServeStartPosition.SetServingPosition();
                transform.position = playerServeStartPosition.GetStartPosition(false).position;
                ballHitter.ballHitAimer.target = playerServeStartPosition.GetAimerLookatPosition(false);
                TennisMatchManager.Instance.tennisBall.ResetBall(playerID);
                MainUI.Instance.servingSign.UpdateSign(transform);
                break;
            case PlayState.Serving:
                ballHitter.ballHitAimer.target = playerServeStartPosition.GetAimerLookatPosition(false);
                TennisMatchManager.Instance.tennisBall.ServeBall();
                MainUI.Instance.servingSign.gameObject.SetActive(false);
                MainUI.Instance.servingBar.UpdateBar(transform);
                break;
            case PlayState.OpposingServe:
                actionCooldown = 0;

                if (teamID == TeamID.team1)
                {
                    transform.position = secondPlayerInTeam ? playerServeStartPosition.GetStartPosition(true).position : playerServeStartPosition.GetStartPosition(false).position;
                }
                else
                {
                    transform.position = secondPlayerInTeam ? playerServeStartPosition.GetStartPosition(false).position : playerServeStartPosition.GetStartPosition(true).position;
                }

                playStateToSet = PlayState.Playing;
                break;
            case PlayState.SameTeamAsServe:
                actionCooldown = 0;

                transform.position = playerServeStartPosition.GetStartPosition(true).position;
                playStateToSet = PlayState.Playing;
                break;
        }

        playState = playStateToSet;
        ResetMoveState();
    }

    void ResetMoveState()
    {
        divePower = 0;
        diveTimer = 0;
        isJumping = false;
        divingParticle.SetActive(false);
    }

    bool CheckIfCanUseAction(int specialCost)
    {
        bool canUseAction = true;

        if (actionCooldown > 0 || playState == PlayState.Serving || playState == PlayState.ServingMove || specialStars.amountOfStars < specialCost && TennisMatchManager.Instance.currentGameType != GameTypeID.Classic)
        {
            canUseAction = false;
        }

        return canUseAction;
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
