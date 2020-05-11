using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SoundManager : MonoBehaviour
{
    public AudioClip introMusic;
    public AudioClip mainMusic;
    // [Header("WeaponSounds")]
    // public AudioClip projectile;
    // public AudioClip bonusProjectile;
    // public AudioClip hookShot;
    // public AudioClip hookHitMiss;
    // public AudioClip hookHitSuccess;
    // [Header("EnemySounds")]
    // public AudioClip flyMove;
    // public AudioClip crawlerMove;
    // public AudioClip crawlerAdjust;
    private AudioSource audioSource;
    private PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView=GetComponent<PhotonView>();
        audioSource=GetComponent<AudioSource>();

        GameManager.instance.onGameStart.AddListener(PlayGameMusic);
        GameManager.instance.onGameOver.AddListener(PlayIntroMusic);
    }

    public void PlayIntroMusic()
    {
        StartCoroutine(FadeTo(introMusic,1.5f,0.7f));
    }

    public void PlayGameMusic()
    {
        StartCoroutine(FadeTo(mainMusic,1.5f,0.7f));
    }

    IEnumerator FadeTo(AudioClip clip, float time, float volume)
    {
        float startingVol = audioSource.volume;
        float elapsedTime=0f;
        while(elapsedTime<=time)
        {
            elapsedTime+=Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startingVol,0,elapsedTime/time);
            yield return null;
        }

        audioSource.clip = clip;
        audioSource.Play();
        elapsedTime=0f;
        while(elapsedTime<=time)
        {
            elapsedTime+=Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0,volume,elapsedTime/time);
            yield return null;
        }
    }

    void onDestroy()
    { 
        if(GameManager.instance!=null)
        {
            GameManager.instance.onGameStart.RemoveListener(PlayGameMusic);
            GameManager.instance.onGameOver.RemoveListener(PlayIntroMusic);
        }
    }
}
