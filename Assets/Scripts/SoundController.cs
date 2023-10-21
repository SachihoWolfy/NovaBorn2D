using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SoundController : MonoBehaviourPun
{
    //This Experimental Class focuses on having different ways to play audioClips,
    //mainly to also have sound be played to everyone as well.
    public static SoundController instance;
    void Awake()
    {
        instance = this;
    }
    [PunRPC]
    private void PlaySoundRPC(AudioSource AS, AudioClip clip)
    {
        AS.PlayOneShot(clip);
    }
    public void PlaySound(AudioSource AS, AudioClip clip)
    {
        AS.PlayOneShot(clip);
        //photonView.RPC("PlaySoundRPC", RpcTarget.Others, AS, clip);
    }
}
