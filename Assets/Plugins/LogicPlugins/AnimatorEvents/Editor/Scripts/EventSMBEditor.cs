using Ashkatchap.AnimatorEvents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Ashkatchap.AnimatorEventsEditor {
	[CustomEditor(typeof(EventSMB))]
	public class EventSMBEditor : Editor {
		static readonly char[] options = new char[] { '0', '1', '2' };
		static readonly GUIContent[] names = new GUIContent[] { new GUIContent("Never"), new GUIContent("On Exit Start"), new GUIContent("On Exit End") };
		static readonly List<EventSMB.Entry> copiedEntries = new List<EventSMB.Entry>();
		static readonly AnimatorControllerParameter[] EmptyParameterArray = new AnimatorControllerParameter[0];
		static GUIContent c2 = new GUIContent("On All Loops", "Execute the actions every time the state loops. Otherwise only the first loop will execute them.");
		static GUIContent c1 = new GUIContent("At Least", "Execute the actions on the specified state exit if they haven't already.");
		static GUIContent c0 = new GUIContent("During Exit Transition", "Allow executing actions during an exit transition."); // The logic here is reversed compared to EventSMB.Condition.Prepared to use less words in the GUI
		static GUIContent discardCopies = new GUIContent("Discard copied entries", "Discard all copied entries.");

		static readonly Dictionary<string, int> PreparedByName = ((Func<Dictionary<string, int>>) (() => {
			var dict = new Dictionary<string, int>();

			for (int i = 0; i < EventSMB.Condition.Prepared.Length; i++) {
				var plainName = EventSMB.Condition.Prepared[i].Key;
				int subStrIndex = plainName.IndexOf('|');
				if (subStrIndex != -1) {
					plainName = plainName.Substring(0, subStrIndex);
				}

				if (!dict.ContainsKey(plainName)) {
					dict.Add(plainName, i);
				}
			}

			return dict;
		}))();


		string[] eventsAvailableNames = new string[0];
		readonly List<CollapsableList> lists = new List<CollapsableList>();
		readonly List<int> eventsAvailable = new List<int>();
		readonly List<AnimatorEvent> matchingAnimatorEvent = new List<AnimatorEvent>();
		readonly Dictionary<AnimatorEvent, SerializedObject> animatorEventSO = new Dictionary<AnimatorEvent, SerializedObject>();
		AnimatorController controller;
		SimpleReorderableList list;
		GUIStyle plain;
		GUIStyle styleHint;
		GUIStyle styleConditionExplained;
		GUIStyle centeredGreyMiniLabel;
		SerializedProperty entryColor;
		EventSMB targetCasted;


		void InitializeIfNeeded() {
			CreateParameterInfoList();
			if (controller != null) return;

			Animator ignore;
			ScrubAnimatorUtil.GetCurrentAnimatorAndController(out controller, out ignore);

			targetCasted = (EventSMB) target;
			styleHint = new GUIStyle(GUI.skin.label);
			styleConditionExplained = new GUIStyle(GUI.skin.label);
			styleConditionExplained.richText = true;
			styleConditionExplained.stretchWidth = true;
			styleConditionExplained.wordWrap = true;
			styleConditionExplained.alignment = TextAnchor.MiddleCenter;
			styleConditionExplained.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : Color.gray;
			var c = styleHint.normal.textColor;
			c.a = 0.5f;
			styleHint.normal.textColor = c;

			plain = new GUIStyle();
			plain.normal.background = new Texture2D(1, 1);
			plain.normal.background.SetPixel(0, 0, Color.white);
			plain.normal.background.Apply();

			centeredGreyMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
			centeredGreyMiniLabel.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : Color.gray;

			list = new SimpleReorderableList();
			list.entries = serializedObject.FindProperty("entries");
			list.draw = DrawEntry;
			list.getEntryColor = GetEntryColor;
			list.initAddedEntry = (entry) => {
				entry.FindPropertyRelative("preparedConditionsIndex").intValue = 5;
				entry.FindPropertyRelative("conditions").ClearArray();
				entry.FindPropertyRelative("actions").ClearArray();
			};

			UpdateMatchingAnimatorEventList();
		}

		void UpdateMatchingAnimatorEventList() {
			AnimatorEvent[] animatorEvents;
#if !UNITY_2018_3_OR_NEWER
			animatorEvents = FindObjectsOfType<AnimatorEvent>();
#else
#if UNITY_2021_1_OR_NEWER
			var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
			var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
			animatorEvents = prefabStage != null ?
				prefabStage.stageHandle.FindComponentsOfType<AnimatorEvent>() :
				FindObjectsOfType<AnimatorEvent>();
#endif

			matchingAnimatorEvent.Clear();
			var eventNames = new List<string>();
			var eventNames_Inline = new List<string>();
			var eventNames_NewInlines = new List<string>();
			var eventsAvailable_Inline = new List<int>();
			eventsAvailable.Clear();
			int j = 1;
			foreach (var ae in animatorEvents) {
				var runtimeController = ae.GetComponent<Animator>().runtimeAnimatorController;
				var overrided = runtimeController as AnimatorOverrideController;
				if (runtimeController == controller || (overrided != null && overrided.runtimeAnimatorController == controller)) {
					matchingAnimatorEvent.Add(ae);

					List<int> sortedEventIndices = new List<int>();
					for (int i = 0; i < ae.events.Count; i++) {
						sortedEventIndices.Add(i);
					}
					sortedEventIndices.Sort((a, b) => ae.events[a].name.CompareTo(ae.events[b].name));

					foreach (var i in sortedEventIndices) {
						var ev = ae.events[i];
						if (eventsAvailable.Contains(ev.id)) continue;

						bool isInline = ev.name.StartsWith(InlineStart);
						(isInline ? eventsAvailable_Inline : eventsAvailable).Add(ev.id);
						(isInline ? eventNames_Inline : eventNames).Add(ev.name);
					}

					eventNames_NewInlines.Add("New inline event/In AnimatorEvent " + j + " (Name: " + ae.name + ")");
					j++;
				}
			}
			eventNames.AddRange(eventNames_Inline);
			eventsAvailable.AddRange(eventsAvailable_Inline);
			eventNames.AddRange(eventNames_NewInlines);
			eventsAvailableNames = eventNames.ToArray();
		}

		/// <returns>Simple UI was used</returns>
		bool TryDrawSimpleUI(int index, SerializedProperty entry) {
			var conditions = entry.FindPropertyRelative("conditions");
			var preparedConditionsIndex = entry.FindPropertyRelative("preparedConditionsIndex");
			var contents = new List<string>();

			string currentName = "Advanced";
			int currentLocalIndex = -1;

			for (int i = 0; i < EventSMB.Condition.Prepared.Length; i++) {
				var plainName = EventSMB.Condition.Prepared[i].Key;
				int subStrIndex = plainName.IndexOf('|');
				if (subStrIndex != -1) {
					plainName = plainName.Substring(0, subStrIndex);
				}

				if (!contents.Contains(plainName)) {
					contents.Add(plainName);
				}
				if (i == preparedConditionsIndex.intValue) {
					currentName = plainName;
					currentLocalIndex = contents.IndexOf(plainName);
				}
			}
			if (currentLocalIndex == -1) currentLocalIndex = contents.Count;
			contents.Add("Advanced");

			if (preparedConditionsIndex.intValue != -1) {
				var conditionsCached = EventSMB.Condition.Prepared[preparedConditionsIndex.intValue].Value;
				if (!CheckIfSame(conditions, conditionsCached)) {
					SetConditions(entry.FindPropertyRelative("conditions"), conditionsCached);
				}
			}

			if (DrawPopup(index, entry, currentLocalIndex, contents.ToArray())) {
				SetLabelColors();

				if (preparedConditionsIndex.intValue != -1 && currentName.StartsWith("On Normalized Time Reached")) {
					string previousModeStr = EventSMB.Condition.Prepared[preparedConditionsIndex.intValue].Key;
					previousModeStr = previousModeStr.Substring(previousModeStr.IndexOf('|') + 1);
					var previousMode = previousModeStr.ToCharArray();
					var currentMode = new char[previousMode.Length];
					previousMode.CopyTo(currentMode, 0);

					SerializedProperty time = GetFirstValueProperty(entry);
					if (time == null) {
						preparedConditionsIndex.intValue = -1;
						return false;
					}

					Rect rect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
					ScrubAnimatorUtil.DrawScrub(rect, targetCasted, time, true, m_ParameterInfoList);

					GUILayout.BeginHorizontal();
					var prevLabelWidth = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(c2).x;
					currentMode[0] = EditorGUILayout.Toggle(c2, currentMode[0] == '1') ? '1' : '0';

					EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(c1).x;
					var popupMinWidth = EditorGUIUtility.labelWidth + EditorStyles.popup.CalcSize(new GUIContent(names[1])).x;
					currentMode[1] = options[EditorGUILayout.Popup(c1, Array.IndexOf(options, currentMode[1]), names, GUILayout.MinWidth(popupMinWidth))];

					EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(c0).x;
					currentMode[2] = EditorGUILayout.Toggle(c0, currentMode[2] == '0') ? '0' : '1';

					EditorGUIUtility.labelWidth = prevLabelWidth;
					GUILayout.EndHorizontal();

					if (!previousMode.SequenceEqual(currentMode)) {
						// Except for the normalized time
						float prevTime = time.floatValue;
						string nameToSearch = currentName + "|";
						for (int i = 0; i < currentMode.Length; i++) nameToSearch += currentMode[i];
						for (int i = 0; i < EventSMB.Condition.Prepared.Length; i++) {
							if (EventSMB.Condition.Prepared[i].Key == nameToSearch) {
								SetConditions(entry.FindPropertyRelative("conditions"), EventSMB.Condition.Prepared[i].Value);
								preparedConditionsIndex.intValue = i;
								time = GetFirstValueProperty(entry);
								time.floatValue = prevTime;
								break;
							}
						}
					}
				}

				RevertLabelColors();
			}

			return preparedConditionsIndex.intValue != -1;
		}

		void DrawBlendTreePreviewParams() {
			var contexts = AnimatorController.FindStateMachineBehaviourContext(targetCasted);
			foreach (var context in contexts) {
				AnimatorState state = context.animatorObject as AnimatorState;
				if (state == null) continue;

				BlendTree tree = state.motion as BlendTree;
				if (tree != null) {
					InitBlendTreeSupport(tree);

					GUILayout.BeginVertical(GUI.skin.box);
					GUILayout.Label("BlendTree Preview parameters", EditorStyles.miniLabel);
					for (int i = 0; i < m_ParameterInfoList.Count; i++) {
						var item = m_ParameterInfoList[i];
						if (item.p.type != AnimatorControllerParameterType.Float) continue;
						if (!item.show) continue;
						m_ParameterInfoList[i].previewValue = EditorGUILayout.Slider(item.p.name, item.previewValue, item.minMax.x, item.minMax.y);
					}
					GUILayout.EndVertical();
				}
			}
		}

		SerializedProperty GetFirstValueProperty(SerializedProperty entry) {
			var conditions = entry.FindPropertyRelative("conditions");
			for (int i = 0; i < conditions.arraySize; i++) {
				var value = conditions.GetArrayElementAtIndex(i);
				EventSMB.Condition.Type t = (EventSMB.Condition.Type) value.FindPropertyRelative("type").intValue;

				if (t == EventSMB.Condition.Type.OnNormalizedTimePerLoop || t == EventSMB.Condition.Type.OnNormalizedTime) {
					return value.FindPropertyRelative("value");
				}
			}
			return null;
		}

		static bool CheckIfSame(SerializedProperty conditions, EventSMB.Condition[] comparison) {
			if (conditions.arraySize != comparison.Length) return false;
			for (int k = 0; k < conditions.arraySize; k++) {
				var cond = conditions.GetArrayElementAtIndex(k);
				if ((int) comparison[k].type != cond.FindPropertyRelative("type").intValue) {
					return false;
				}
				if (comparison[k].value != cond.FindPropertyRelative("value").floatValue && !HasIgnoreValueAttribute(comparison[k].type)) {
					return false;
				}
			}
			return true;
		}

		Rect popupRect;
		/// <returns>True if nothing changed</returns>
		bool DrawPopup(int entryIndex, SerializedProperty entry, int index, string[] contents) {
			GUILayout.BeginHorizontal();
			int newIndex = EditorGUILayout.Popup(index, contents);
			popupRect = GUILayoutUtility.GetLastRect();
			var r = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(40), GUILayout.Height(20));
			r.y += 2;
			r.height = 15;
			if (GUI.Button(r, new GUIContent("Copy", "Copy this entry to a temporary list of copied entries."), EditorStyles.miniButton)) {
				copiedEntries.Add(targetCasted.entries[entryIndex].Clone());
			}
			if (entryColor.colorValue == default(Color)) entryColor.colorValue = EditorGUIUtility.isProSkin ? new Color(0.11f, 0.45f, 0.38f) : new Color(0.5f, 0.83f, 0.76f, 1);
			entryColor.colorValue = EditorGUILayout.ColorField(GUIContent.none, entryColor.colorValue, true, false, false,
#if UNITY_2017 || UNITY_5
				new ColorPickerHDRConfig(0, 1, 0, 1),
#endif
			GUILayout.Width(30));
			GUILayout.EndHorizontal();
			if (newIndex != index) {
				var preparedConditionsIndex = entry.FindPropertyRelative("preparedConditionsIndex");
				if (newIndex == contents.Length - 1) {
					preparedConditionsIndex.intValue = -1;
				}
				else {
					preparedConditionsIndex.intValue = PreparedByName[contents[newIndex]];
					SetConditions(entry.FindPropertyRelative("conditions"), EventSMB.Condition.Prepared[preparedConditionsIndex.intValue].Value);
				}
				return false;
			}
			return true;
		}

		List<ParameterInfo> m_ParameterInfoList = new List<ParameterInfo>();
		void CreateParameterInfoList() {
			if (controller == null) return;
			var ps = controller.parameters;
			if (ps != null) {
				foreach (var p in ps) {
					if (!m_ParameterInfoList.Exists((e) => e.p.nameHash == p.nameHash)) {
						m_ParameterInfoList.Add(new ParameterInfo() { p = p });
					}
				}
			}
		}
		void InitBlendTreeSupport(BlendTree tree) {
			CreateParameterInfoList();
			if (controller == null) return;
			for (int j = 0; j < tree.recursiveBlendParameterCount(); j++) {
				int num = controller.IndexOfParameter(tree.GetRecursiveBlendParameter(j));
				bool show = num != -1;
				m_ParameterInfoList[num].show = show;
				if (!show) continue;
				m_ParameterInfoList[num].minMax = new Vector2(Mathf.Min(tree.GetRecursiveBlendParameterMin(j), m_ParameterInfoList[num].minMax.x), Mathf.Max(tree.GetRecursiveBlendParameterMax(j), m_ParameterInfoList[num].minMax.y));
			}
		}

		void SetConditions(SerializedProperty conditions, EventSMB.Condition[] toCopy) {
			conditions.ClearArray();
			for (int i = 0; i < toCopy.Length; i++) {
				conditions.InsertArrayElementAtIndex(i);
				var elem = conditions.GetArrayElementAtIndex(i);
				elem.FindPropertyRelative("type").intValue = (int) toCopy[i].type;
				if (!HasIgnoreValueAttribute(toCopy[i].type)) {
					elem.FindPropertyRelative("value").floatValue = (int) toCopy[i].value;
				}
			}
			serializedObject.ApplyModifiedProperties();
		}

		static bool HasIgnoreValueAttribute(EventSMB.Condition.Type conditionType) {
			return Attribute.IsDefined(typeof(EventSMB.Condition.Type).GetField(conditionType.ToString()), typeof(EventSMB.Condition.IgnoreValueAttribute));
		}

		public override bool RequiresConstantRepaint() {
			return EditorApplication.isPlaying;
		}

		public override void OnInspectorGUI() {
			//base.OnInspectorGUI();
			//serializedObject.ShowScriptInput();

			InitializeIfNeeded();

			if (Application.isPlaying) EditorGUILayout.HelpBox("Changes made during play mode won't be saved! Write them down somewhere before stopping.", MessageType.Warning);
			else {
				DrawBlendTreePreviewParams();
			}

			serializedObject.Update();

			var controllerOkIfFound = serializedObject.FindProperty("controllerOkIfFound");
			EditorGUILayout.PropertyField(controllerOkIfFound, new GUIContent("Animator Controller", "In the case of not choosing \"Any\", Events will only be executed if the current AnimatorController or AnimatorControllerOverride that's playing appears or not in the list."));
			if (controllerOkIfFound.intValue != 0) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty("controllers"), true);
			}


			if (list != null) {
				list.DoLayoutList();
			}
			else {
				GUILayout.Label("List not available. Try clicking the object that has the animator.");
			}

			if (GUILayout.Button("Copy all")) {
				foreach (var entry in targetCasted.entries) {
					copiedEntries.Add(entry.Clone());
				}
			}

			if (copiedEntries.Count > 0) {
				GUILayout.BeginHorizontal();

				if (GUILayout.Button(new GUIContent("Paste (" + copiedEntries.Count + ")", "Add the copied entries here."), EditorStyles.miniButton)) {
					foreach (var elem in copiedEntries) {
						targetCasted.entries.Add(elem.Clone());
					}
					serializedObject.Update();
					serializedObject.ApplyModifiedProperties();
				}
				if (GUILayout.Button(discardCopies, EditorStyles.miniButton)) {
					copiedEntries.Clear();
				}
				GUILayout.EndHorizontal();
			}

			serializedObject.ApplyModifiedProperties();

			if (Application.isPlaying) GUI.enabled = true;
		}

		void DrawEntry(SerializedProperty entry, int index) {
			entryColor = entry.FindPropertyRelative("color");
			currentIsDark = IsDark(entryColor.colorValue);

			EventSMB.Entry entryObj = null;
#if DEBUG
			entryObj = (EventSMB.Entry) ConditionsSaver.GetTargetObjectOfProperty(entry);
#endif

			CollapsableList c;
			int arraySize;
			var parameters = controller != null ? controller.parameters : EmptyParameterArray;
			if (!TryDrawSimpleUI(index, entry)) {
				// If condition is known, show dropdown and custom simple gui
				ShowConditions(entry.FindPropertyRelative("conditions"), targetCasted.entries[index].conditions.ToStringFull(parameters, EditorGUIUtility.isProSkin), entryObj, 1);
			}

			ShowConditions(entry.FindPropertyRelative("conditionsExtra"), targetCasted.entries[index].conditionsExtra.ToStringFull(parameters, EditorGUIUtility.isProSkin), entryObj, 2);

			c = GetOrCreateCollapsableList(lists, entry.FindPropertyRelative("actions"), PrepareReorderableActions, out arraySize);
#if DEBUG
			c.list.drawHeaderCallback = (Rect r) => {
				if (entryObj != null && entryObj.debugLastCheckWasTrue != null) {
					ShowDebugBox(ref r, entryObj.debugLastCheckWasTrue, 2);
				}
				GUI.Label(r, "Actions", EditorStyles.boldLabel);
			};
#endif
			c.list.DoLayoutList();
		}

		static List<EventSMB.Condition> GetConditions(SerializedProperty conditions) {
			return ConditionsSaver.GetTargetObjectOfProperty(conditions) as List<EventSMB.Condition>;
		}
		static readonly GUIContent Cond_Copy = new GUIContent("Copy", "Copy all the conditions");
		static readonly GUIContent Cond_Paste = new GUIContent("Paste", "Paste all the copied conditions");
		static readonly GUIContent Cond_Discard = new GUIContent("Discard", "Discard the copied conditions");
		static readonly GUIContent Cond_Save = new GUIContent("Save", "Save this list of conditions to an asset. The character semicolon `;` is interpreted as the character `/` to categorize it inside a sub-dropdowns in the the Load dropdown, as if it was a path with folders.");
		static readonly GUIContent Cond_Load = new GUIContent("Load", "Load an asset with a list of conditions");
		void ShowConditions(SerializedProperty conditions, string description, EventSMB.Entry entryObj, int debugIndex) {
			int arraySize;
			var c = GetOrCreateCollapsableList(lists, conditions, PrepareReorderableAdvancedConditions, out arraySize);

			bool asBox = conditions.arraySize > 0 || c.show;
			if (debugIndex == 2) {
				Rect buttonConditionsExtra = popupRect;
				GUIContent bCondExtraSize = new GUIContent(c.show ? "-" : "+", "Show/hide Extra Conditions (Both the normal condition and the extra condition must be true for the events to be fired.");
				buttonConditionsExtra.size = EditorStyles.miniButton.CalcSize(bCondExtraSize);
				buttonConditionsExtra.x -= buttonConditionsExtra.width + 1;
				c.show = GUI.Toggle(buttonConditionsExtra, c.show, bCondExtraSize, EditorStyles.miniButton);
			}
			else {
				asBox = true;
			}

			if (asBox) {
				GUILayout.BeginVertical(GUI.skin.box);

				c.show = GUILayout.Toggle(c.show, description, styleConditionExplained);
#if DEBUG
				if (entryObj != null) {
					Rect r = GUILayoutUtility.GetLastRect();
					ShowDebugBox(ref r, entryObj.debugLastCheckWasTrue, debugIndex);
				}
#endif

				if (c.show) {
					GUILayout.Label(conditions.displayName, centeredGreyMiniLabel);
					Rect r = GUILayoutUtility.GetLastRect();

					r.xMin += r.width - 120;
					if (copiedConditions == null) {
						r.width *= 0.3333333f;
						if (GUI.Button(r, Cond_Copy, EditorStyles.miniButton)) {
							copiedConditions = new List<EventSMB.Condition>(GetConditions(conditions));
						}
						r.x += r.width;
						if (GUI.Button(r, Cond_Save, EditorStyles.miniButton)) {
							ConditionsSaver.Save(GetConditions(conditions));
						}
						r.x += r.width;
						if (GUI.Button(r, Cond_Load, EditorStyles.miniButton)) {
							ConditionsSaver.Load(targetCasted, GetConditions(conditions));
						}
					}
					else {
						r.width *= 0.5f;
						if (GUI.Button(r, Cond_Paste, EditorStyles.miniButton)) {
							var conditionArray = GetConditions(conditions);
							Undo.RecordObject(targetCasted, "Pasted conditions");
							conditionArray.Clear();
							conditionArray.AddRange(copiedConditions);
						}
						r.x += r.width;
						if (GUI.Button(r, Cond_Discard, EditorStyles.miniButton)) {
							copiedConditions = null;
						}
					}

					c.list.DoLayoutList();
				}

				GUILayout.EndVertical();
			}
		}

		static List<EventSMB.Condition> copiedConditions;

		CollapsableList GetOrCreateCollapsableList(List<CollapsableList> lists, SerializedProperty arraySP, Func<SerializedProperty, UnityEditorInternal.ReorderableList> ReorderableCreator, out int arraySize) {
			CollapsableList reorderable = null;
			arraySize = arraySP.arraySize;
			for (int i = 0; i < lists.Count; i++) {
				if (SerializedProperty.EqualContents(lists[i].list.serializedProperty, arraySP)) {
					reorderable = lists[i];
					break;
				}
			}
			if (reorderable == null) {
				lists.Add(reorderable = new CollapsableList() {
					list = ReorderableCreator(arraySP),
					show = /*arraySize > 0*/ false
				});
			}

			return reorderable;
		}

		Color GetEntryColor(SerializedProperty entry, int index) {
			return entry.FindPropertyRelative("color").colorValue;
		}

		const string InlineStart = "Inline/";

		UnityEditorInternal.ReorderableList PrepareReorderableActions(SerializedProperty actions) {
			return CreateReorderableList(actions,
				new GUIContent("Actions", "List of actions to execute, ordered from top to bottom."),
				(index) => {
					if (index < 0 || index >= actions.arraySize) return 20; // Since Unity 2021, when arraySize is 0 this gets called for index 0
					var property = actions.GetArrayElementAtIndex(index);
					var type = property.FindPropertyRelative("type");
					var mode = property.FindPropertyRelative("mode");

					switch ((EventSMB.Action.Type) type.intValue) {
						case EventSMB.Action.Type.CallEvent: {
							if (matchingAnimatorEvent == null || matchingAnimatorEvent.Count == 0 || matchingAnimatorEvent[0] == null) {
								goto default;
							}
							if ((mode.intValue & 0x1) == 1) {
								goto default;
							}
							var eventId = property.FindPropertyRelative("eventId");
							foreach (var ae in matchingAnimatorEvent) {
								if (ae == null) continue;
								foreach (var ev in ae.events) {
									if (ev == null) continue;
									if (ev.id == eventId.intValue) {
										if (ev.name.StartsWith(InlineStart)) {
											return 40 + 40 + 24 + 47 * Mathf.Max(1, ev.action.GetPersistentEventCount());
										}
										break;
									}
								}
							}
							goto default;
						}
						default:
							return 20;
					}
				},
				(rect, index, isActive, isFocused) => {
					var prevLabelWidth = EditorGUIUtility.labelWidth;
					var previousColor = GUI.color;

					var property = actions.GetArrayElementAtIndex(index);
					var type = property.FindPropertyRelative("type");
					var mode = property.FindPropertyRelative("mode");

#if DEBUG
					var boxed = ConditionsSaver.GetTargetObjectOfProperty(property);
					if (boxed != null) {
						var actionObj = (EventSMB.Action) boxed;
						ShowDebugBox(ref rect, actionObj.debugLastCheckWasTrue);
					}
#endif

					Rect r = rect;
					r.height = 18;
					var typeGuiContent = GetPrintName((EventSMB.Action.Type) type.intValue);
					r.width = EditorStyles.boldLabel.CalcSize(typeGuiContent).x;
					GUI.Label(r, typeGuiContent, EditorStyles.boldLabel);

					r = new Rect(r.x + r.width + 2, rect.y, rect.width - r.width - 2, 18);

					int eButtonSize = 2;
					Rect r3 = r;
					r3.xMin = r.xMax - eButtonSize + 1;
					r3.width = eButtonSize;
					r.xMax -= r3.width;





					Event current = Event.current;
					bool showRightClickMenu = current.type == EventType.MouseDown && current.button == 1 && r.Contains(current.mousePosition);
					GenericMenu gm = null;
					if (showRightClickMenu) {
						gm = new GenericMenu();
					}





					var prevColor = GUI.color;

					switch ((EventSMB.Action.Type) type.intValue) {
						case EventSMB.Action.Type.CallEvent: {
							r = new Rect(r.x, rect.y, r.width - 2 - eButtonSize, 18);
							if (matchingAnimatorEvent == null || matchingAnimatorEvent.Count == 0 || matchingAnimatorEvent[0] == null) {
								EditorGUI.HelpBox(r, "Select a GameObject with an AnimatorEvent component", MessageType.Info);
								break;
							}
							if ((mode.intValue & 0x1) == 1) {
								EditorGUI.HelpBox(r, "Callback managed by a script", MessageType.Info);
								break;
							}

							var eventId = property.FindPropertyRelative("eventId");
							var eventName = property.FindPropertyRelative("functionName");
							AnimatorEvent ae = null;
							AnimatorEvent.EventElement ev = null;
							foreach (var m in matchingAnimatorEvent) {
								foreach (var elem in m.events) {
									if (elem.id == eventId.intValue || elem.name == eventName.stringValue) {
										eventId.intValue = elem.id;
										eventName.stringValue = elem.name;
										ae = m;
										ev = elem;
										goto END;
									}
								}
							}

							END:
							if (ev == null) GUI.color = Color.red;

							int indexOld = eventsAvailable.IndexOf(eventId.intValue);
							int indexNew = EditorGUI.Popup(r, indexOld, eventsAvailableNames);

							if (showRightClickMenu) {
								gm.AddItem(new GUIContent("Show an error on the console when trying to execute this event if it's not on any AnimatorEvent (this is the default)."), (mode.intValue & 0x2) == 0, () => {
									mode.intValue ^= 0x2;
									mode.serializedObject.ApplyModifiedProperties();
									Repaint();
								});
							}

							if (indexNew >= eventsAvailable.Count) {
								var animEv = matchingAnimatorEvent[indexNew - eventsAvailable.Count];
								Undo.RecordObject(animEv, "Create new inline event");
								ev = animEv.AddEventForEditor();
								ev.name = InlineStart + RandomString(12);
								ev.action = new UnityEngine.Events.UnityEvent();
								eventId.intValue = ev.id;
								UpdateMatchingAnimatorEventList();
							}
							else if (indexNew == -1) {
								eventId.intValue = 0;
							}
							else {
								eventId.intValue = eventsAvailable[indexNew];
							}

							GUI.color = Color.white;
							if (ev == null) GUI.Label(r, "EVENT NOT FOUND (" + property.FindPropertyRelative("functionName").stringValue + ")", EditorStyles.whiteLabel);
							else property.FindPropertyRelative("functionName").stringValue = eventsAvailableNames[eventsAvailable.IndexOf(eventId.intValue)];

							var r2 = new Rect(r.x + r.width + 2, rect.y, eButtonSize, 18);
							{
								bool tValue = (mode.intValue & 0x2) != 0;
								GUI.color = tValue ? Color.red : EditorGUIUtility.isProSkin ? Color.black : Color.white;
								GUI.Box(r2, "", plain);
							}
							GUI.color = prevColor;

							if (ev != null && ae != null && ev.name.StartsWith(InlineStart)) {
								SerializedObject so;
								if (!animatorEventSO.TryGetValue(ae, out so)) {
									so = new SerializedObject(ae);
									animatorEventSO[ae] = so;
								}
								so.Update();
								var events = so.FindProperty("events");
								var evIndex = ae.events.IndexOf(ev);

								SerializedProperty evProperty = events.GetArrayElementAtIndex(evIndex);



								EditorGUIUtility.labelWidth = 60;
								var propertyName = evProperty.FindPropertyRelative("name");
								string name = propertyName.stringValue.Substring(InlineStart.Length);
								Rect rName = rect;
								rName.y += 20;
								rName.height = 18;
								name = EditorGUI.DelayedTextField(rName, "Name", name);
								propertyName.stringValue = InlineStart + name;
								EditorGUIUtility.labelWidth = prevLabelWidth;

								Rect rEvents = rect;
								rEvents.y += 40;
								rEvents.height = 40 + 43 * Mathf.Max(1, ev.action.GetPersistentEventCount());
								EditorGUI.PropertyField(rEvents, evProperty.FindPropertyRelative("action"), false);

								so.ApplyModifiedProperties();
							}
							break;
						}
						case EventSMB.Action.Type.SendMessage: {
							var value = property.FindPropertyRelative("value");
							var functionName = property.FindPropertyRelative("functionName");

							var r1 = r;
							var r2 = r;
							r1.width *= 0.75f;
							r2.width -= r1.width + 2;
							r2.x += r1.width + 2;
							EditorGUI.PropertyField(r1, functionName, GUIContent.none);
							string placeholder = functionName.stringValue == "" ? "Function name.." : "";
							EditorGUI.LabelField(r1, new GUIContent(placeholder, "Name of the function that will be called."), styleHint);

							string input = EditorGUI.TextField(r2, (mode.intValue & 0x1) == 0 ? "" : value.floatValue.ToString(CultureInfo.InvariantCulture));
							float result;
							mode.intValue &= 0x80;
							if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
								mode.intValue |= 1;
								value.floatValue = result;
							}
							else {
								value.floatValue = 0;
							}
							placeholder = (mode.intValue & 0x1) == 0 ? "Value..." : "";
							EditorGUI.LabelField(r2, new GUIContent(placeholder, "Number (float) to send as a paratemer when calling SendMessage. If none, SendMessage will be called without extra arguments."), styleHint);
							break;
						}
						case EventSMB.Action.Type.SetParameter: {
							var parameterHash = property.FindPropertyRelative("parameterHash");
							var parameters = controller != null ? controller.parameters : EmptyParameterArray;
							GUIContent[] parametersStr = new GUIContent[parameters.Length];

							int paramIndex = -1;

							for (int i = 0; i < parameters.Length; i++) {
								parametersStr[i] = new GUIContent(parameters[i].name);
								if (parameters[i].nameHash == parameterHash.intValue) paramIndex = i;
							}

							if (paramIndex == -1) GUI.color = Color.red;
							Rect r1 = r;
							r1.width *= 0.5f;
							r1.y += 1;
							int selectedIndex = EditorGUI.Popup(r1, GUIContent.none, paramIndex, parametersStr);
							if (paramIndex != selectedIndex) {
								paramIndex = selectedIndex;
								parameterHash.intValue = parameters[selectedIndex].nameHash;
							}
							GUI.color = previousColor;

							if (paramIndex != -1) {
								var elem = parameters[paramIndex];

								Rect r2 = r;
								r2.x += r1.width + 2;
								r2.width -= r1.width + 2;

								SerializedProperty value = property.FindPropertyRelative("value");

								if (elem.type == AnimatorControllerParameterType.Bool || elem.type == AnimatorControllerParameterType.Trigger) {
									var checkboxWidth = GUI.skin.toggle.CalcSize(GUIContent.none).x;
									r2.x += r2.width - checkboxWidth;
									r2.width = checkboxWidth;
								}

								switch (elem.type) {
									case AnimatorControllerParameterType.Bool:
									case AnimatorControllerParameterType.Trigger:
										mode.intValue = (mode.intValue & 0x80); // Set to replace
										value.floatValue = EditorGUI.Toggle(r2, GUIContent.none, value.floatValue == 1) ? 1 : 0;
										break;
									case AnimatorControllerParameterType.Int:
									case AnimatorControllerParameterType.Float:
										Rect r21 = r2;
										Rect r22 = r2;
										var bb = new GUIStyle(EditorStyles.miniButtonLeft);
										bb.fontSize += 15;
										bb.padding = new RectOffset(3, 1, 0, 2);
										bb.margin = new RectOffset();
										bb.alignment = TextAnchor.MiddleCenter;
										r21.width = bb.CalcSize(new GUIContent(SetParamModeStr[mode.intValue & 0x7])).x;
										r22.x += r21.width;
										r22.width -= r21.width;
										mode.intValue = (mode.intValue & 0x80) | EditorGUI.Popup(r21, mode.intValue & 0x7, SetParamModeStr, bb);

										string description = "Operation " + descriptions[mode.intValue & 0x7] + ".";

										EditorGUI.LabelField(r21, new GUIContent("", description), styleHint);
										value.floatValue = elem.type == AnimatorControllerParameterType.Int ?
											EditorGUI.IntField(r22, GUIContent.none, (int) value.floatValue) :
											EditorGUI.FloatField(r22, GUIContent.none, value.floatValue);
										break;
								}
							}
							break;
						}
					}

					{
						bool tValue = (mode.intValue & 0x80) != 0;
						GUI.color = tValue ? EditorGUIUtility.isProSkin ? Color.cyan : Color.blue : EditorGUIUtility.isProSkin ? Color.black : Color.white;
						GUI.Box(r3, "", plain);
					}
					GUI.color = prevColor;

					if (showRightClickMenu) {
						gm.AddItem(new GUIContent("Only execute this event if the layer weight is larger than 0 (this is the default)"), (mode.intValue & 0x80) == 0, () => {
							mode.intValue ^= 0x80;
							mode.serializedObject.ApplyModifiedProperties();
							Repaint();
						});
						gm.ShowAsContext();
						EditorGUIUtility.ExitGUI();
					}

					EditorGUIUtility.labelWidth = prevLabelWidth;
					GUI.color = previousColor;
				},
				(buttonRect, list) => {
					var menu = new GenericMenu();

					GenericMenu.MenuFunction2 AddActionCallback = (object data) => {
						var Settings = AnimatorEventsEditorSettings.Settings;
						int mode = 0;
						if (!Settings.onlyExecuteIfLayerWeightHigherThan0) mode |= 0x80;
						if (!Settings.showConsoleErrorIfExecutingEventNotFound) mode |= 0x2;

						EventSMB.Action.Type type = (EventSMB.Action.Type) data;
						serializedObject.Update();
						actions.InsertArrayElementAtIndex(actions.arraySize);
						var action = actions.GetArrayElementAtIndex(actions.arraySize - 1);
						action.FindPropertyRelative("type").intValue = (int) type;
						action.FindPropertyRelative("mode").intValue = mode;
						action.FindPropertyRelative("eventId").intValue = 0;
						action.FindPropertyRelative("functionName").stringValue = "";
						action.FindPropertyRelative("parameterHash").intValue = 0;
						action.FindPropertyRelative("value").floatValue = 0;
						serializedObject.ApplyModifiedProperties();
					};

					menu.AddItem(GetPrintName(EventSMB.Action.Type.SetParameter), false, AddActionCallback, EventSMB.Action.Type.SetParameter);
					menu.AddItem(GetPrintName(EventSMB.Action.Type.CallEvent), false, AddActionCallback, EventSMB.Action.Type.CallEvent);
					menu.AddItem(GetPrintName(EventSMB.Action.Type.SendMessage), false, AddActionCallback, EventSMB.Action.Type.SendMessage);

					menu.ShowAsContext();
				}
			);
		}

		static readonly string[] SetParamModeStr = new string[] { " ", "+", "-", "x", "÷", "%", "Min", "Max" };
		static readonly string[] descriptions = new string[] {
			"None: Replace the parameter",
			SetParamModeStr[1] + ": Add to the parameter",
			SetParamModeStr[2] + ": Subtract from the parameter",
			SetParamModeStr[3] + ": Multiply the parameter",
			SetParamModeStr[4] + ": Divide the parameter",
			SetParamModeStr[5] + ": Modulus of the parameter",
			SetParamModeStr[6] + ": Smallest between this and the parameter",
			SetParamModeStr[7] + ": Largest between this and the parameter"
		};


		static readonly string[] ConditionsPopupStr = new string[] {
			"Group/AND GROUP",
			"Group/OR GROUP",
			"Group/NOT",
			"Group/AND",
			"Group/OR",
			"Group/CLOSE",

			"Parameter/Equals",
			"Parameter/Distinct",
			"Parameter/Greater than or equals",
			"Parameter/Less than or equals",
			"Parameter/Greater than",
			"Parameter/Less than",

			"Layer Index/Equals",
			"Layer Index/Distinct",
			"Layer Index/Greater than or equals",
			"Layer Index/Less than or equals",
			"Layer Index/Greater than",
			"Layer Index/Less than",

			"Layer Weight/Equals",
			"Layer Weight/Distinct",
			"Layer Weight/Greater than or equals",
			"Layer Weight/Less than or equals",
			"Layer Weight/Greater than",
			"Layer Weight/Less than",

			"On/Normalized time (%)",
			"On/Normalized time (%) per loop",
			"On/Enter transition starts",
			"On/Enter transition ends",
			"On/Exit transition starts",
			"On/Exit transition ends",

			"After/Normalized time (%)",
			"After/Normalized time (%) per loop",
			"After/Fixed time (seconds)",
			"After/Normalized time (%) Start transition",
			"After/Normalized time (%) Exit transition",
			"After/Enter transition ends",
			"After/Exit transition starts",

			"Before/Normalized Time (%)",
			"Before/Normalized time (%) per loop",
			"Before/Fixed time (seconds)",
			"Before/Normalized time (%) Start transition",
			"Before/Normalized time (%) Exit transition",
			"Before/Enter transition ends",
			"Before/Exit transition starts",

			"Max N Times/After Start",
			"Max N Times/Per Loop",

			"Delegate (requires code)",
		};
		static readonly int[] ConditionsPopupInt = new int[] {
			(int) EventSMB.Condition.Type.AND_GROUP,
			(int) EventSMB.Condition.Type.OR_GROUP,
			(int) EventSMB.Condition.Type.NOT,
			(int) EventSMB.Condition.Type.AND,
			(int) EventSMB.Condition.Type.OR,
			(int) EventSMB.Condition.Type.PARENTHESIS_CLOSE,

			(int) EventSMB.Condition.Type.ParameterEquals,
			(int) EventSMB.Condition.Type.ParameterNotEquals,
			(int) EventSMB.Condition.Type.ParameterGreaterThanOrEquals,
			(int) EventSMB.Condition.Type.ParameterLessThanOrEquals,
			(int) EventSMB.Condition.Type.ParameterGreaterThan,
			(int) EventSMB.Condition.Type.ParameterLessThan,

			(int) EventSMB.Condition.Type.LayerEquals,
			(int) EventSMB.Condition.Type.LayerNotEquals,
			(int) EventSMB.Condition.Type.LayerGreaterThanOrEquals,
			(int) EventSMB.Condition.Type.LayerLessThanOrEquals,
			(int) EventSMB.Condition.Type.LayerGreaterThan,
			(int) EventSMB.Condition.Type.LayerLessThan,

			(int) EventSMB.Condition.Type.LayerWeightEquals,
			(int) EventSMB.Condition.Type.LayerWeightNotEquals,
			(int) EventSMB.Condition.Type.LayerWeightGreaterThanOrEquals,
			(int) EventSMB.Condition.Type.LayerWeightLessThanOrEquals,
			(int) EventSMB.Condition.Type.LayerWeightGreaterThan,
			(int) EventSMB.Condition.Type.LayerWeightLessThan,

			(int) EventSMB.Condition.Type.OnNormalizedTime,
			(int) EventSMB.Condition.Type.OnNormalizedTimePerLoop,
			(int) EventSMB.Condition.Type.OnEnterStart,
			(int) EventSMB.Condition.Type.OnEnterEnd,
			(int) EventSMB.Condition.Type.OnExitStart,
			(int) EventSMB.Condition.Type.OnExitEnd,

			(int) EventSMB.Condition.Type.AfterNormalizedTime,
			(int) EventSMB.Condition.Type.AfterNormalizedTimePerLoop,
			(int) EventSMB.Condition.Type.AfterFixedTime,
			(int) EventSMB.Condition.Type.AfterNormalizedTimeStartTransition,
			(int) EventSMB.Condition.Type.AfterNormalizedTimeExitTransition,
			(int) EventSMB.Condition.Type.AfterEnterEnd,
			(int) EventSMB.Condition.Type.AfterExitStart,

			(int) EventSMB.Condition.Type.BeforeNormalizedTime,
			(int) EventSMB.Condition.Type.BeforeNormalizedTimePerLoop,
			(int) EventSMB.Condition.Type.BeforeFixedTime,
			(int) EventSMB.Condition.Type.BeforeNormalizedTimeStartTransition,
			(int) EventSMB.Condition.Type.BeforeNormalizedTimeExitTransition,
			(int) EventSMB.Condition.Type.BeforeEnterEnd,
			(int) EventSMB.Condition.Type.BeforeExitStart,

			(int) EventSMB.Condition.Type.MaxNTimesAfterStart,
			(int) EventSMB.Condition.Type.MaxNTimesPerLoop,


			(int) EventSMB.Condition.Type.DelegateCondition,
		};

