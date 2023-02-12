using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerLoading : MonoBehaviour
{
    public float transitionTime = 0.5f;
    private string targetScene;
    private AsyncOperation asyncOperation;
    public CanvasGroup transitionCanvas;
    private bool isLoading = false;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name != "LoadingScene")
        {
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        transitionCanvas.alpha = 1;
        float startValue = transitionCanvas.alpha;
        float time = 0;
        while (time < transitionTime)
        {
            transitionCanvas.alpha = Mathf.Lerp(startValue, 0, time / transitionTime);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        transitionCanvas.alpha = 0;
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    public void StartLoading(string scene)
    {
        if(!isLoading)
        {
            isLoading = true;
            targetScene = scene;
            transitionCanvas.blocksRaycasts = true;
            StartCoroutine(LoadingTransition());
        }
    }

    private void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetScene) && targetScene != null)
        {
            LoadingScreenInfo.targetSceneName = targetScene;
            if (SceneManager.GetActiveScene().name != "LoadingScene")
            {
                asyncOperation = SceneManager.LoadSceneAsync("LoadingScene");
            }
        }
        else
        {
            Debug.LogError("Invalid Destination");
            StopAllCoroutines();
        }
    }

    IEnumerator LoadingTransition()
    {
        LoadTargetScene();
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                float startValue = 0.0f;
                float time = 0;

                while (time < transitionTime)
                {
                    transitionCanvas.alpha = Mathf.Lerp(startValue, 1, time / transitionTime);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }
                transitionCanvas.alpha = 1;
                yield return new WaitForEndOfFrame();
                transitionCanvas.blocksRaycasts = false;

                asyncOperation.allowSceneActivation = true;
            }
            isLoading = false;
            yield return null;
        }
    }
}
