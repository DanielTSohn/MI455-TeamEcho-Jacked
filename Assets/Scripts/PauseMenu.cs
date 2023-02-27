using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup pauseMenuGroup;
    public void PauseGame()
    {
        Debug.Log("Pause");
        pauseMenuGroup.alpha = 1;
        pauseMenuGroup.interactable = true;
        pauseMenuGroup.blocksRaycasts = true;
        TimeManager.Instance.PauseTimeScale(true);
    }

    public void ResumeGame()
    {
        Debug.Log("Resume");
        pauseMenuGroup.alpha = 0;
        pauseMenuGroup.interactable = false;
        pauseMenuGroup.blocksRaycasts = false;
        TimeManager.Instance.ResumeTimeScale(true);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void ProcessPause()
    {
        if (!TimeManager.Instance.MenuPaused)
        {
            PauseGame();
        }
    }
}
