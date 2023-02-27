using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup victoryMenuGroup;
    public void Victory()
    {
        StartCoroutine(VictoryDelay());
    }

    private void VictoryActivate()
    {
        victoryMenuGroup.alpha = 1;
        victoryMenuGroup.interactable = true;
        victoryMenuGroup.blocksRaycasts = true;
        TimeManager.Instance.PauseTimeScale(true);
    }

    private IEnumerator VictoryDelay()
    {
        yield return new WaitForSeconds(1.5f);
        VictoryActivate();
    }
}
