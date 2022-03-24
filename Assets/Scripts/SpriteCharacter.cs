using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCharacter : MonoBehaviour
{
    public PlayerController playerAssignedTo;
    public SpriteRenderer shadowRenderer;

    public void IsHighUp()
    {
        shadowRenderer.color = new Color32(0, 0, 0, 60);
        shadowRenderer.transform.localScale = new Vector3(2, 2, 2);
    }

    public void IsOnGround()
    {
        shadowRenderer.color = new Color32(0, 0, 0, 200);
        shadowRenderer.transform.localScale = new Vector3(3, 3, 3);
    }

    public void EndJumping()
    {
        playerAssignedTo.EndJumping();
    }
}