#if DEBUG
		void ShowDebugBox(ref Rect rect, IList<AnimatorEvents.Extensions.DebugData<int>> debugLastCheckWasTrue, int truth = 1) {
			if (debugLastCheckWasTrue == null) return;
			if (!EditorApplication.isPlaying) return;

			for (int i = 0; i < debugLastCheckWasTrue.Count; i++) {
				if (debugLastCheckWasTrue[i].frameCount != Time.frameCount) continue;

				Rect r2 = rect;
				r2.width = i == 0 ? 30 : 50;
				rect.xMin += r2.width;
				var previousColor = GUI.color;
				GUI.color = debugLastCheckWasTrue[i].value >= truth ? Color.green : Color.red;
				GUI.Box(r2, new GUIContent((debugLastCheckWasTrue[i].value >= truth ? "OK" : "NO") + (i > 0 ? " (" + i + ")" : ""), "Was this check successful? Was this executed?\n" +
					"While a state transitions to itself there can be 3 boxes because the same state is playing several times on the same frame.\n" +
					"The box on the upmost right is the one that was checked/executed last."));
				GUI.color = previousColor;
			}
		}
#endif

		UnityEditorInternal.ReorderableList PrepareReorderableAdvancedConditions(SerializedProperty conditions) {
			return CreateReorderableList(conditions,
				new GUIContent(conditions.displayName, conditions.tooltip),
				20,
				(rect, index, isActive, isFocused) => {
					var prevLabelWidth = EditorGUIUtility.labelWidth;
					var previousColor = GUI.color;

					var property = conditions.GetArrayElementAtIndex(index);
					var type = property.FindPropertyRelative("type");
					var value = property.FindPropertyRelative("value");
					var parameterHash = property.FindPropertyRelative("parameterHash");

#if DEBUG
					var boxed = ConditionsSaver.GetTargetObjectOfProperty(property);
					if (boxed != null) {
						var condition = (EventSMB.Condition) boxed;
						ShowDebugBox(ref rect, condition.debugLastCheckWasTrue);
					}
#endif

					Rect r = rect;
					r.height = 18;
					r.width *= 0.3333333f;
					type.intValue = EditorGUI.IntPopup(r, type.intValue, ConditionsPopupStr, ConditionsPopupInt);
					//EditorGUI.PropertyField(r, type, GUIContent.none);
					r.xMin += r.width + 4;
					r.xMax = rect.xMax;

					switch ((EventSMB.Condition.Type) type.intValue) {
						case EventSMB.Condition.Type.OnNormalizedTimePerLoop:
						case EventSMB.Condition.Type.OnNormalizedTime:
						case EventSMB.Condition.Type.AfterNormalizedTime:
						case EventSMB.Condition.Type.AfterNormalizedTimePerLoop:
						case EventSMB.Condition.Type.BeforeNormalizedTime:
						case EventSMB.Condition.Type.BeforeNormalizedTimePerLoop:
							ScrubAnimatorUtil.DrawScrub(r, targetCasted, value, true, m_ParameterInfoList);
							break;
						case EventSMB.Condition.Type.AfterFixedTime:
						case EventSMB.Condition.Type.BeforeFixedTime:
							value.floatValue = EditorGUI.FloatField(r, new GUIContent("Fixed Time", "Time in seconds"), value.floatValue);
							break;
						case EventSMB.Condition.Type.MaxNTimesAfterStart:
						case EventSMB.Condition.Type.MaxNTimesPerLoop:
							value.floatValue = EditorGUI.IntField(r, new GUIContent("Count", "Number of times"), (int) value.floatValue);
							break;
						case EventSMB.Condition.Type.AfterNormalizedTimeStartTransition:
						case EventSMB.Condition.Type.AfterNormalizedTimeExitTransition:
						case EventSMB.Condition.Type.BeforeNormalizedTimeStartTransition:
						case EventSMB.Condition.Type.BeforeNormalizedTimeExitTransition:
							value.floatValue = EditorGUI.Slider(r, new GUIContent("Normalized time"), value.floatValue, 0, 1);
							break;
						case EventSMB.Condition.Type.LayerEquals:
						case EventSMB.Condition.Type.LayerGreaterThanOrEquals:
						case EventSMB.Condition.Type.LayerGreaterThan:
						case EventSMB.Condition.Type.LayerLessThan:
						case EventSMB.Condition.Type.LayerLessThanOrEquals:
						case EventSMB.Condition.Type.LayerNotEquals: {
							string content = "Layer Index " + GetConditionTypeString((EventSMB.Condition.Type) type.intValue);

							var tmp = EditorGUIUtility.labelWidth;
							EditorGUIUtility.labelWidth = r.width / 2;
							var bb = new GUIStyle(EditorStyles.miniButtonLeft);
							bb.fontSize += 15;
							bb.padding = new RectOffset(3, 1, 0, 2);
							bb.margin = new RectOffset();
							bb.alignment = TextAnchor.MiddleCenter;

							parameterHash.intValue = EditorGUI.IntField(r, content, parameterHash.intValue);
							EditorGUIUtility.labelWidth = tmp;
							break;
						}
						case EventSMB.Condition.Type.LayerWeightEquals:
						case EventSMB.Condition.Type.LayerWeightGreaterThanOrEquals:
						case EventSMB.Condition.Type.LayerWeightGreaterThan:
						case EventSMB.Condition.Type.LayerWeightLessThan:
						case EventSMB.Condition.Type.LayerWeightLessThanOrEquals:
						case EventSMB.Condition.Type.LayerWeightNotEquals: {
							string content = "Layer Weight " + GetConditionTypeString((EventSMB.Condition.Type) type.intValue);

							var tmp = EditorGUIUtility.labelWidth;
							EditorGUIUtility.labelWidth = r.width / 2;
							var bb = new GUIStyle(EditorStyles.miniButtonLeft);
							bb.fontSize += 15;
							bb.padding = new RectOffset(3, 1, 0, 2);
							bb.margin = new RectOffset();
							bb.alignment = TextAnchor.MiddleCenter;

							value.floatValue = EditorGUI.Slider(r, content, value.floatValue, 0, 1);
							EditorGUIUtility.labelWidth = tmp;
							break;
						}
						case EventSMB.Condition.Type.ParameterEquals:
						case EventSMB.Condition.Type.ParameterGreaterThanOrEquals:
						case EventSMB.Condition.Type.ParameterGreaterThan:
						case EventSMB.Condition.Type.ParameterLessThan:
						case EventSMB.Condition.Type.ParameterLessThanOrEquals:
						case EventSMB.Condition.Type.ParameterNotEquals: {
							List<GUIContent> parametersStr = new List<GUIContent>();

							int paramIndex = -1;

							for (int i = 0; i < m_ParameterInfoList.Count; i++) {
								parametersStr.Add(new GUIContent(m_ParameterInfoList[i].p.name));
								if (m_ParameterInfoList[i].p.nameHash == parameterHash.intValue) paramIndex = parametersStr.Count - 1;
							}

							if (paramIndex == -1) GUI.color = Color.red;
							Rect r1 = r;
							r1.width *= 0.5f;
							r1.y += 1;
							int selectedIndex = EditorGUI.Popup(r1, GUIContent.none, paramIndex, parametersStr.ToArray());
							if (paramIndex != selectedIndex) {
								paramIndex = selectedIndex;
								parameterHash.intValue = m_ParameterInfoList[selectedIndex].p.nameHash;
							}
							GUI.color = previousColor;

							if (paramIndex != -1) {
								var elem = m_ParameterInfoList[paramIndex];

								Rect r2 = r;
								r2.x += r1.width + 2;
								r2.width -= r1.width + 2;

								string content = GetConditionTypeString((EventSMB.Condition.Type) type.intValue);

								var tmp = EditorGUIUtility.labelWidth;
								EditorGUIUtility.labelWidth = 50;
								switch (elem.p.type) {
									case AnimatorControllerParameterType.Bool:
									case AnimatorControllerParameterType.Trigger:
										value.floatValue = EditorGUI.Toggle(r2, content, value.floatValue == 1) ? 1 : 0;
										break;
									case AnimatorControllerParameterType.Int:
									case AnimatorControllerParameterType.Float:
										var bb = new GUIStyle(EditorStyles.miniButtonLeft);
										bb.fontSize += 15;
										bb.padding = new RectOffset(3, 1, 0, 2);
										bb.margin = new RectOffset();
										bb.alignment = TextAnchor.MiddleCenter;

										value.floatValue = elem.p.type == AnimatorControllerParameterType.Int ?
											EditorGUI.IntField(r2, content, (int) value.floatValue) :
											EditorGUI.FloatField(r2, content, value.floatValue);
										break;
								}
								EditorGUIUtility.labelWidth = tmp;
							}

							break;
						}
					}
				}
			);
		}

		void OnDestroy() {
			ScrubAnimatorUtil.Stop();
		}

		static string GetConditionTypeString(EventSMB.Condition.Type type) {
			switch (type) {
				case EventSMB.Condition.Type.LayerEquals:
				case EventSMB.Condition.Type.ParameterEquals:
				case EventSMB.Condition.Type.LayerWeightEquals:
					return "=";
				case EventSMB.Condition.Type.LayerGreaterThanOrEquals:
				case EventSMB.Condition.Type.ParameterGreaterThanOrEquals:
				case EventSMB.Condition.Type.LayerWeightGreaterThanOrEquals:
					return "≥";
				case EventSMB.Condition.Type.LayerGreaterThan:
				case EventSMB.Condition.Type.ParameterGreaterThan:
				case EventSMB.Condition.Type.LayerWeightGreaterThan:
					return ">";
				case EventSMB.Condition.Type.LayerLessThan:
				case EventSMB.Condition.Type.ParameterLessThan:
				case EventSMB.Condition.Type.LayerWeightLessThan:
					return "<";
				case EventSMB.Condition.Type.LayerLessThanOrEquals:
				case EventSMB.Condition.Type.ParameterLessThanOrEquals:
				case EventSMB.Condition.Type.LayerWeightLessThanOrEquals:
					return "≤";
				case EventSMB.Condition.Type.LayerNotEquals:
				case EventSMB.Condition.Type.ParameterNotEquals:
				case EventSMB.Condition.Type.LayerWeightNotEquals:
					return "≠";
				default: return "INVALID TYPE";
			}
		}

		public static UnityEditorInternal.ReorderableList CreateReorderableList(
			SerializedProperty array,
			GUIContent header,
			int height,
			UnityEditorInternal.ReorderableList.ElementCallbackDelegate drawElement,
			UnityEditorInternal.ReorderableList.AddDropdownCallbackDelegate dropDown = null
			) {

			return CreateReorderableList(array, header, (_) => height, drawElement, dropDown);
		}

		public static UnityEditorInternal.ReorderableList CreateReorderableList(
			SerializedProperty array,
			GUIContent header,
			UnityEditorInternal.ReorderableList.ElementHeightCallbackDelegate elementHeight,
			UnityEditorInternal.ReorderableList.ElementCallbackDelegate drawElement,
			UnityEditorInternal.ReorderableList.AddDropdownCallbackDelegate dropDown = null
			) {

			var rl = new UnityEditorInternal.ReorderableList(array.serializedObject, array, true, false, true, true);
			rl.displayAdd = true;
			rl.displayRemove = true;
			rl.elementHeightCallback = elementHeight;
			rl.drawHeaderCallback = (rect) => {
				GUI.Label(rect, header, EditorStyles.boldLabel);
			};
			rl.drawElementBackgroundCallback = (rect, index, isActive, isFocused) => {
				if (Event.current.type == EventType.Repaint) {
					if (index % 2 == 1) {
						EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
					}
					((GUIStyle) "RL Element").Draw(rect, true, false, isActive, isFocused);
				}
			};
			rl.drawElementCallback = drawElement;
			if (dropDown != null)
				rl.onAddDropdownCallback = dropDown;
			return rl;
		}

		static GUIContent GetPrintName(EventSMB.Action.Type type) {
			switch (type) {
				case EventSMB.Action.Type.CallEvent: return new GUIContent("Event", "Execute an event set in the AnimatorEvent component attached to the Animator.");
				case EventSMB.Action.Type.SendMessage: return new GUIContent("SendMessage", "Calls the API SendMessage on the GameObject that contains the Animator. If possible it is recommended to use Call Event instead of SendMessage for performance reasons.");
				case EventSMB.Action.Type.SetParameter: return new GUIContent("Parameter", "Set the value of a parameter in the AnimatorController.");
			}
			return GUIContent.none;
		}

		// Generate a new random name for a new inline event
		static string RandomString(int size) {
			size = (size / 2) * 2;
			const string vowels = "aeiou";
			const string consonants = "bcdfgjklmnpqstvxz";
			char[] result = new char[size];
			for (var i = 0; i < size; i += 2) {
				result[i + 0] = consonants[UnityEngine.Random.Range(0, consonants.Length)];
				result[i + 1] = vowels[UnityEngine.Random.Range(0, vowels.Length)];
			}
			return new string(result);
		}

		#region Label color depending on brightness
		static bool currentIsDark;
		static Color prevLabelColor, prevFoldoutColor;
		static void SetLabelColors() {
			prevLabelColor = EditorStyles.label.normal.textColor;
			prevFoldoutColor = EditorStyles.foldout.normal.textColor;
			EditorStyles.label.normal.textColor = currentIsDark ? Color.white : Color.black;
			EditorStyles.foldout.normal.textColor = currentIsDark ? Color.white : Color.black;
		}
		static void RevertLabelColors() {
			EditorStyles.label.normal.textColor = prevLabelColor;
			EditorStyles.foldout.normal.textColor = prevFoldoutColor;
		}
		public static bool IsDark(Color color) {
			return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f < 0.5f;
		}
		#endregion
	}

	public class ParameterInfo {
		public AnimatorControllerParameter p;
		public float previewValue;
		public Vector2 minMax;
		public bool show;
	}
	public class CollapsableList {
		public UnityEditorInternal.ReorderableList list;
		public bool show;
	}

	public static class ConditionsSaver {
		// https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
		public static object GetTargetObjectOfProperty(SerializedProperty prop) {
			if (prop == null) return null;

			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements) {
				if (element.Contains("[")) {
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else {
					obj = GetValue_Imp(obj, element);
				}
			}
			return obj;
		}
		private static object GetValue_Imp(object source, string name) {
			if (source == null)
				return null;
			var type = source.GetType();

			while (type != null) {
				var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null)
					return f.GetValue(source);

				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null)
					return p.GetValue(source, null);

				type = type.BaseType;
			}
			return null;
		}
		private static object GetValue_Imp(object source, string name, int index) {
			var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
			if (enumerable == null) return null;
			var enm = enumerable.GetEnumerator();

			for (int i = 0; i <= index; i++) {
				if (!enm.MoveNext()) return null;
			}
			return enm.Current;
		}

		public static void Save(List<EventSMB.Condition> conditions) {
			string path = EditorUtility.SaveFilePanelInProject("Save Conditions preset", "Some category;Conditions", "asset", "Please enter a file name to save the preset to");
			if (path != "") {
				var asset = ScriptableObject.CreateInstance<ConditionPreset>();
				asset.conditions = new List<EventSMB.Condition>(conditions);
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
		public static void Load(UnityEngine.Object toSave, List<EventSMB.Condition> conditions) {
			GenericMenu menu = new GenericMenu();
			foreach (var elem in GetAllInstances<ConditionPreset>()) {
				menu.AddItem(new GUIContent(elem.name.Replace(';', '/')), false, (object preset) => {
					Undo.RecordObject(toSave, "Loaded conditions");
					conditions.Clear();
					conditions.AddRange(((ConditionPreset) preset).conditions);
				}, elem);
			}
			menu.ShowAsContext();
		}

		// https://answers.unity.com/questions/1425758/how-can-i-find-all-instances-of-a-scriptable-objec.html?childToView=1460944#comment-1460944
		public static T[] GetAllInstances<T>() where T : ScriptableObject {
			return AssetDatabase.FindAssets("t:" + typeof(T).Name).Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
		}
	}

	// Unity likes to hide stuff, so let's unhide it and kill some performance while doing it.
	public static class Extensions {
		static BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		public static int recursiveBlendParameterCount(this BlendTree tree) {
			return (int) typeof(BlendTree).GetProperty("recursiveBlendParameterCount", flags).GetValue(tree, null);
		}
		public static string GetRecursiveBlendParameter(this BlendTree tree, int index) {
			return (string) typeof(BlendTree).GetMethod("GetRecursiveBlendParameter", flags).Invoke(tree, new object[] { index });
		}
		public static float GetRecursiveBlendParameterMin(this BlendTree tree, int index) {
			return (float) typeof(BlendTree).GetMethod("GetRecursiveBlendParameterMin", flags).Invoke(tree, new object[] { index });
		}
		public static float GetRecursiveBlendParameterMax(this BlendTree tree, int index) {
			return (float) typeof(BlendTree).GetMethod("GetRecursiveBlendParameterMax", flags).Invoke(tree, new object[] { index });
		}
		public static AnimationClip[] GetAnimationClipsFlattened(this BlendTree tree) {
			return (AnimationClip[]) typeof(BlendTree).GetMethod("GetAnimationClipsFlattened", flags).Invoke(tree, null);
		}
		public static int IndexOfParameter(this AnimatorController controller, string name) {
			return (int) typeof(AnimatorController).GetMethod("IndexOfParameter", flags).Invoke(controller, new object[] { name });
		}
	}
}
