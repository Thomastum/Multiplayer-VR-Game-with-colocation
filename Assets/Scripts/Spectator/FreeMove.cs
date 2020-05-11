using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMove : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] string x;
    [SerializeField] string y;
    [SerializeField] string z;
    [SerializeField] string zoom;

    [Header("Properties")]
    [SerializeField] float speed;
    [SerializeField] float sensitivityX = 15F;
    [SerializeField] float sensitivityY = 15F;
    [SerializeField] float fow = 40;
    [SerializeField] Cinemachine.CinemachineVirtualCamera virtualCamera;


    Vector3 _newPos;
    float _t = 0;
    float _rotationX;
    float _rotationY;
    Quaternion _originalRotation = Quaternion.identity;
    Rigidbody _rigidbody;

    void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _newPos = new Vector3(Input.GetAxis(x), Input.GetAxis(y), Input.GetAxis(z));
        

        _rotationX += Input.GetAxis("Mouse X") * sensitivityX * (fow/60);
        _rotationY += Input.GetAxis("Mouse Y") * sensitivityY * (fow/60);
        _rotationX = ClampAngle(_rotationX, -360, 360);
        _rotationY = ClampAngle(_rotationY, -85, 85);
        Quaternion xQuaternion = Quaternion.AngleAxis(_rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(_rotationY, -Vector3.right);

        transform.localRotation = Quaternion.Lerp(transform.localRotation, _originalRotation * xQuaternion * yQuaternion, Time.deltaTime*10);

        fow += Input.GetAxis(zoom);
        if(fow < 20)
            fow = 20;
        if(fow > 80)
            fow = 80;
        virtualCamera.m_Lens.FieldOfView = fow;
    }

    private void FixedUpdate() {
        if (_newPos != Vector3.zero)
        {
            _rigidbody.AddRelativeForce(_newPos * (speed / 10), ForceMode.Impulse);
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
         angle += 360F;
        if (angle > 360F)
         angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
