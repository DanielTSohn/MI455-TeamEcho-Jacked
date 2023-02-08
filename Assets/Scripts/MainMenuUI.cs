using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame()
    {
        // Game Scene goes here
        //SceneManager.LoadScene("");
        Debug.Log("Play the game!");
    }

    public void QuitGame()
    {
        Debug.Log("Quit the game!");
        Application.Quit();
    }
}
