using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForResume : CustomYieldInstruction
{
    public override bool keepWaiting
    {
        get
        {
            return TimeManager.Instance.IsPaused;
        }
    }
}

public class TimeManager : MonoBehaviour
{
    #region Public Variables
    #region Components
    [Header("Components")]
    [Tooltip("The time settings scriptable object for base time information")]
    public TimeSettings timeSettings;
    #endregion
    
    /// <summary>
    /// Is the game paused? 
    /// </summary>
    /// <remarks>
    /// Is the time scale set to 0
    /// </remarks>
    public bool IsPaused { get; private set; }
    /// <summary>
    /// Is the game paused via a Menu?
    /// </summary>
    /// <remarks>
    /// An additional modifier to the paused state
    /// </remarks>
    public bool MenuPaused { get; private set; }
    #endregion

    #region Private Variables
    [SerializeField]
    private GameEvent OnPause, OnMenuPause, OnResume, OnTimeChange;
    private List<float> timeMultipliers = new();
    #endregion
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);

            ResetTime();
        }
    }

    /// <summary>
    /// Resets time to base time scale and fixed time step and clears all multipliers
    /// </summary>
    public void ResetTime()
    {
        StopAllCoroutines();
        timeMultipliers.Clear();
        Time.timeScale = timeSettings.baseTimeScale;
        Time.fixedDeltaTime = timeSettings.baseFixedTimeStep;
    }

    private void ChangeTime(float multiplier)
    {
        timeMultipliers.Add(multiplier);

        SetTime();
    }

    private void SetTime()
    {
        float finalMult = 1;
        foreach (float mult in timeMultipliers)
        {
            finalMult *= mult;
        }

        finalMult = Mathf.Clamp(finalMult, timeSettings.minimumTimeScale / timeSettings.baseTimeScale, timeSettings.maximumTimeScale / timeSettings.baseTimeScale);

        Time.timeScale = timeSettings.baseTimeScale * finalMult;
        Time.fixedDeltaTime = timeSettings.baseFixedTimeStep * finalMult;
    }

    /// <summary>
    /// Set the time scale directly to given number
    /// </summary>
    /// <param name="timeScale">What number should the timescale be set to </param>
    public void AdjustTimeScale(float timeScale)
    {
        ChangeTime(timeScale);
        OnTimeChange.TriggerEvent(timeScale);
        OnTimeChange.TriggerEvent();
    }

    /// <summary>
    /// Set the time scale either directly by given number or multiplicatively off of the current time scale
    /// </summary>
    /// <param name="multiplier">The direct assignment or multiplier depending on overide settings</param>
    /// <param name="overide">True to direct assign, false to multiply</param>
    public void AdjustTimeScale(float multiplier, bool overide)
    {
        if(overide)
        {
            AdjustTimeScale(multiplier);
        }
        else
        {
            AdjustTimeScale(multiplier * Time.timeScale);
        }
    }

    /// <summary>
    /// Set the time scale either diirectly by given number or multiplicatively off of the current time scale for the given time 
    /// </summary>
    /// <param name="multiplier">The direct assignment or multiplier depending on overide settings</param>
    /// <param name="overide">True to direct assign, false to multiply</param>
    /// <param name="duration">The time the adjustment lasts, reverts back after time has passed</param>
    /// <returns>The coroutine that is counting the time</returns>
    public Coroutine AdjustTimeScale(float multiplier, bool overide, float duration)
    {
        if (overide)
        {
            AdjustTimeScale(multiplier);
        }
        else
        {
            AdjustTimeScale(multiplier, false);
        }

        return StartCoroutine(CountTime(duration, multiplier, overide));
    }

    /// <summary>
    /// Counts the time for AdjustTimeScale, only counts while not paused; resumes counting when game resumes.
    /// </summary>
    /// <param name="duration">How long to count before setting time scale to/multiply by given value</param>
    /// <param name="prevTimeScale">The time scale to set to when the counter ends</param>
    /// <param name="overide">Whether to overide or multiply by the given time scale</param>
    /// <returns></returns>
    private IEnumerator CountTime(float duration, float multiplier, bool overide)
    {
        float time = 0;
        
        while(time < duration)
        {
            if(!IsPaused)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            else
            {
                yield return new WaitUntil(() => IsPaused == false);
            }
        }

        timeMultipliers.Remove(multiplier);
        SetTime();
    }

    /// <summary>
    /// Set the time scale to 0; pausing the game and invokes the pause event. 
    /// </summary>
    public void PauseTimeScale()
    {
        IsPaused = true;

        Time.timeScale = 0;
        OnPause.TriggerEvent();
    }

    /// <summary>
    /// Set the time scale to 0; pausing the game. Will save the time scale before a pause occured and invoke the pause event. 
    /// Additional option to apply menu pause
    /// </summary>
    /// <param name="menuPause">True to menu pause, false to pause normally</param>
    public void PauseTimeScale(bool menuPause)
    {
        if (menuPause)
        {
            MenuPaused = true;
            Cursor.lockState = CursorLockMode.None;
            PauseTimeScale();
            OnMenuPause.TriggerEvent();
        }
        else
        {
            PauseTimeScale();
        }
    }

    /// <summary>
    /// Set the time scale to 0; pausing the game. Will save the time scale before a pause occured and invoke the pause event. 
    /// Additional option to apply menu pause. 
    /// Additional option to stop all other timed effects
    /// </summary>
    /// <param name="menuPause">True to menu pause, false to pause normally</param>
    /// <param name="clearEffects">True to disable all timed effects, false to pause with menu pause option</param>
    public void PauseTimeScale(bool menuPause, bool clearEffects)
    {
        if(clearEffects)
        {
            StopAllCoroutines();
        }
        PauseTimeScale(menuPause);
    }

    /// <summary>
    /// Temporarily pause the game for given duration; it is not compatible with menu pausing.
    /// Additional option to stop all other timed effects
    /// </summary>
    /// <param name="duration">Time to stay paused, resume after timer counts out</param>
    /// <param name="clearEffects">True to disable all timed effects, false to pause with menu pause option</param>
    public Coroutine PauseTimeScale(float duration, bool clearEffects = false)
    {
        if (clearEffects)
        {
            StopAllCoroutines();
        }

        PauseTimeScale();

        OnPause.TriggerEvent();
        return StartCoroutine(CountPausedTime(duration));
    }

    /// <summary>
    /// Counts time for PauseTimeScale, will not count while menu paused; resumes counting upon menu pause ending. Resumes after timer counts out
    /// </summary>
    /// <param name="duration">Time to count before resuming</param>
    /// <returns></returns>
    private IEnumerator CountPausedTime(float duration)
    {
        float time = 0;

        while (time < duration)
        {
            if (!MenuPaused)
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            else
            {
                yield return new WaitUntil(() => MenuPaused == false);
            }
        }
        ResumeTimeScale();
    }

    /// <summary>
    /// Resumes time scale from stored scale data; only resumes if the game was paused.
    /// </summary>
    public void ResumeTimeScale(bool menuResume = false)
    {
        if(menuResume)
        {
            if (MenuPaused)
            {
                MenuPaused = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
        if (IsPaused)
        {
            SetTime();

            IsPaused = false;
            OnResume.TriggerEvent();
        }
    }
}