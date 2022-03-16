using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform lookatTarget;
    public Vector3 lookatOffset;
    public float speed;
    public List<Transform> playerTransforms;

    private Quaternion targetRotation;

    private void FixedUpdate()
    {
        lookatTarget.position = PlayersPositions() + lookatOffset;
        targetRotation = Quaternion.LookRotation(lookatTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.fixedTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, 0);
    }

    private Vector3 PlayersPositions()
    {
        Vector3 playerPositions = Vector3.zero;

        switch (playerTransforms.Count)
        {
            case 2:
                playerPositions = (playerTransforms[0].position + playerTransforms[1].position) / playerTransforms.Count;
                break;
            case 3:
                playerPositions = (playerTransforms[0].position + playerTransforms[1].position + playerTransforms[2].position) / playerTransforms.Count;
                break;
            case 4:
                playerPositions = (playerTransforms[0].position + playerTransforms[1].position + playerTransforms[2].position + playerTransforms[3].position) / playerTransforms.Count;
                break;
        }
       
        return playerPositions;
    }

    public void AddPlayersToCamera(List<Transform> playersToAdd, Vector3 setCameraOffSetTo) 
    {
        playerTransforms.Clear();
        playerTransforms = playersToAdd;
        lookatOffset = setCameraOffSetTo;
    }
}
