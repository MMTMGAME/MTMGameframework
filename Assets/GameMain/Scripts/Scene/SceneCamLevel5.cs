using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SceneCamLevel5 : SceneCam
{
    public CinemachineVirtualCamera oldManCam;

    public void SetOldManCam(Transform target)
    {
        oldManCam.Follow = target;
        oldManCam.LookAt = target;
    }

    public void EnableOldManCam()
    {
        oldManCam.Priority = 11;
    }

    public void DisableOldManCam()
    {
        oldManCam.Priority = 9;
    }
}
