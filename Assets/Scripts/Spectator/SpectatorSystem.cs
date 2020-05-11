using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public enum SpectatorMode { Free, Cinematic, View };

[ExecuteInEditMode]
public class SpectatorSystem : MonoBehaviour
{
    [Header("Global Properties")]
    [SerializeField] SpectatorMode mode;
    [SerializeField] GameObject camera;
    [SerializeField] Animator animator;
    [SerializeField] TMPro.TextMeshProUGUI endText;

    [Header("Free Spectator")]
    [SerializeField] GameObject freeCameraGameobject;

    [Header("Cinematic Spectator")]
    [SerializeField] GameObject cinematicCameraGameobject;
    [SerializeField] PlayableDirector timeline;
    [SerializeField] List<float> times;

    DoF _dof;
    GameManager _gameManager;
    PhotonManager _photonManager;
    bool _checkForCameras = true;
    [SerializeField] List<Camera> _cameraObjects;
    int _current = 1;

    void Start()
    {
        _dof = GetComponentInChildren<DoF>();

        GameObject gm = GameObject.Find("GameManager");

        if (!gm)
        {
            Debug.LogError("Spectator could't find GameManager object");
            return;
        }

        _gameManager = gm.GetComponent<GameManager>();
        _photonManager = gm.GetComponent<PhotonManager>();

        _gameManager.onGameOver.AddListener(End);

        StartCoroutine(CameraFinder());

        EnableMode(mode);
    }

    private void OnDestroy() {
        _gameManager.onGameOver.RemoveListener(End);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EnableMode(SpectatorMode.Free);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            EnableMode(SpectatorMode.Cinematic);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            EnableMode(SpectatorMode.View);

        if(Input.GetKeyDown(KeyCode.X)){
            _photonManager.QuitRoom();
        }

        if(_cameraObjects.Count == 1 && mode == SpectatorMode.View)
            EnableMode(SpectatorMode.Cinematic);

            
    }


    public void EnableMode(SpectatorMode newMode)
    {
        animator.SetTrigger("Cut");
        StartCoroutine(Change(newMode));
    }

    public void End(){
        StartCoroutine(EndCoroutine());
    }

    IEnumerator EndCoroutine(){
        while(animator.GetCurrentAnimatorStateInfo(0).IsName("Cut")){
            Debug.Log("Wait");
            yield return new WaitForEndOfFrame();
        }
        endText.text = "THEY SURVIVED FOR <b>"+_gameManager.timeSurvived.ToString("00") +"</b> SECONDS";
        animator.SetTrigger("End");
        WaitForSeconds ws2 = new WaitForSeconds(0.3f);
        yield return ws2;
        if(mode == SpectatorMode.Cinematic)
            timeline.Pause();
        WaitForSeconds ws = new WaitForSeconds(3.3f);
        yield return ws;
        if(mode == SpectatorMode.Cinematic)
            timeline.Resume();
    }

    IEnumerator CameraFinder()
    {
        WaitForSeconds ws = new WaitForSeconds(1f);
        while (_checkForCameras)
        {
            var found = FindObjectsOfType<Camera>();
            Camera[] cms = found;
            for (int i = _cameraObjects.Count - 1; i >= 0; i--)
            {
                if (System.Array.IndexOf(cms, _cameraObjects[i]) == -1)
                    _cameraObjects.RemoveAt(i);
            }
            for (int i = cms.Length - 1; i >= 0; i--)
            {
                if (!_cameraObjects.Contains(cms[i]) && ( cms[i].name == "CenterEyeAnchor" || cms[i].name == "Camera" ))
                    _cameraObjects.Add(cms[i]);
            }
            yield return ws;
        }
    }

    IEnumerator Change(SpectatorMode newMode)
    {
        yield return new WaitForSeconds(0.5f);
        mode = newMode;
        freeCameraGameobject.SetActive(false);
        cinematicCameraGameobject.SetActive(false);
        camera.GetComponent<Camera>().enabled = true;
        if (_cameraObjects.Count > 1)
            _cameraObjects[1].enabled = false;
        if (_cameraObjects.Count > 2)
            _cameraObjects[2].enabled = false;
        switch (mode)
        {
            case SpectatorMode.Free:
                _dof.dofEnable = false;
                freeCameraGameobject.SetActive(true);
                break;
            case SpectatorMode.Cinematic:
                _dof.dofEnable = true;
                cinematicCameraGameobject.SetActive(true);
                timeline.Stop();
                timeline.time = times[Random.Range(0, times.Count-1)];
                timeline.Evaluate();
                timeline.Play();
                break;
            case SpectatorMode.View:
                _dof.dofEnable = false;
                if(_cameraObjects.Count != 1){
                    camera.GetComponent<Camera>().enabled = false;
                    _cameraObjects[_current].enabled = true;
                    _cameraObjects[_current].fieldOfView = 70;
                    if (_current == 1 && _cameraObjects.Count == 3)
                        _current = 2;
                    else if (_current == 2)
                        _current = 1;
                }

                break;
        }
    }
}
