using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetInteractionSystem
{
public enum Axis {x,y,z}
public class PositionClamper
{
    private Transform obj;
    private Transform pointA;
    private Transform pointB;
    private Vector3 minPoint;
    private Vector3 maxPoint;

    private Axis moveAxis;
    private Vector3 moveDirection;

    public PositionClamper(Transform obj, Transform pointA, Transform pointB)
    {  
        this.obj = obj;
        this.pointA = pointA;
        this.pointB = pointB;
        moveAxis = GetMoveAxis();
        moveDirection = GetClampDirection();
        AssignMinMaxPoints();
    }

    Axis GetMoveAxis()
    {
        Transform pointInLine;
        if(pointA.localPosition != obj.localPosition)
            pointInLine = pointA;
        else if(pointB.localPosition != obj.localPosition)
            pointInLine = pointB;
        else
        {
            Debug.LogError("Could not determine move axis");
            return 0;
        }

        if(obj.localPosition.x != pointInLine.localPosition.x)
           return Axis.x;
        if(obj.localPosition.y != pointInLine.localPosition.y)
            return Axis.y;
        if(obj.localPosition.z != pointInLine.localPosition.z)
            return Axis.z;
        
        Debug.LogError("Could not determine move axis");
        return 0;
    }

    Vector3 GetClampDirection()
    {
        Vector3 clampDir = Vector3.zero;
        clampDir[(int)moveAxis] = 1.0f;
        return clampDir;
    }

    void AssignMinMaxPoints()
    {
        if(pointA==null || pointB == null)
        {
            Debug.LogError("object's positional points have not been assigned");
            return;
        }

        if(pointA.localPosition[(int)moveAxis] > pointB.localPosition[(int)moveAxis])
        {
            maxPoint = pointA.localPosition;
            minPoint = pointB.localPosition;
        }
        else
        {
            maxPoint = pointB.localPosition;
            minPoint = pointA.localPosition;
        }
    }

    public float GetAmountMoved()
    {
        return Mathf.Lerp(0,1,(obj.localPosition[(int)moveAxis]-pointA.localPosition[(int)moveAxis])/
                            (pointB.localPosition[(int)moveAxis]-pointA.localPosition[(int)moveAxis]));
    }

    public Vector3 GetClampedValue()
    {
        Vector3 positionVector = obj.localPosition;
        positionVector[(int)moveAxis] = Mathf.Clamp(positionVector[(int)moveAxis],minPoint[(int)moveAxis],maxPoint[(int)moveAxis]);
        return Vector3.Scale(positionVector,moveDirection); 
    }

}
}
