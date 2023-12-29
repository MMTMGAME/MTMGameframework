using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Animations;

namespace Ashkatchap.AnimatorEvents {
	[CustomEditor(typeof(AnimatorEvent))]
	public class AnimatorEventEditor : Editor {
		public class Entry {
			public List<Entry> children = new List<Entry>();
			public string splitName;
			public int? eventIndex;
			public bool isExpanded;
		}

		private SerializedProperty events;
		List<int> sortedEventIndices = new List<int>();
		string filterString = "";
		Entry parent;
		SearchField searchField;
		GUIStyle foldoutStyle;

		private void InitializeIfNeeded() {
			if (searchField != null && sortedEventIndices.Count == ((AnimatorEvent) target).events.Count) return;
			foldoutStyle = new GUIStyle(EditorStyles.foldout);
			foldoutStyle.stretchWidth = true;
			events = serializedObject.FindProperty("events");
			CalculatedSortedIndices();
			searchField = new SearchField();
		}

		private void OnEnable() {
			Undo.undoRedoPerformed += MyUndoCallback;
		}

		private void OnDisable() {
			Undo.undoRedoPerformed -= MyUndoCallback;
		}

		private void MyUndoCallback() {
			serializedObject.Update();
			CalculatedSortedIndices();
		}

		public override void OnInspectorGUI() {
			//DrawDefaultInspector();

			serializedObject.Update();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("useParentAnimators"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("useChildrenAnimators"));

			InitializeIfNeeded();

			filterString = searchField.OnGUI(filterString);

			if (Application.isPlaying) GUI.enabled = false;

			if (GUILayout.Button("Add")) {
				Undo.RecordObject(target, "Added event");

				((AnimatorEvent) target).AddEventForEditor();

				serializedObject.Update();
				CalculatedSortedIndices();
				parent.children[0].isExpanded = true;
			}

			if (filterString.Length != 0) {
				for (int i = 0; i < events.arraySize; i++) {
					var property = events.GetArrayElementAtIndex(sortedEventIndices[i]);
					var eventName = property.FindPropertyRelative("name").stringValue;
					if (!eventName.ToLowerInvariant().Contains(filterString.ToLowerInvariant())) continue;
					DrawEvent(sortedEventIndices[i]);
				}
			}
			else {
				RecursiveDrawingPerform(parent);
			}

			serializedObject.ApplyModifiedProperties();

			if (Application.isPlaying) GUI.enabled = true;
		}

