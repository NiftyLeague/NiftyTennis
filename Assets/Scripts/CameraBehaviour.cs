using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform lookatTarget;
    public Vector3 lookatOffset;
    public float speed;
    public Transform player1;
    public Transform player2;
    public Transform player3;
    public Transform player4;

    private Quaternion targetRotation;

    private void FixedUpdate()
    {
        lookatTarget.position = ((player1.position + player2.position) / 2) + lookatOffset;
        targetRotation = Quaternion.LookRotation(lookatTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.fixedTime);
    }
}
