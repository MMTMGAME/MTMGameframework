using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Ashkatchap.AnimatorEventsEditor {
	public static class ScrubAnimatorUtil {
		static EditorWindow AnimatorWindow;
		static readonly Dictionary<int, float> ParamTree = new Dictionary<int, float>();
		static readonly List<float> prevParams = new List<float>();
		// Used ONLY as a cache for the list of attributes the editor has to set as "animated" during an animation preview so that a rollback is possible later.
		static readonly Dictionary<AnimationClip, List<EditorCurveBinding>> ClipBindings = new Dictionary<AnimationClip, List<EditorCurveBinding>>();
		static readonly List<ParameterInfo> PreviewParameters = new List<ParameterInfo>();
		static StateMachineBehaviour Target;
		static float Time;
		static bool UpdatePreview;

		[InitializeOnLoadMethod]
		static void Init() {
			EditorApplication.update += EditorUpdate;
		}

		/// <summary>
		/// Get the Currently used AnimatorController in the current AnimatorWindow and its related selected Animator, if any.
		/// </summary>
		/// <param name="controller">Current controller displayed in the Animator Window</param>
		/// <param name="animator">Current Animator component if inspecting one. May be null</param>
		public static void GetCurrentAnimatorAndController(out AnimatorController controller, out Animator animator) {
			Type animatorWindowType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
			if (AnimatorWindow == null) AnimatorWindow = EditorWindow.GetWindow(animatorWindowType);

			animator = animatorWindowType.GetField("m_PreviewAnimator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AnimatorWindow) as Animator;
			controller = animatorWindowType.GetField("m_AnimatorController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(AnimatorWindow) as AnimatorController;
		}

		/// <summary>
		/// Draw a timeline that can be scrubbed to allow picking a specific normalized time of an animation
		/// </summary>
		public static void DrawScrub(Rect rect, StateMachineBehaviour target, SerializedProperty time, bool isNormalized, List<ParameterInfo> previewParameters) {
			if (!isNormalized) throw new NotImplementedException();

			Animator animator;
			AnimatorController ignore;
			GetCurrentAnimatorAndController(out ignore, out animator);

			float timeBefore = time.floatValue;
			EditorGUI.PrefixLabel(new Rect(rect.x, rect.y, 50, 18), new GUIContent("Time"));
			Rect slider = new Rect(rect.x + 50, rect.y, rect.width - 50 - 42 - 50, 18);
			time.floatValue = GUI.HorizontalSlider(slider, time.floatValue, Math.Min(0, time.floatValue), Math.Max(1, time.floatValue));
			EditorGUI.PropertyField(new Rect(slider.x + slider.width, slider.y, 50, 18), time, GUIContent.none);
			bool preview = GUI.Button(new Rect(rect.x + rect.width - 40, rect.y, 40, 18), new GUIContent("View", "Preview the animation at this normalized time."));

			if (preview || timeBefore != time.floatValue) {
				Target = target;
				Time = time.floatValue;
				UpdatePreview = true;
			}

			for (int i = 0; i < previewParameters.Count; i++) {
				if (previewParameters[i].p.type != AnimatorControllerParameterType.Float) continue; // Only float params affect blend trees
				try {
					if (animator.IsParameterControlledByCurve(previewParameters[i].p.name)) continue;
				}
				catch (Exception) { continue; }

				ParamTree[i] = previewParameters[i].previewValue;
			}

			PreviewParameters.Clear();
			PreviewParameters.AddRange(previewParameters);
		}

		public static void Stop() {
			if (AnimationMode.InAnimationMode()) {
				AnimationMode.StopAnimationMode();
			}
			ClipBindings.Clear();
			UpdatePreview = false;
		}

		// Current bug: This dirties transforms.
		static void EditorUpdate() {
			if (!UpdatePreview) return;
			if (Target == null) return;

			if (!AnimationMode.InAnimationMode())
				AnimationMode.StartAnimationMode();

			{
				AnimatorController controller;
				Animator animator;
				GetCurrentAnimatorAndController(out controller, out animator);
				if (animator == null) return;
				if (controller == null) return;

				var contexts = AnimatorController.FindStateMachineBehaviourContext(Target);

				foreach (var context in contexts) {
					AnimatorState state = context.animatorObject as AnimatorState;
					if (state == null) continue;

					float t = Time;

					AnimationMode.BeginSampling();

					try {
						if (state.motion is BlendTree) {
							var clips = (state.motion as BlendTree).GetAnimationClipsFlattened();
							foreach (var clip in clips) {
								SetClipCurvesAnimationMode(animator, clip);
							}
						}
						else {
							SetClipCurvesAnimationMode(animator, state.motion as AnimationClip);
						}

						//Undo.FlushUndoRecordObjects(); // Doesn't seem to do anything

						// All parameters should start with the default value of the current value on the animator
						var tmpFireEvents = animator.fireEvents;
						animator.fireEvents = false;
						animator.IsInTransition(0); // animator.playableGraph can be null, try to initialize it indirectly.
						animator.Play(state.nameHash, context.layerIndex, t); // Preparing the animator to sample the correct state

						// After doing Animator.Play, we can do animator.GetFloat. Changing the order only works after Unity 2017
						prevParams.Clear();
						for (int i = 0; i < PreviewParameters.Count; i++) {
							if (PreviewParameters[i].p.type != AnimatorControllerParameterType.Float) continue;
							if (!animator.IsParameterControlledByCurve(PreviewParameters[i].p.nameHash)) {
								animator.SetFloat(PreviewParameters[i].p.nameHash, PreviewParameters[i].previewValue);
							}
							prevParams.Add(animator.GetFloat(PreviewParameters[i].p.nameHash));
						}
						if (animator.playableGraph.IsValid()) {
							animator.playableGraph.Evaluate(); // updating the previewed transforms without executing events on the Animator States.
						}
						animator.fireEvents = tmpFireEvents;

						for (int i = 0, j = 0; i < PreviewParameters.Count; i++) {
							if (PreviewParameters[i].p.type != AnimatorControllerParameterType.Float) continue;
							if (!animator.IsParameterControlledByCurve(PreviewParameters[i].p.nameHash)) {
								animator.SetFloat(PreviewParameters[i].p.nameHash, prevParams[j++]);
							}
						}
					}
					catch (Exception e) {
						Debug.LogException(e);
					}

					AnimationMode.EndSampling();

					break;
				}
			}
		}

		public static void SetClipCurvesAnimationMode(Animator animator, AnimationClip clip) {
			if (clip == null) return;
			List<EditorCurveBinding> bindings;
			if (!ClipBindings.TryGetValue(clip, out bindings)) {
				var tmp1 = AnimationUtility.GetCurveBindings(clip);
				var tmp2 = AnimationUtility.GetObjectReferenceCurveBindings(clip);
				bindings = new List<EditorCurveBinding>(tmp1.Length + tmp2.Length);
				bindings.AddRange(tmp1);
				bindings.AddRange(tmp2);

				for (int i = 0; i < bindings.Count; i++) {
					var b = bindings[i];
					var target = animator.transform.Find(b.path);
					if (target == null) {
						bindings.RemoveAt(i--);
						continue;
					}

					// Special cases that don't work as is. This is just so that the animated field can be reverted back later.
					if (b.propertyName.StartsWith("localEulerAnglesRaw")) b.propertyName = b.propertyName.Replace("localEulerAnglesRaw", "m_LocalRotation");

					bindings[i] = b;
				}

				ClipBindings.Add(clip, bindings);
			}
			foreach (var bTmp in bindings) {
				var b = bTmp;

#if !UNITY_2017_4_OR_NEWER || UNITY_2017_4
				// create dummy
				var tmp = new PropertyModification();
				var target = animator.transform.Find(b.path);
				if (b.type == typeof(GameObject)) tmp.target = target.gameObject;
				else tmp.target = target.GetComponent(b.type);
				tmp.propertyPath = b.propertyName;
				object currentValue = GetCurrentValue(animator.gameObject, b);
				if (currentValue is UnityEngine.Object)
					tmp.objectReference = (UnityEngine.Object) currentValue;
				else
					tmp.value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", currentValue);

				AnimationMode.AddPropertyModification(b, tmp, false);
#else
				AnimationMode.AddEditorCurveBinding(animator.gameObject, b);
#endif
			}
		}

#if !UNITY_2017_4_OR_NEWER || UNITY_2017_4
		static object GetCurrentValue(GameObject rootGameObject, EditorCurveBinding curveBinding) {
			if ((UnityEngine.Object) (object) rootGameObject != null) {
				if (curveBinding.isPPtrCurve) {
					UnityEngine.Object value;
					AnimationUtility.GetObjectReferenceValue(rootGameObject, curveBinding, out value);
					return value;
				}
				float value2;
				AnimationUtility.GetFloatValue(rootGameObject, curveBinding, out value2);
				return value2;
			}
			if (curveBinding.isPPtrCurve) {
				return null;
			}
			return 0f;
		}
#endif
	}
}
