using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVacuumable
{
    bool CanBeVacuumed();
    void LockToPoint(Transform vacuumPoint);
    void GetVacuumed();
    void VacuumRelease();
}
