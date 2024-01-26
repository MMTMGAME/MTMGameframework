using GameFramework;
using GameFramework.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

public static class SoundExtension
{

	public static void PlayMusic(this SoundComponent soundComponent, string assetsName)
	{
		PlaySoundParams playSoundParams = PlaySoundParams.Create();
		playSoundParams.MuteInSoundGroup = false;

		playSoundParams.Priority = 64;
		playSoundParams.Loop = true;
		playSoundParams.VolumeInSoundGroup = 1f;
		playSoundParams.SpatialBlend = 0f;
		GameEntryGet.Sound.PlaySound($"Assets/Audio/{assetsName}.mp3", "Music", playSoundParams);
	}
	public static void PlaySound(this SoundComponent soundComponent, string assetsName)
	{

		PlaySoundParams playSoundParams = PlaySoundParams.Create();
		playSoundParams.Priority = 30;
		playSoundParams.Loop = false;
		playSoundParams.VolumeInSoundGroup = 1f;
		playSoundParams.SpatialBlend = 0f;
		GameEntryGet.Sound.PlaySound($"Assets/Audio/{assetsName}.wav", "SoundEffect", playSoundParams);
	}
	public static void PlayUISound(this SoundComponent soundComponent, string assetsName)
	{

		PlaySoundParams playSoundParams = PlaySoundParams.Create();
		playSoundParams.Priority = 30;
		playSoundParams.Loop = false;
		playSoundParams.VolumeInSoundGroup = 1f;
		playSoundParams.SpatialBlend = 0f;
		GameEntryGet.Sound.PlaySound($"Assets/Audio/{assetsName}.wav", "UISoundEffect", playSoundParams);
	}
	public static void SetVolume(this SoundComponent soundComponent, string soundGroupName, float volume)
	{
		if (string.IsNullOrEmpty(soundGroupName))
		{
			Log.Error("Sound group is invalid.");
			return;
		}

		ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
		if (soundGroup == null)
		{
			Log.Error("Sound group '{0}' is invalid.", soundGroupName);
			return;
		}

		soundGroup.Volume = volume;

	//	Component.Setting.SetFloat(string.Format("Setting.{0}Volume", soundGroupName), volume);

	}
	public static void Mute(this SoundComponent soundComponent, string soundGroupName, bool mute)
	{
		if (string.IsNullOrEmpty(soundGroupName))
		{
			Log.Error("Sound group is invalid.");
			return;
		}

		ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
		if (soundGroup == null)
		{
			Log.Error("Sound group '{0}' is invalid.", soundGroupName);
			return;
		}

		soundGroup.Mute = mute;

	//	Component.Setting.SetBool(string.Format("Setting.{0}Muted", soundGroupName), mute);

	}
	public static bool IsMuted(this SoundComponent soundComponent, string soundGroupName)
	{
		if (string.IsNullOrEmpty(soundGroupName))
		{
			Log.Error("Sound group is invalid.");
			return true;
		}

		ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
		if (soundGroup == null)
		{
			Log.Error("Sound group '{0}' is invalid.", soundGroupName);
			return true;
		}

		return soundGroup.Mute;
	}
	public static float GetVolume(this SoundComponent soundComponent, string soundGroupName)
	{
		if (string.IsNullOrEmpty(soundGroupName))
		{
			Log.Error("Sound group is invalid.");
			return 0f;
		}

		ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
		if (soundGroup == null)
		{
			Log.Error("Sound group '{0}' is invalid.", soundGroupName);
			return 0f;
		}

		return soundGroup.Volume;
	}
}

