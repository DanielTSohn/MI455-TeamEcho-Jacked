using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerData
{
    public GameObject player;
    public JackhammerStandard moveScript;
    public Image jumpCD;
    public Image chargeBar;
}

public class GameUIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerData[] players;

    private void FixedUpdate()
    {
        foreach(PlayerData data in players)
        {
            UpdateJumpCD(data);
            UpdateChargeBar(data);
        }

    }

    public void UpdateJumpCD(PlayerData player)
    {
        player.jumpCD.fillAmount = player.moveScript.jumpCDProportion;
    }

    public void UpdateChargeBar(PlayerData player)
    {
        player.chargeBar.fillAmount = player.moveScript.jumpProportion;
    }
}
