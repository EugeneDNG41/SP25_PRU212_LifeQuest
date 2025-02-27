using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //tao bien luu tru
    public AudioSource musicAudioSourse;
    public AudioSource vfxAudioSourse;
    public AudioClip BgMusic;
   


    void Start()
    {
        //nhac backGround
        musicAudioSourse.clip = BgMusic;
        musicAudioSourse.Play();
    }

    
    void Update()
    {
        
    }
}
