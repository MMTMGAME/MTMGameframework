using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityGameFramework.Runtime;

public class CameraShakeComponent : GameFrameworkComponent
{
    public List<Cinemachine.NoiseSettings> noiseProfiles; // List of Noise Profiles

    private CinemachineBrain cinemachineBrain;
    private Coroutine fadeoutCoroutine;

    public void ShakeCamera(float amplitude, float frequency, float duration, int noiseIndex = 0)
    {
        if (cinemachineBrain == null)
        {
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
        
        if (cinemachineBrain == null)
            return;
        
        var activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;

        if (activeVirtualCamera != null)
        {
            var noise = activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
            {
                noise = activeVirtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            // Apply the selected Noise Profile from the list based on the index
            if (noiseProfiles != null && noiseProfiles.Count > noiseIndex)
            {
                noise.m_NoiseProfile = noiseProfiles[noiseIndex];
            }

            // Set shake parameters
            noise.m_AmplitudeGain = amplitude;
            noise.m_FrequencyGain = frequency;

            // Start coroutine to handle shake duration and reset
            if (fadeoutCoroutine != null)
                StopCoroutine(fadeoutCoroutine);
            fadeoutCoroutine = StartCoroutine(ResetShake(duration, noise));
        }
    }

    private IEnumerator ResetShake(float duration, CinemachineBasicMultiChannelPerlin noise)
    {
        DOTween.To(() => noise.m_AmplitudeGain, x => noise.m_AmplitudeGain = x, 0, duration);
        yield return new WaitForSeconds(duration);

        // Reset parameters after shake ends
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;

        // Optionally reset Noise Profile to null or to a default state
        // noise.m_NoiseProfile = null;
    }
}
