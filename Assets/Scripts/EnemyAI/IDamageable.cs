using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void GetHit(RaycastHit hitInfo, int damage, int knockbackForce);   
    void GetHookHit(Transform hook,RaycastHit hitInfo);

    bool IsEyeHit(Collider hitCollider);
    float GetHealth();

}
