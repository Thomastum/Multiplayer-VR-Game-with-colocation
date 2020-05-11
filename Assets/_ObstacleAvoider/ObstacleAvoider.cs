using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoider : MonoBehaviour
{

   [SerializeField] private Transform _target;
   
   private void FixedUpdate()
   {
      TryToTurnBeforeObstacle();
   }
   
   
   private void TryToTurnBeforeObstacle()
   {
      // Try to avoid obstacles 
      var direction = (_target.position - transform.position).normalized;
      RaycastHit hit;
      
      // Check forward raycast
      if (Physics.Raycast(transform.position, transform.forward, out hit, 20))
      {
         if (hit.transform != transform)
         {
            Debug.DrawLine(transform.position,   hit.point, Color.red);            
            direction += hit.normal * 150;
         }
      }

      
      var leftR = transform.position;
      var rigthR  = transform.position;
      leftR.x -= 1.5f;
      rigthR.x += 1.5f;
      
      if (Physics.Raycast(leftR, transform.forward, out hit, 20))
      {
         if (hit.transform != transform)
         {
            Debug.DrawLine(leftR, hit.point, Color.red);
            direction += hit.normal * 150;
         }
      }
      
      if (Physics.Raycast(rigthR, transform.forward, out hit, 20))
      {
         if (hit.transform != transform)
         {
            Debug.DrawLine(rigthR, hit.point, Color.red);
            direction += hit.normal * 150;
         }
      }

        
      var rot = Quaternion.LookRotation(direction);
      transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
      transform.position += transform.forward * 5 * Time.deltaTime;
   }
}
