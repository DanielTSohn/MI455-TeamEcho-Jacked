using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimed : MonoBehaviour
{
    [SerializeField]
    private GameEvent wallDestroyed;
    [SerializeField]
    [Tooltip("Parent of all debris, used to destroy everything")]
    private Transform debrisParent;
    [SerializeField]
    [Tooltip("Explosion force applied to all children of debris parent after delay time")]
    private float explosionForce;
    [SerializeField]
    [Tooltip("Time until the object and children is completely destroyed, counts down after delay time")]
    private float destroyTime;

    public float DelayTime { get { return delayTime; } set { delayTime = value; } }
    [SerializeField]
    [Tooltip("The time before explosion force is applied")]
    private float delayTime;

    private void OnEnable()
    {
        StartCoroutine(CountDelay());
    }

    private IEnumerator CountDelay()
    {
        for (float time = 0; time < delayTime; time += Time.fixedDeltaTime)
        {
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            yield return new WaitForFixedUpdate();
        }
        if (wallDestroyed != null) { wallDestroyed.TriggerEvent(); }
        Debug.Log("Destroy");
        for (int i = 0; i < debrisParent.childCount; i++)
        {
            debrisParent.GetChild(i).TryGetComponent(out Rigidbody debrisRB);
            if (debrisRB != null)
            {
                debrisRB.useGravity = true;
                debrisRB.isKinematic = false;
                debrisRB.AddExplosionForce(explosionForce, debrisParent.position, 5, 0, ForceMode.VelocityChange);
            }
        }

        for (float time = 0; time < destroyTime; time += Time.fixedDeltaTime)
        {
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) { yield return new WaitForResume(); }
            yield return new WaitForFixedUpdate();
        }

        Destroy(gameObject);
    }
}
