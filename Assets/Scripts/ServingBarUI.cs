using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServingBarUI : MonoBehaviour
{
    public Transform playerAssignedTo;
    public Vector3 positionOffset;
    public TennisBall ball;
    public Image serveBar;

    private void Update()
    {
        transform.position = playerAssignedTo.position + positionOffset;
        float currentServeBallPosition = ball.transform.GetChild(0).position.y / 1.35f;
        serveBar.fillAmount = currentServeBallPosition / 1;
        if (serveBar.fillAmount > 0.9)
        {
            serveBar.color = Color.red;
        }
        else
        {
            serveBar.color = Color.cyan;
        }
    }

    public void UpdateBar(Transform playerPosition)
    {
        playerAssignedTo = playerPosition;
        transform.position = playerAssignedTo.position + positionOffset;
        gameObject.SetActive(true);
    }
}
