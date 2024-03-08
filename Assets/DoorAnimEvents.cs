using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class DoorAnimEvents : MonoBehaviour
{
    public void PlayOpenDoorAudio()
    {
        GameEntry.Sound.PlaySound(10000);
    }
}
