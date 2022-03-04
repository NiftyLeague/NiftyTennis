using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    public float moveSpeed;
    private float moveSpeedMultiplier = 1;
    private float dashTimer;
    public float dashPower = 4;
    public float chargePower = 0;
    public ParticleSystem chargeParticle;
    public Transform ballServePosition;
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

    void FixedUpdate()
    {
        //MOVE INPUT
        if (playerID == PlayerID.Player1)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            var normal = input.normalized;
            var magnitude = input.magnitude;

            magnitude = Mathf.Clamp01(magnitude);
            input = normal * magnitude;

            Vector3 movement = new Vector3(input.x, 0.0f, input.y);
            if (dashTimer > 0)
            {
                rigidBody.velocity = movement * (moveSpeed * moveSpeedMultiplier) * dashPower;
                dashTimer -= Time.deltaTime;
            }
            else
            {
                rigidBody.velocity = movement * (moveSpeed * moveSpeedMultiplier);
            }

            spriteRenderer.flipX = Input.GetAxis("Horizontal") > 0 ? false : true;

            if (isHittingBall)
            {
                spriteAnimationTimer -= Time.deltaTime;

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
                spriteAnimationTimer -= Time.deltaTime;

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
                spriteAnimationTimer -= Time.deltaTime;

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
    }

    private void Update()
    {
        //INPUT
        if (playerID == PlayerID.Player1)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (dashTimer <= 0)
                {
                    dashTimer = 0.15f;
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                ballHitter.TurnOnHitter();
                chargePower = 0;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                ballHitter.TurnOnHitter(HitType.Weak);
                chargePower = -0.8f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                ballHitter.TurnOnHitter(HitType.Hard);
                chargePower += Time.deltaTime;
                if (chargePower > 1.0f)
                {
                    chargePower = 1.0f;
                }
            }
            else
            {
                chargePower = 0;
            }

            var chargeParticleEmission = chargeParticle.emission;
            chargeParticleEmission.rateOverTime = chargePower * 500;
        }
    }

    public void HitBall()
    {
        UnReady();
        isHittingBall = true;
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
}
