using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleCalculator : MonoBehaviour
{
    public Transform target;
    public float angle;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        angle = Vector3.Angle(Vector3.forward,(target.position-transform.position).normalized);
    }
}
