using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitRoom : MonoBehaviour
{
    public Color pressedColor;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonManager.instance.QuitRoom();
        DisableButton();
    }

    void DisableButton()
    {
        gameObject.GetComponent<Renderer>().material.color = pressedColor;
        gameObject.GetComponent<Collider>().enabled = false;
    }
}
