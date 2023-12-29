using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class AnimatorEventsEditorSettings : ScriptableObject {
	static AnimatorEventsEditorSettings _Settings;
	public static AnimatorEventsEditorSettings Settings {
		get {
			if (_Settings == null) {
				var guids = AssetDatabase.FindAssets("t:AnimatorEventsEditorSettings");
				if (guids != null && guids.Length > 0) {
					_Settings = AssetDatabase.LoadAssetAtPath<AnimatorEventsEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
				}
				else {
					_Settings = ScriptableObject.CreateInstance<AnimatorEventsEditorSettings>();
					// Set default settings
					_Settings.onlyExecuteIfLayerWeightHigherThan0 = true;
					_Settings.showConsoleErrorIfExecutingEventNotFound = true;
				}
			}
			if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_Settings))) {
				MonoScript ms = MonoScript.FromScriptableObject(_Settings);
				var path = AssetDatabase.GetAssetPath(ms);
				AssetDatabase.CreateAsset(_Settings, Path.GetDirectoryName(path) + "/AnimatorEvents Editor Settings.asset");
				AssetDatabase.SaveAssets();
			}
			return _Settings;
		}
	}


	public bool onlyExecuteIfLayerWeightHigherThan0;
	public bool showConsoleErrorIfExecutingEventNotFound;
}

static class MyCustomSettingsIMGUIRegister {
	[SettingsProvider]
	public static SettingsProvider CreateMyCustomSettingsProvider() {
		var provider = new SettingsProvider("Project/Animator Events Settings", SettingsScope.Project) {
			label = "Animator Events",
			guiHandler = (searchContext) => {
				var settings = new SerializedObject(AnimatorEventsEditorSettings.Settings);
				GUILayout.Space(12);
				EditorGUIUtility.labelWidth = 300;
				GUILayout.Label("Default event settings", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox("This will only apply to new events, existing ones won't be modified.", MessageType.Info);
				EditorGUILayout.PropertyField(settings.FindProperty("onlyExecuteIfLayerWeightHigherThan0"), new GUIContent("Only execute if layer weight higher than 0"));
				EditorGUILayout.PropertyField(settings.FindProperty("showConsoleErrorIfExecutingEventNotFound"), new GUIContent("Throw error when the event can't be found", "Trying to execute an event that can't be found in any of the AnimatorEvent attached to the animator throws an error to the console."));
				settings.ApplyModifiedProperties();
			},

			keywords = new HashSet<string>(new[] { "Animator", "Events" })
		};

		return provider;
	}
}
