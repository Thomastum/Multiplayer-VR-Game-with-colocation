using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerAvoid : MonoBehaviour
{
    
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float mass = 5.0f;
    [SerializeField] private float force = 40.0f;
    [SerializeField] private float minimumDistToAvoid = 5.0f;

    
    private float velocity;
    private Vector3 targetPoint;
    //private float initialSpeed;
    private Vector3 desiredDestination;
    private Rigidbody rb;


    [Header("Transform Target")] 
    [SerializeField] private Transform targetTransform;
    
    
    // Start is called before the first frame update
    void Start()
    {
        mass = 5.0f;
        targetPoint = this.transform.localPosition;
        rb = gameObject.GetComponent<Rigidbody>();
        //initialSpeed = speed;

        if (targetTransform != null)
        {
            targetPoint = targetTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vehicle move by mouse click
        RaycastHit hit;
        //Retrieve the mouse click position by shooting a ray from the camera
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)&& Physics.Raycast(ray, out hit, 100.0f))
        {
            //Take the point where the ray hits the ground plane as the target rotation
            targetPoint = hit.point;
        }

        //1- Compute the directional vector to the target position
        desiredDestination = targetPoint - transform.localPosition;

        // //2- When the target point is 1 meter away, exit the update function, so that the vehicle stops 
        if (desiredDestination.sqrMagnitude < 1)
            return;

        //3- Adjust the speed to delta time
        velocity = speed * Time.deltaTime;

        //4- Apply obstacle avoidance
        var newVector = AvoidObstacles();

        //5- Rotate the vehicle to its target directional vector
        LookAtTarget(newVector);

        //6- Move the vehicle towards the target point
        SetDestination();
    }
    
    public Vector3 AvoidObstacles()
    {
        RaycastHit hit;

        float shoulderMultiplier = 1f;
        Vector3 leftR = transform.localPosition - (transform.right * shoulderMultiplier);
        Vector3 rightR = transform.localPosition + (transform.right * shoulderMultiplier);

        if (Physics.Raycast(leftR, transform.forward, out hit, minimumDistToAvoid))
        {
            if (hit.transform != transform)
            {
                Debug.DrawLine(leftR, hit.point, Color.blue);
                desiredDestination += hit.normal * force;
            }
        }

        else if (Physics.Raycast(rightR, transform.forward, out hit, minimumDistToAvoid))
        {
            if (hit.transform != transform)
            {
                Debug.DrawLine(rightR, hit.point, Color.green);
                desiredDestination += hit.normal * force;
            }
        }

        return desiredDestination;
    }
    
    void LookAtTarget(Vector3 target)
    {
        Quaternion rot = Quaternion.LookRotation(target);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
    }

    void SetDestination()
    {
        Debug.DrawLine(targetPoint, transform.position, Color.red);

        //transform.position += transform.forward * velocity;
        rb.position += transform.forward * velocity;

    }
}
