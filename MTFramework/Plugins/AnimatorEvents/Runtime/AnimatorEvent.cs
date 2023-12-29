using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ashkatchap.AnimatorEvents {
	[RequireComponent(typeof(Animator))]
	[ExecuteInEditMode]
	public class AnimatorEvent : MonoBehaviour, ISerializationCallbackReceiver {
		static readonly Dictionary<Animator, List<AnimatorEvent>> ByAnimator = new Dictionary<Animator, List<AnimatorEvent>>(32);
		static readonly Stack<List<AnimatorEvent>> Pool = new Stack<List<AnimatorEvent>>();
		static readonly List<Animator> AnimatorsTmp = new List<Animator>();

		[Tooltip("The events in this AnimatorEvent can be called by animators placed on parent GameObjects (in the scene hierarchy).\n\nThe events will always be available for the Animator in this GameObject independently of this setting.")]
		public bool useParentAnimators;

		[Tooltip("The events in this AnimatorEvent can be called by animators placed on children GameObject (in the scene hierarchy).\n\nThe events will always be available for the Animator in this GameObject independently of this setting.")]
		public bool useChildrenAnimators;

		/// <summary>
		/// Only edit this inside the editor, as it gets baked in <see cref="runtimeEventsById"/> and this gets unused after that.
		/// </summary>
		public List<EventElement> events = new List<EventElement>();

		readonly Dictionary<int, EventElement> runtimeEventsById = new Dictionary<int, EventElement>(32);
		readonly Dictionary<string, EventElement> runtimeEventsByName = new Dictionary<string, EventElement>(32);
		/// <summary>
		/// Used to be able to support Hot Reloading
		/// </summary>
		List<EventElement> runtimeEventsCache = new List<EventElement>();
		List<Animator> animators = new List<Animator>();

		#if UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Support "Domain Reloading disabled"
		static void Init() {
			ByAnimator.Clear();
			Pool.Clear();
			Dummy.Clear();
		}
		#endif

		void Start() {
			if (useChildrenAnimators) GetComponentsInChildren<Animator>(true, animators);
			else animators.Add(GetComponent<Animator>());

			if (useParentAnimators) {
				GetComponentsInParent<Animator>(true, AnimatorsTmp);
				animators.AddRange(AnimatorsTmp);
				AnimatorsTmp.Clear();
			}

			foreach (var elem in events) {
				AddEventRuntime(elem);
			}

			foreach (var animator in animators) {
				List<AnimatorEvent> ae;
				if (!ByAnimator.TryGetValue(animator, out ae)) {
					ae = Pool.Count > 0 ? Pool.Pop() : new List<AnimatorEvent>();
					ByAnimator.Add(animator, ae);
				}
				ae.Add(this);
			}
		}

		public bool onDestroyCalled { get; private set; }
		void OnDestroy() {
			onDestroyCalled = true;

			foreach (var animator in animators) {
				List<AnimatorEvent> ae;
				if (ByAnimator.TryGetValue(animator, out ae)) {
					if (ae.Remove(this) && ae.Count == 0) {
						Pool.Push(ae);
						ByAnimator.Remove(animator);
					}
				}
			}
		}

#if DEBUG && UNITY_EDITOR
		[NonSerialized] public static List<IList> DebugArraysToClearEveryFrame = new List<IList>();
		[NonSerialized] static int LastFrameCountCleared;
		void FixedUpdate() {
			CleanUpAtStart();
		}
		void Update() {
			CleanUpAtStart();
		}
		void CleanUpAtStart() {
			// Clean up the debug arrays of EventSMB before it executes
			if (LastFrameCountCleared == Time.frameCount) return;

			foreach (var elem in DebugArraysToClearEveryFrame) if (elem != null) elem.Clear();

			DebugArraysToClearEveryFrame.Clear();
			LastFrameCountCleared = Time.frameCount;
		}
#endif

		public bool AddEventRuntime(EventElement ev) {
			if (!runtimeEventsById.ContainsKey(ev.id) && !runtimeEventsByName.ContainsKey(ev.name)) {
				runtimeEventsById.Add(ev.id, ev);
				runtimeEventsByName.Add(ev.name, ev);
				return true;
			}
			return false;
		}
		public bool DelEventRuntime(EventElement ev) {
			EventElement evOut;
			if (runtimeEventsById.TryGetValue(ev.id, out evOut) && evOut == ev && runtimeEventsByName.TryGetValue(ev.name, out evOut) && evOut == ev) {
				runtimeEventsById.Remove(ev.id);
				runtimeEventsByName.Remove(ev.name);
				return true;
			}
			return false;
		}

		private static readonly List<AnimatorEvent> Dummy = new List<AnimatorEvent>();
		public static List<AnimatorEvent> Get(Animator animator) {
			List<AnimatorEvent> ae;
			if (!ByAnimator.TryGetValue(animator, out ae)) {
				ae = Dummy;
			}
			return ae;
		}

		public void CallEvent(string name) {
			TryCallEvent(name);
		}
		public bool TryCallEvent(string name) {
			EventElement ev;
			if (runtimeEventsByName.TryGetValue(name, out ev)) {
				CallEvent(ev.id);
				return true;
			}
			return false;
		}

		public bool CallEvent(int id) {
			return CallEvent(ref id, null);
		}

		public bool CallEvent(ref int id, string functionName) {
			EventElement ev;
			if (runtimeEventsById.TryGetValue(id, out ev)) {
				ev.action.Invoke();
				return true;
			}

			if (runtimeEventsByName.TryGetValue(functionName, out ev)) {
				id = ev.id;
				ev.action.Invoke();
				return true;
			}

			return false;
		}

		public EventElement AddEventForEditor() {
			// Avoid any collision of id.
			int idCandidate = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			while (events.Exists(e => e.id == idCandidate))
				idCandidate++;

			var ev = new EventElement() {
				id = idCandidate,
				name = ""
			};
			events.Add(ev);
			return ev;
		}

		public void OnBeforeSerialize() {
			foreach (var elem in runtimeEventsByName) {
				runtimeEventsCache.Add(elem.Value);
			}
		}

		public void OnAfterDeserialize() {
			foreach (var elem in runtimeEventsCache) {
				AddEventRuntime(elem);
			}
			runtimeEventsCache.Clear();
		}

		[Serializable]
		public class EventElement {
			public string name;
			public int id;
			public UnityEvent action;
		}
	}
}
