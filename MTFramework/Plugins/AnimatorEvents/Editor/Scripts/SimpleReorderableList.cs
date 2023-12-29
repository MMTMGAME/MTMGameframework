using UnityEditor;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class SimpleReorderableList {
		private static Texture2D entryBG;

		public SerializedProperty entries;
		public delegate void Draw(SerializedProperty entry, int index);
		public Draw draw;
		public delegate Color GetEntryColor(SerializedProperty entry, int index);
		public GetEntryColor getEntryColor;
		public delegate void ResetEntry(SerializedProperty entry);
		public ResetEntry initAddedEntry;
		public bool showAdd = true;
		public bool showRemove = true;

		private int draggingIndex = -1;
		private Vector2 offset;
		private Rect draggedRect;
		private GUIStyle dragBarHandle, delete, BG;

		public SimpleReorderableList() {
			if (entryBG == null) entryBG = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("SRL_EntryBG")[0]));

			dragBarHandle = GUI.skin.GetStyle("RL DragHandle");
			BG = new GUIStyle(GUI.skin.GetStyle("flow node 0"));
			BG.padding = new RectOffset(5, 5, 5, 5);
			BG.normal.background = entryBG;
			delete = GUI.skin.GetStyle("WinBtnClose");
		}

		public void DoLayoutList() {
			Rect previous = default(Rect), next = default(Rect);
			Rect first = default(Rect), last = default(Rect);

			GUILayout.Space(4);

			for (int i = 0; i < entries.arraySize; i++) {
				if (draggingIndex == i) {
					GUILayoutUtility.GetRect(draggedRect.width, draggedRect.height);
				}
				else {
					DrawEntry(i);
				}
				var verticalRect = GUILayoutUtility.GetLastRect();

				GUILayout.Space(6);

				if (Event.current.type == EventType.MouseDown) {
					if (verticalRect.Contains(Event.current.mousePosition)) {
						draggingIndex = i;
						draggedRect = verticalRect;
						offset = Event.current.mousePosition - draggedRect.position;
						Event.current.Use();
					}
				}

				if (draggingIndex != i) {
					if (i == draggingIndex - 1) {
						previous = verticalRect;
					}
					else if (i == draggingIndex + 1) {
						next = verticalRect;
					}
				}

				if (i == 0) first = verticalRect;
				if (i == entries.arraySize - 1) last = verticalRect;
			}


			if (draggingIndex != -1) {
				EditorGUIUtility.AddCursorRect(new Rect(0, 0, 100000, 1000000), MouseCursor.ResizeVertical);
				if (Event.current.type == EventType.MouseDrag) {
					// Move only on the y axis
					draggedRect.y = Mathf.Clamp(Event.current.mousePosition.y - offset.y, first.y, last.y - draggedRect.height + last.height);

					// Try move up
					if (draggingIndex > 0 && draggedRect.y < previous.y + previous.height / 2) {
						entries.MoveArrayElement(draggingIndex, draggingIndex - 1);
						draggingIndex--;
					}

					// Try move down
					else if (draggingIndex < entries.arraySize - 1 && draggedRect.y + draggedRect.height > next.y + next.height / 2) {
						entries.MoveArrayElement(draggingIndex, draggingIndex + 1);
						draggingIndex++;
					}

					Event.current.Use();
				}

				if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore) {
					draggingIndex = -1;
					Event.current.Use();
				}
				else {
					GUILayout.BeginArea(draggedRect);
					DrawEntry(draggingIndex);
					GUILayout.EndArea();
				}
			}

			if (showAdd && GUILayout.Button("Add")) {
				// Add element to the array. The added element is always a copy of the previous one
				entries.InsertArrayElementAtIndex(entries.arraySize);

				if (initAddedEntry != null)
					initAddedEntry(entries.GetArrayElementAtIndex(entries.arraySize - 1));
			}
			entries.serializedObject.ApplyModifiedProperties();
		}

		void DrawEntry(int i) {
			var defaultColor = GUI.color;
			GUI.color = getEntryColor != null ? getEntryColor(entries.GetArrayElementAtIndex(i), i) : new Color(0.5f, 0.83f, 0.76f, 1);
			GUILayout.BeginHorizontal(BG);
			GUI.color = defaultColor;

			GUILayout.BeginVertical(GUILayout.Width(18));
			var handleRect = GUILayoutUtility.GetRect(0, 0, GUILayout.Width(14));
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			if (draw != null)
				draw(entries.GetArrayElementAtIndex(i), i);
			else
				EditorGUILayout.PropertyField(entries.GetArrayElementAtIndex(i), true);
			GUILayout.EndVertical();
			var verticalRect = GUILayoutUtility.GetLastRect();

			if (Event.current.type == EventType.Repaint) {
				handleRect.width = 10;
				handleRect.x += 2;
				handleRect.y = verticalRect.y + verticalRect.height / 2 - dragBarHandle.normal.background.height / 2;
				handleRect.height = dragBarHandle.normal.background.height;
				dragBarHandle.Draw(handleRect, false, false, false, false);
			}

			if (showRemove && GUI.Button(GUILayoutUtility.GetRect(0, 0, GUILayout.Width(14), GUILayout.Height(14)), GUIContent.none, delete)) {
				entries.DeleteArrayElementAtIndex(i);
				entries.serializedObject.ApplyModifiedProperties();
				EditorGUIUtility.ExitGUI(); // Fix. Without this, there's a bug that makes the last entry's Normalized Time to be changed for some reason
			}

			GUILayout.EndHorizontal();
		}
	}
}
