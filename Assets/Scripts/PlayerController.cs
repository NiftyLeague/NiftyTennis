using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerID playerID;
    public Rigidbody rigidBody;
    public BallHitter ballHitter;
    public float moveSpeed;
    float dashTimer;
    public float dashPower = 4;
    float ballHitterTimer;
    public float chargePower = 0;
    public ParticleSystem chargeParticle;
    public Transform ballServePosition;

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
                rigidBody.velocity = movement * moveSpeed * dashPower;
                dashTimer -= Time.deltaTime;
            }
            else
            {
                rigidBody.velocity = movement * moveSpeed;
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
}
