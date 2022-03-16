using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServingSignUI : MonoBehaviour
{
    public Transform playerAssignedTo;
    public Vector3 positionOffset;
    public TextMeshProUGUI servingText;
    private float colorTimer;
    private bool isWhite;

    void Update()
    {
        transform.position = playerAssignedTo.position + positionOffset;
        colorTimer += Time.deltaTime;
        if (colorTimer > 0.2f)
        {
            isWhite = !isWhite;
            switch (isWhite)
            {
                case true: servingText.color = Color.white; break;
                case false: servingText.color = Color.cyan; break;
            }

            colorTimer = 0;
        }
    }

    public void UpdateSign(Transform playerPosition)
    {
        playerAssignedTo = playerPosition;
        gameObject.SetActive(true);
    }
}
