using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas instance;
    // Start is called before the first frame update
    public TextMeshProUGUI frameRate;

    void Awake()
    {
        if(instance==null)
            instance=this;
        

    }
    void OnDestroy()
    {
        if(instance==this)
            instance=null;
    }

    void Start()
    {
    }

    float framerateTickTime = .5f;
    float timePassed=0;
    int framesPassed =0;
    void Update()
    {   
        timePassed += Time.unscaledDeltaTime;
        framesPassed++;
        if(timePassed>=framerateTickTime)
        {
            frameRate.text = Mathf.RoundToInt(framesPassed/timePassed).ToString();
            timePassed=0;
            framesPassed=0;
        }
    }

}
