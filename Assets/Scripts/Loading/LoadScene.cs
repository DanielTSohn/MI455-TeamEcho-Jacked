using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    public float transitionTime = 0.5f;
    private AsyncOperation asyncOperation;

    public CanvasGroup transitionCanvas;
    public Slider loadingBar;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TimeManager.Instance.ResumeTimeScale(true);
        TimeManager.Instance.ResetTime();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(LoadingTransition());
    }

    private IEnumerator FadeIn()
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
    }

    private IEnumerator LoadingTransition()
    {
        yield return StartCoroutine(FadeIn());

        asyncOperation = SceneManager.LoadSceneAsync(LoadingScreenInfo.targetSceneName);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            loadingBar.value = Mathf.Clamp01(asyncOperation.progress / 0.9f);
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

                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
