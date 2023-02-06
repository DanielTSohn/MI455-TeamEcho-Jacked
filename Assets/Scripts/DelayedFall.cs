using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedFall : MonoBehaviour
{
    [SerializeField]
    private GameEvent tileFall;
    [SerializeField]
    private Transform debrisParent;
    [SerializeField]
    private float explosionForce;


    public float DelayTime { get { return delayTime; } set { delayTime = value; } }
    [SerializeField]
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
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        tileFall.TriggerEvent();
        for(int i = 0; i < debrisParent.childCount; i++)
        {
            debrisParent.GetChild(i).TryGetComponent(out Rigidbody debrisRB);
            if(debrisRB != null) 
            { 
                debrisRB.useGravity = true; 
                debrisRB.isKinematic = false; 
                debrisRB.AddExplosionForce(explosionForce, debrisParent.position, 5, 0, ForceMode.VelocityChange); 
            }
        }
    }
}