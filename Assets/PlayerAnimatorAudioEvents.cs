using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class PlayerAnimatorAudioEvents : MonoBehaviour
{
    public void PlayFootStepAudios()
    { 
        GameEntry.Sound.PlaySound(UnityEngine.Random.Range(10002, 10006));
    }
    
    public void PlayJumpAudios()
    {
        
        GameEntry.Sound.PlaySound(UnityEngine.Random.Range(10007, 10011));
        GameEntry.Sound.PlaySound(10011);
    }

    public void PlayLandAudio()
    {
        GameEntry.Sound.PlaySound(10006);
    }

    public void PlaySlideAudio()
    {
        GameEntry.Sound.PlaySound(10020);
    }
    
}
