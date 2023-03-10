using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private List<FixedJoint> joints;
    [HideInInspector] public List<GameObject> neighbors;
    public bool destroyed = false;

    public Vector3Int gridLocation;
}
