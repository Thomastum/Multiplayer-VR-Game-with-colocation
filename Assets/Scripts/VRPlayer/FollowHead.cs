using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class FollowHead : MonoBehaviour
{
    public Transform head;
    private GameObject headModel;
    public Vector3 bodyOffset;
    private Vector3 originalOffset;
    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        originalOffset = transform.localPosition;
        headModel = head.GetChild(0).gameObject;

        if(photonView.IsMine)
        {
            headModel.SetActive(false);
            foreach(Transform bodyPart in transform)
                bodyPart.GetComponent<MeshRenderer>().enabled=false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        transform.localPosition = head.localPosition + originalOffset;
        transform.localEulerAngles = new Vector3(0,head.localEulerAngles.y,0); 
    }
}
