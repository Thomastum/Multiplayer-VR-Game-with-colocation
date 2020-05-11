using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetInteractionSystem
{
public class RotationClamper
{
    private Transform obj;
    private float minAngle;
    private float maxAngle;

    private Axis rotationAxis;
    private Vector3 rotateDirection;

    public RotationClamper(Transform obj, float minAngle, float maxAngle, Axis rotationAxis)
    {  
        this.obj = obj;
        this.minAngle = minAngle;
        this.maxAngle = maxAngle;
        this.rotationAxis = rotationAxis;
        rotateDirection = GetClampDirection();
    }

    Vector3 GetClampDirection()
    {
        Vector3 clampDir = Vector3.zero;
        clampDir[(int)rotationAxis] = 1.0f;
        return clampDir;
    }

    public Vector3 GetClampedValue()
    {
        Vector3 clampedVector = obj.localEulerAngles;
        clampedVector[(int)rotationAxis] = FixRotation(clampedVector[(int)rotationAxis]); //eulerangles don't go negative, so need to be fixed
        clampedVector[(int)rotationAxis] = Mathf.Clamp(clampedVector[(int)rotationAxis], minAngle, maxAngle);
        return Vector3.Scale(clampedVector,rotateDirection); 
    }

    float FixRotation(float angle)
    {
        if(angle > 180f)
            angle = angle-360f;
        
        return angle;
    }

}
}
