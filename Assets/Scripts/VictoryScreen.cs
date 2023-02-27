using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup victoryMenuGroup;
    public void Victory(PlayerInput player)
    {
        StartCoroutine(VictoryDelay(player));
    }

    private void VictoryActivate(PlayerInput player)
    {
        victoryMenuGroup.alpha = 1;
        victoryMenuGroup.interactable = true;
        victoryMenuGroup.blocksRaycasts = true;
        TimeManager.Instance.PauseTimeScale(true);
        player.gameObject.SetActive(false);
    }

    private IEnumerator VictoryDelay(PlayerInput player)
    {
        yield return new WaitForSeconds(1.5f);
        VictoryActivate(player);
    }
}
