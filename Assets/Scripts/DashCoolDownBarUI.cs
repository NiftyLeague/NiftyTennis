using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashCoolDownBarUI : MonoBehaviour
{
    public PlayerController playerAssignedTo;
    public Vector3 positionOffset;
    public Image dashBar;

    private void Update()
    {
        transform.position = playerAssignedTo.transform.position + positionOffset;
        dashBar.fillAmount = playerAssignedTo.actionCooldown / 1;
        if (playerAssignedTo.actionCooldown <= 0)
        {
            gameObject.SetActive(false);
            playerAssignedTo.CanMoveAgain();
        }
    }
}
