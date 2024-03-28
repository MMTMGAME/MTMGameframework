using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityGameFramework.Runtime;

public class CameraShakeComponent : GameFrameworkComponent
{
    
    public Cinemachine.NoiseSettings noiseProfile; // 在 Inspector 中分配你的 Noise Settings

    private CinemachineBrain cinemachineBrain;

    

    public void ShakeCamera(float amplitude, float frequency, float duration)
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

            // 应用选择的 Noise Profile
            noise.m_NoiseProfile = noiseProfile;

            // 设置震动参数
            noise.m_AmplitudeGain = amplitude;
            noise.m_FrequencyGain = frequency;

            // 开始协程来处理震动持续时间并重置
            StartCoroutine(ResetShake(duration, noise));
        }
    }

    private IEnumerator ResetShake(float duration, CinemachineBasicMultiChannelPerlin noise)
    {
        yield return new WaitForSeconds(duration);

        // 震动结束后重置参数
        noise.m_AmplitudeGain = 0;
        noise.m_FrequencyGain = 0;

        // 也可以选择重置 Noise Profile
        // noise.m_NoiseProfile = null;
    }
}