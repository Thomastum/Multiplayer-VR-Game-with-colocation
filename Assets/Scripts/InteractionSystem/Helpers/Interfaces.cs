using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public interface IHoverable
{
    void StartHover();
    void StayHover();
    void StopHover();
}

public interface IGrabbable
{
    bool IsGrabbed();
    bool CanBeGrabbed();
    void GetGrabbed(GameObject hand);

    [PunRPC]
    void GetGrabbedNet(int viewID);

    void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity);

    [PunRPC]
    void GetReleasedNet(); 
}