		void RecursiveDrawing(Entry entry) {
			if (entry.isExpanded = EditorGUILayout.Foldout(entry.isExpanded, entry.splitName, true, foldoutStyle)) {
				EditorGUILayout.BeginHorizontal();
				// Not using GUILayout.Space because x position changes. GUILayout.Width(14) fixes the problem.
				GUILayoutUtility.GetRect(14, 0, GUILayout.Width(14));
				EditorGUILayout.BeginVertical();

				RecursiveDrawingPerform(entry);

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
		}

		void RecursiveDrawingPerform(Entry entry) {
			if (entry.eventIndex.HasValue) {
				DrawEvent(entry.eventIndex.Value);
			}

			for (int i = 0; i < entry.children.Count; i++) {
				RecursiveDrawing(entry.children[i]);
			}
		}

		private void DrawEvent(int eventIndex) {
			SerializedProperty property = events.GetArrayElementAtIndex(eventIndex);
			EditorGUILayout.BeginVertical("box");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			var prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 60;
			var propertyName = property.FindPropertyRelative("name");
			EditorGUILayout.DelayedTextField(propertyName);
			bool remove = GUILayout.Button("Remove", GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck()) {
				CalculatedSortedIndices();
				// Expand tree to this element
				var split = propertyName.stringValue.Split('/');
				Entry currentParent = parent;
				for (int j = 0; j < split.Length; j++) {
					currentParent = GetOrCreateChild(currentParent, split[j]);
					currentParent.isExpanded = true;
				}
			}
			EditorGUILayout.PropertyField(property.FindPropertyRelative("action"), false);
			var guiEnabled = GUI.enabled;
			GUI.enabled = true;
			if (GUILayout.Button("Where is it used? (shown in the Console)")) {
				var stateList = StatesUsingEvent(property.FindPropertyRelative("id").intValue);
				Debug.Log(stateList.Count + " states using the event " + propertyName.stringValue);
				foreach (var str in stateList) {
					Debug.Log(str);
				}
			}
			GUI.enabled = guiEnabled;
			EditorGUILayout.EndVertical();

			if (remove) {
				if (!IsEventInUse(property.FindPropertyRelative("id").intValue) || EditorUtility.DisplayDialog("Do you want to remove an event that is being used?", "The event \"" + property.FindPropertyRelative("name").stringValue + "\" is currently in use by the current AnimatorController.\nDo you really want to remove it?", "Remove", "Cancel")) {
					events.DeleteArrayElementAtIndex(eventIndex);
					CalculatedSortedIndices();
					serializedObject.ApplyModifiedProperties();
					GUIUtility.ExitGUI();
				}
			}
			EditorGUIUtility.labelWidth = prevLabelWidth;
		}

		bool IsEventInUse(int eventId) {
			var animator = ((AnimatorEvent) target).GetComponent<Animator>();
			if (animator == null) return false;
			var eventSMBs = animator.GetBehaviours<EventSMB>();
			foreach (var elem in eventSMBs) {
				for (int j = 0; j < elem.entries.Count; j++) {
					for (int i = 0; i < elem.entries[j].actions.Count; i++) {
						if (elem.entries[j].actions[i].type != EventSMB.Action.Type.CallEvent) continue;
						if (elem.entries[j].actions[i].eventId != eventId) continue;
						return true;
					}
				}
			}
			return false;
		}

		List<string> StatesUsingEvent(int eventId) {
			List<string> names = new List<string>();
			var animator = ((AnimatorEvent) target).GetComponent<Animator>();
			if (animator != null) {
				if (animator.runtimeAnimatorController != null) {
					AnimatorController controller = null;
					if (animator.runtimeAnimatorController is AnimatorController ac) {
						controller = ac;
					}
					else if (animator.runtimeAnimatorController is AnimatorOverrideController aoc) {
						controller = aoc.runtimeAnimatorController as AnimatorController;
					}
					if (controller != null) {
						foreach (var l in controller.layers) {
							StatesUsingEventRecursive(eventId, l.stateMachine, names, "");
						}
					}
				}
			}
			return names;
		}

		void StatesUsingEventRecursive(int eventId, AnimatorStateMachine m, List<string> names, string path) {
			path += m.name + "/";
			foreach (var state in m.states) {
				foreach (var b in state.state.behaviours) {
					if (b is EventSMB ev) {
						foreach (var entry in ev.entries) {
							foreach (var action in entry.actions) {
								if (action.type == EventSMB.Action.Type.CallEvent && action.eventId == eventId) {
									names.Add(path + state.state.name);
									goto NEXT_STATE; // this state contains the event, continue searching other states. If we wanted every single entry, remove this.
								}
							}
						}
					}
				}
				NEXT_STATE:
				continue;
			}

			foreach (var sm in m.stateMachines) {
				StatesUsingEventRecursive(eventId, sm.stateMachine, names, path);
			}
		}

		void CalculatedSortedIndices() {
			sortedEventIndices.Clear();
			events = serializedObject.FindProperty("events");
			for (int i = 0; i < events.arraySize; i++) {
				var eventsI = events.GetArrayElementAtIndex(i);
				if (eventsI.FindPropertyRelative("id").intValue == 0) {
					eventsI.FindPropertyRelative("id").intValue = Animator.StringToHash(eventsI.FindPropertyRelative("name").stringValue);
				}
				sortedEventIndices.Add(i);
			}
			sortedEventIndices.Sort((a, b) => events.GetArrayElementAtIndex(a).FindPropertyRelative("name").stringValue.CompareTo(events.GetArrayElementAtIndex(b).FindPropertyRelative("name").stringValue));

			var oldParent = parent;
			parent = new Entry();
			for (int i = 0; i < events.arraySize; i++) {
				int eventId = sortedEventIndices[i];
				string propertyName = events.GetArrayElementAtIndex(eventId).FindPropertyRelative("name").stringValue;
				var split = propertyName.Split('/');

				Entry currentParent = parent;
				for (int j = 0; j < split.Length - 1; j++) {
					currentParent = GetOrCreateChild(currentParent, split[j]);
				}

				currentParent.children.Add(new Entry() { eventIndex = eventId, splitName = split[split.Length - 1] });
			}

			if (oldParent != null) {
				MatchParents(oldParent, parent);
			}
		}

		void MatchParents(Entry source, Entry dest) {
			dest.isExpanded = source.isExpanded;
			for (int i = 0; i < source.children.Count; i++) {
				for (int j = 0; j < dest.children.Count; j++) {
					if (source.children[i].splitName == dest.children[j].splitName) {
						MatchParents(source.children[i], dest.children[j]);
					}
				}
			}
		}

		Entry GetOrCreateChild(Entry parent, string name) {
			for (int i = 0; i < parent.children.Count; i++) {
				if (parent.children[i].splitName == name) {
					return parent.children[i];
				}
			}
			Entry c = new Entry() { splitName = name };
			parent.children.Add(c);
			return c;
		}
	}
}
