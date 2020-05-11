using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class DoF : MonoBehaviour
{
    public bool dofEnable;
    [SerializeField] float dofDistance;
    DepthOfField _dof;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!_dof)
            gameObject.GetComponent<PostProcessVolume>().profile.TryGetSettings(out _dof);
        if(dofEnable){
            _dof.active = true;
            _dof.focusDistance.value = dofDistance;
        }else{
            _dof.active = false;
        }
    }
}
