using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Vector3 offset = Vector3.zero;
    [SerializeField]
    private bool offsetFromCurrentPosition = false;

    public enum UpdateType { Update, FixedUpdate, LateUpdate };
    [SerializeField]
    private UpdateType updateChoice = UpdateType.LateUpdate;

    private void Awake()
    {
        if (offsetFromCurrentPosition) { offset = target.position - transform.position; }
    }

    private void Update()
    {
        if(updateChoice == UpdateType.Update)
        {
            Follow();
        }
    }

    private void FixedUpdate()
    {
        if (updateChoice == UpdateType.FixedUpdate)
        {
            Follow();
        }
    }

    private void LateUpdate()
    {
        if (updateChoice == UpdateType.LateUpdate)
        {
            Follow();
        }
    }

    private void Follow()
    {
        transform.position = target.position + offset;
    }
}
