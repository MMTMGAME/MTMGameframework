#if UNITY_EDITOR
//#define DEBUG_AE
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class EventSMB : StateMachineBehaviourExtended {
		public enum FilterByControllerMode : byte { Any, MustMatch, MustNotMatch }

		/// <summary>
		/// If true, Event conditions will evaluate to false resulting on events not executing.
		/// </summary>
		public static bool AllConditionsEvaluateToFalse;

		public List<RuntimeAnimatorController> controllers = new List<RuntimeAnimatorController>();
		/// <summary>
		/// If true, it MUST be one of the controllers. If false, it MUST NOT be one of the controllers
		/// </summary>
		public FilterByControllerMode controllerOkIfFound;

		public struct Data {
			// Needed to calculate fixed time (seconds)
			public float fixedTime;
			public float lastNormalizedTime;
		}

		[Serializable]
		public struct Condition {
			/// <summary>
			/// Ignore the value value when copying or comparing.
			/// </summary>
			public class IgnoreValueAttribute : Attribute { }

			internal const int BeforeCondTypeOffset = 40;

			public enum Type : byte {
				PARENTHESIS_CLOSE = 0,
				AND = 1,
				OR = 2,
				NOT = 3,

				MaxNTimesAfterStart = 4,
				MaxNTimesPerLoop = 5,

				OnExitEnd = 6,
				AfterEnterEnd = 7,
				AfterExitStart = 8,

				[IgnoreValue] OnNormalizedTime = 9,
				[IgnoreValue] OnNormalizedTimePerLoop = 10,
				[IgnoreValue] AfterNormalizedTime = 11,

				[IgnoreValue] AfterFixedTime = 12,

				[IgnoreValue] AfterNormalizedTimeStartTransition = 13,
				[IgnoreValue] AfterNormalizedTimeExitTransition = 14,

				ParameterEquals = 16,
				ParameterGreaterThanOrEquals = 17,
				ParameterGreaterThan = 21,
				ParameterLessThan = 22,
				ParameterLessThanOrEquals = 23,
				ParameterNotEquals = 24,

				DelegateCondition = 18,

				AND_GROUP = 19,
				OR_GROUP = 20,

				LayerEquals = 25,
				LayerNotEquals = 26,
				LayerGreaterThan = 27,
				LayerLessThan = 28,
				LayerGreaterThanOrEquals = 29,
				LayerLessThanOrEquals = 30,

				LayerWeightEquals = 31,
				LayerWeightNotEquals = 32,
				LayerWeightGreaterThan = 33,
				LayerWeightLessThan = 34,
				LayerWeightGreaterThanOrEquals = 35,
				LayerWeightLessThanOrEquals = 36,

				[IgnoreValue] AfterNormalizedTimePerLoop = 37,

				OnEnterStart = 38,
				OnEnterEnd = 39,
				OnExitStart = 40,

				// Careful, 46 is the last available number here

				BeforeEnterEnd = AfterEnterEnd + BeforeCondTypeOffset,
				BeforeExitStart = AfterExitStart + BeforeCondTypeOffset,
				[IgnoreValue] BeforeNormalizedTime = AfterNormalizedTime + BeforeCondTypeOffset,
				[IgnoreValue] BeforeFixedTime = AfterFixedTime + BeforeCondTypeOffset,
				[IgnoreValue] BeforeNormalizedTimeStartTransition = AfterNormalizedTimeStartTransition + BeforeCondTypeOffset,
				[IgnoreValue] BeforeNormalizedTimeExitTransition = AfterNormalizedTimeExitTransition + BeforeCondTypeOffset,
				[IgnoreValue] BeforeNormalizedTimePerLoop = AfterNormalizedTimePerLoop + BeforeCondTypeOffset,

				// More numbers available here
			}

			public Type type;
#if DEBUG && UNITY_EDITOR
			[NonSerialized] public List<Extensions.DebugData<int>> debugLastCheckWasTrue;
#endif

			/// <summary>
			/// Represents a time or a value to compare against a parameter
			/// </summary>
			public float value;
			public int parameterHash; // or layer index
			internal AnimatorControllerParameterType parameterType;

			/// <summary>
			/// Delegate to make a custom condition from code.
			/// </summary>
			/// <returns>True if the condition is met.</returns>
			public delegate bool ConditionDelegate(Animator animator, int layerIndex, AnimatorStateInfo stateInfo, State prev, State now, Data data, Entry.Data entryData);
			public ConditionDelegate conditionDelegate;

			public Condition(Type type) : this(type, 0) { }
			public Condition(Type type, float value) : this() {
				this.type = type;
				this.value = value;
			}
			public Condition(Type type, int parameterHash, float value) : this() {
				this.type = type;
				this.value = value;
				this.parameterHash = parameterHash;
			}
			public Condition(ConditionDelegate conditionDelegate) : this() {
				this.type = Type.DelegateCondition;
				this.conditionDelegate = conditionDelegate;
			}

			/// <summary>
			/// Set of simple conditions to be used in the simple mode dropdown
			/// </summary>
			public static readonly KeyValuePair<string, Condition[]>[] Prepared = new KeyValuePair<string, Condition[]>[] {
				new KeyValuePair<string, Condition[]>("On State Update (every frame)", new Condition[]{ }),

				new KeyValuePair<string, Condition[]>("On State Enter, Transition Start", new Condition[]{
					new Condition(Type.OnEnterStart)
				}),
				new KeyValuePair<string, Condition[]>("On State Enter, Transition End", new Condition[]{
					new Condition(Type.OnEnterEnd)
				}),
				new KeyValuePair<string, Condition[]>("On State Exit, Transition Start", new Condition[]{
					new Condition(Type.OnExitStart)
				}),
				new KeyValuePair<string, Condition[]>("On State Exit, Transition End", new Condition[]{
					new Condition(Type.OnExitEnd)
				}),

				// Bit 0 is on the left
				// On All Loops:      [No|Yes] bit 0
				// At Least:          [No|On Exit Start|On Exit End] bit 1
				// During Transition: [Yes|No] bit 2
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|000", new Condition[]{
					new Condition(Type.OnNormalizedTime, 0),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|001", new Condition[]{
					new Condition(Type.OnNormalizedTime, 0),
					new Condition(Type.NOT),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|010", new Condition[]{
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AND),
					new Condition(Type.OnNormalizedTime, 0),
					new Condition(Type.OR),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|011", new Condition[]{
					// Same as 010
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AND),
					new Condition(Type.OnNormalizedTime, 0),
					new Condition(Type.OR),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|020", new Condition[]{
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AND),
					new Condition(Type.OnNormalizedTime, 0),
					new Condition(Type.OR),
					new Condition(Type.OnExitEnd),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|021", new Condition[]{
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AND),
					new Condition(Type.OnExitEnd),
					new Condition(Type.OR),
					new Condition(Type.OnNormalizedTime, 0),
					new Condition(Type.NOT),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|100", new Condition[]{
					new Condition(Type.OnNormalizedTimePerLoop, 0)
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|101", new Condition[]{
					new Condition(Type.OnNormalizedTimePerLoop, 0),
					new Condition(Type.NOT),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|110", new Condition[]{
					new Condition(Type.OnNormalizedTimePerLoop, 0),
					new Condition(Type.OR),
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|111", new Condition[]{
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.AfterExitStart),
					new Condition(Type.OR),
					new Condition(Type.OnNormalizedTimePerLoop, 0),
					new Condition(Type.NOT),
					new Condition(Type.AfterExitStart),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|120", new Condition[]{
					new Condition(Type.OnNormalizedTimePerLoop, 0),
					new Condition(Type.OR),
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.OnExitEnd),
				}),
				new KeyValuePair<string, Condition[]>("On Normalized Time Reached|121", new Condition[]{
					new Condition(Type.MaxNTimesAfterStart, 1),
					new Condition(Type.OnExitEnd),
					new Condition(Type.OR),
					new Condition(Type.OnNormalizedTimePerLoop, 0),
					new Condition(Type.NOT),
					new Condition(Type.AfterExitStart),
				}),
			};
		}

		[Serializable]
		public struct Action {
			public enum Type : byte {
				CallEvent,
				SendMessage,
				SetParameter
			}

			public enum SetParamMode : byte {
				Replace, Add, Sub, Mul, Div, Mod, Max, Min
			}

			public Type type;

			/// <summary>
			/// If SendMessage:
			/// 	bit 0:
			/// 		0 = No object. 1 = Use sendMessageTxt.
			/// If CallEvent:
			/// 	bit 0:
			/// 		0 = Use eventId.
			/// 		1 = Execute delegate.
			/// 	bit 1:
			/// 		0 = throw error if event not found.
			/// 		1 = silently fail if event not found.
			/// If SetParam:
			/// 	bits 0,1,2:
			/// 		cast to SetParamMode.
			/// 
			/// Always mask with 0x127
			/// Bit 7: 
			/// 	0 = Execute only if layer weight > 0.
			/// 	1 = execute always
			/// </summary>
			public byte mode;

			// Events
			public int eventId;
			public System.Action callback;

			// SendMessage function, or event name to have something in case the event dissapears
			public string functionName;

			// SetParameter
			internal AnimatorControllerParameterType parameterType;
			public int parameterHash;
			public double value;

#if DEBUG && UNITY_EDITOR
			[NonSerialized] public List<Extensions.DebugData<int>> debugLastCheckWasTrue;
#endif

			public Action(int eventId) : this() {
				type = Type.CallEvent;
				mode = 0;
				this.eventId = eventId;
			}

			public Action(System.Action callback) : this() {
				type = Type.CallEvent;
				mode = 1;
				this.callback = callback;
			}

			public Action(string functionName) : this() {
				type = Type.SendMessage;
				mode = 0;
				this.functionName = functionName;
			}

			public Action(string functionName, float value) : this() {
				type = Type.SendMessage;
				mode = 1;
				this.functionName = functionName;
				this.value = value;
			}

			public Action(int parameterHash, float value, SetParamMode setParameterMode = SetParamMode.Replace) : this() {
				type = Type.SetParameter;
				mode = (byte) setParameterMode;
				this.parameterHash = parameterHash;
				this.value = value;
			}

			public void Execute(EventSMB eSMB, float layerWeight, Animator animator) {
#if DEBUG && UNITY_EDITOR
				Extensions.AddLast(ref debugLastCheckWasTrue, 0);
#endif
				if ((mode & 0x80) == 0 && layerWeight == 0) return;
#if DEBUG && UNITY_EDITOR
				Extensions.SetLast(ref debugLastCheckWasTrue, 1);
#endif

				switch (type) {
					case Type.SendMessage:
						if ((mode & 0x1) == 0)
							animator.transform.SendMessage(functionName, SendMessageOptions.DontRequireReceiver);
						else
							animator.transform.SendMessage(functionName, value, SendMessageOptions.DontRequireReceiver);
						break;
					case Type.CallEvent:
						if ((mode & 0x1) == 1) {
							callback.Invoke();
						}
						else {
							List<AnimatorEvent> aes = AnimatorEvent.Get(animator);
							bool executed = false;
							bool aeIsDestroyed = false;
							for (int i = 0; i < aes.Count; i++) {
								var ae = aes[i];
								// We use the functionName for the cases where the id doesn't match, but a function name does (and the id is updated for that event, because searching by name is slow)
								executed |= ae.CallEvent(ref eventId, functionName);
								if (i < aes.Count && ae != aes[i]) i--; // Fix in case the current animator event was removed, which will move the list at least one item back.

								aeIsDestroyed |= ae.onDestroyCalled;
							}
							if (!executed && (mode & 0x2) == 0) {
#if DEBUG && UNITY_EDITOR
								Extensions.SetLast(ref debugLastCheckWasTrue, 0);
#endif
								Debug.LogError($"Couldn't execute event \"" + functionName + "\" in \"" + Extensions.GetFullPath(animator) + "\". It wasn't found in any AnimatorEvent of the " + aes.Count + " found"+(aeIsDestroyed ? " (at least one of them executed OnDestroy() so it's assumed it was destroyed)" : "")+". You can avoid throwing this error when the event isn't found by setting eventNotFoundThrowsError to true in the AnimatorEvent component", animator);
							}
						}
						break;
					case Type.SetParameter:
						switch ((SetParamMode) (mode & 0x7)) {
							case SetParamMode.Replace:
								eSMB.SetParamValue(animator, value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Add:
								eSMB.SetParamValue(animator, eSMB.GetParamValue(animator, ref parameterType, parameterHash) + value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Sub:
								eSMB.SetParamValue(animator, eSMB.GetParamValue(animator, ref parameterType, parameterHash) - value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Mul:
								eSMB.SetParamValue(animator, eSMB.GetParamValue(animator, ref parameterType, parameterHash) * value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Div:
								eSMB.SetParamValue(animator, eSMB.GetParamValue(animator, ref parameterType, parameterHash) / value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Mod:
								eSMB.SetParamValue(animator, eSMB.GetParamValue(animator, ref parameterType, parameterHash) % value, ref parameterType, parameterHash);
								break;
							case SetParamMode.Max:
								eSMB.SetParamValue(animator, Math.Max(eSMB.GetParamValue(animator, ref parameterType, parameterHash), value), ref parameterType, parameterHash);
								break;
							case SetParamMode.Min:
								eSMB.SetParamValue(animator, Math.Min(eSMB.GetParamValue(animator, ref parameterType, parameterHash), value), ref parameterType, parameterHash);
								break;
						}
						break;
				}
			}

			public override string ToString() {
				switch (type) {
					case Type.SendMessage:
						if ((mode & 0x1) == 0)
							return "SendMessage(" + functionName + ")";
						else
							return "SendMessage(" + functionName + ", " + value + ")";
					case Type.CallEvent:
						if ((mode & 0x1) == 0)
							return "Call event \"" + functionName + "\" with id \"" + eventId + "\"";
						else
							return "Invoke Action";
					case Type.SetParameter:
						switch ((SetParamMode) (mode & 0x7)) {
							case SetParamMode.Replace:
								return "SetParameter, replace hash " + parameterHash + " to value " + value;
							case SetParamMode.Add:
								return "SetParameter, addition hash " + parameterHash + " value " + value;
							case SetParamMode.Sub:
								return "SetParameter, substract hash " + parameterHash + " value " + value;
							case SetParamMode.Mul:
								return "SetParameter, Multiply hash " + parameterHash + " value " + value;
							case SetParamMode.Div:
								return "SetParameter, Divide hash " + parameterHash + " value " + value;
							case SetParamMode.Mod:
								return "SetParameter, Modulus hash " + parameterHash + " value " + value;
							case SetParamMode.Min:
								return "SetParameter, Min hash " + parameterHash + " value " + value;
							case SetParamMode.Max:
								return "SetParameter, Max hash " + parameterHash + " value " + value;
							default:
								return "Invalid SetParameter mode";
						}
					default: return "Invalid action. Not an CallEvent, SetParameter or SendMessage";
				}
			}
		}

		[Serializable]
		public class Entry {
			public struct Data {
				public float lastExecutedFixedTime, lastExecutedNormalizedTime;
				public int executionCountSinceStart, executionCountSinceLoop;

				public static readonly Data New = new Data() {
					lastExecutedFixedTime = float.NegativeInfinity,
					lastExecutedNormalizedTime = float.NegativeInfinity,
					executionCountSinceStart = 0,
					executionCountSinceLoop = 0
				};
			}

			/// <summary>
			/// -1 = advanced
			/// 0 or greater = predefined set of conditions
			/// Points to an element in PreparedConditions if 0 or greater.
			/// </summary>
			[SerializeField] private short preparedConditionsIndex = -1;

			[SerializeField] private Color color;

			/// <summary>
			/// List of restrictions, or conditions, for the <see cref="actions"/> to happen. If none, the actions will be executed every update.
			/// Array instead of list for performance reasons. To change the conditions, either replace the elements of the array or replace the array with another one.
			/// </summary>
			[Tooltip("List of conditions that need to be met to be able to execute the actions.")]
			public List<Condition> conditions = new List<Condition>();
			/// <summary>
			/// Both <see cref="conditions"/> and <see cref="conditionsExtra"/> must resolve to True
			/// </summary>
			[Tooltip("List of extra conditions that need to be met to be able to execute the actions.")]
			public List<Condition> conditionsExtra = new List<Condition>();

#if DEBUG && UNITY_EDITOR
			[NonSerialized] public List<Extensions.DebugData<int>> debugLastCheckWasTrue;
#endif

			/// <summary>
			/// Actions to execute when the conditions are met in a certain Update.
			/// </summary>
			public List<Action> actions = new List<Action>();

			[NonSerialized] internal TinyArray2<Data> dataPerEntry = new TinyArray2<Data>();

			public Entry(short preparedConditionsIndex = -1) {
				this.preparedConditionsIndex = preparedConditionsIndex;
				Reset();
			}

			public void Reset() {
				dataPerEntry[0] = Data.New;
				dataPerEntry[1] = Data.New;
				actions.Clear();
				conditions.Clear();
				conditionsExtra.Clear();

				// Reset conditions
				if (preparedConditionsIndex == -1) return;
				else if (preparedConditionsIndex < Condition.Prepared.Length) {
					var preparedConditions = Condition.Prepared[preparedConditionsIndex];
					conditions.AddRange(preparedConditions.Value);
				}
				else {
					throw new Exception("Prepared Condition Index not found: " + preparedConditionsIndex);
				}
			}
		
			public Entry Clone() {
				var clone = new Entry();
				clone.preparedConditionsIndex = preparedConditionsIndex;
				clone.color = color;
				clone.conditions.AddRange(conditions);
				clone.conditionsExtra.AddRange(conditionsExtra);
				clone.actions.AddRange(actions);
				return clone;
			}
		}

		public static EventSMB CurrentlyExecutingEventSMB;
		public static Entry CurrentlyExecutingEntry;
		public List<Entry> entries = new List<Entry>();
		private Data[] data = new Data[2];
		[NonSerialized] private AnimatorParametersCache parametersCache;

		#if UNITY_2019_3_OR_NEWER
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Support "Domain Reloading disabled"
		static void Init() {
			CurrentlyExecutingEventSMB = null;
			CurrentlyExecutingEntry = null;
		}
		#endif

		protected override void InitData(byte dataIndex) {
			//Debug.LogWarning("InitData: " + dataIndex);
			data[dataIndex] = default(Data);
			data[dataIndex].lastNormalizedTime = -1; // Impossible value to help with OnNormalizedTime special case and avoid everything equal to 0 when comparing values.
			for (int i = 0; i < entries.Count; i++) {
				entries[i].dataPerEntry[dataIndex] = Entry.Data.New;
			}
		}

#if DEBUG_AE
		protected override string ToStringData(byte dataIndex) {
			return entries.Count > 0 ? "executionCountSinceStart: " + entries[0].dataPerEntry[dataIndex].executionCountSinceStart : "[-]";
		}
#endif

		public override void StateUpdate(Animator animator, ref AnimatorStateInfo stateInfo, int layerIndex, State prev, State now, byte dataIndex) {
			if (prev == State._0_NotPlaying) {
				data[dataIndex].fixedTime = 0;
			}
			else {
				// Update playedTime. Handles any update mode, and changes of it, and any Time.timeScale ammount
				if ((int) stateInfo.normalizedTime > (int) data[dataIndex].lastNormalizedTime) { // Cast to int istead of Math.Floor because this numbers will always be positive
					for (int i = 0; i < entries.Count; i++) {
						var tmp = entries[i].dataPerEntry[dataIndex];
						tmp.executionCountSinceLoop = 0;
						entries[i].dataPerEntry[dataIndex] = tmp;
					}
				}
				data[dataIndex].fixedTime += (stateInfo.normalizedTime - data[dataIndex].lastNormalizedTime) * stateInfo.length;
			}


			// Check that the current controller playing is a valid one
			bool controllerOk = true;
			if (controllers.Count > 0 && controllerOkIfFound != FilterByControllerMode.Any) {
				var controllerToCheck = animator.runtimeAnimatorController;
				for (int i = 0; i < controllers.Count; i++) {
					if (controllers[i] == controllerToCheck) {
						controllerOk = controllerOkIfFound == FilterByControllerMode.MustMatch;
						goto CONTROLLER_CHECKED;
					}
				}
				controllerOk = controllerOkIfFound == FilterByControllerMode.MustNotMatch;
			}


			CONTROLLER_CHECKED:
			if (controllerOk && !AllConditionsEvaluateToFalse) {
				// Check conditions and execute actions
				for (int i = 0; i < entries.Count; i++) {
					var tmp = entries[i].dataPerEntry[dataIndex];
#if DEBUG && UNITY_EDITOR
					Extensions.AddLast(ref entries[i].debugLastCheckWasTrue, 0);
#endif
					if (EvaluateConditions(entries[i], entries[i].conditions, animator, layerIndex, ref stateInfo, prev, now, ref data[dataIndex], ref tmp)) {
#if DEBUG && UNITY_EDITOR
						Extensions.SetLast(ref entries[i].debugLastCheckWasTrue, 1);
#endif
						if (EvaluateConditions(entries[i], entries[i].conditionsExtra, animator, layerIndex, ref stateInfo, prev, now, ref data[dataIndex], ref tmp)) {
							TryExecute(entries[i], animator, layerIndex, ref stateInfo, ref data[dataIndex], ref tmp);
#if DEBUG && UNITY_EDITOR
							Extensions.SetLast(ref entries[i].debugLastCheckWasTrue, 2);
#endif
						}
					}
					entries[i].dataPerEntry[dataIndex] = tmp;
				}
			}

			data[dataIndex].lastNormalizedTime = stateInfo.normalizedTime;
		}

		private bool EvaluateConditions(Entry entry, List<Condition> conditions, Animator animator, int layerIndex, ref AnimatorStateInfo stateInfo, State prev, State now, ref Data data, ref Entry.Data entryData) {
			int conditionIndex = -1;
			return RecursiveEvaluateConditions(entry, conditions, ref conditionIndex, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, false);
		}

		// State prev isn't used in any condition. Could be used for a "OnTransitioningInterrupted" condition
		// Entry isn't used in any condition. Could be used to store information specific for the entry.
		private bool RecursiveEvaluateConditions(Entry entry, List<Condition> conditions, ref int index, Animator animator, int layerIndex, ref AnimatorStateInfo stateInfo, State prev, State now, ref Data data, ref Entry.Data entryData, bool groupOr = false, bool debugResult = true) {
			bool result = true;
			while (++index < conditions.Count) {
				bool resultStep;
				int indexNow = index;
				switch (conditions[index].type) {
					// OPERANDS
					case Condition.Type.PARENTHESIS_CLOSE:
						return result;
					case Condition.Type.AND: {
						bool r = RecursiveEvaluateConditions(entry, conditions, ref index, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, groupOr);
						if (debugResult) DebugResult(conditions, indexNow, r);
						result &= r;
						continue;
					}
					case Condition.Type.OR: {
						if (index == 0) result = false;
						bool r = RecursiveEvaluateConditions(entry, conditions, ref index, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, groupOr);
						if (debugResult) DebugResult(conditions, indexNow, r);
						result |= r;
						continue;
					}
					case Condition.Type.NOT:
						resultStep = !RecursiveEvaluateConditions(entry, conditions, ref index, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, groupOr);
						break;

					// OPERANDS 2 (easier to use and understand. Inside a group, each condition uses the operant of the group)
					case Condition.Type.AND_GROUP:
						resultStep = RecursiveEvaluateConditions(entry, conditions, ref index, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, false);
						break;
					case Condition.Type.OR_GROUP:
						resultStep = RecursiveEvaluateConditions(entry, conditions, ref index, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, true);
						break;

					// CONDITIONS
					case Condition.Type.MaxNTimesAfterStart:
						resultStep = entryData.executionCountSinceStart < conditions[index].value;
						break;
					case Condition.Type.MaxNTimesPerLoop:
						resultStep = entryData.executionCountSinceLoop < conditions[index].value;
						break;

					case Condition.Type.AfterEnterEnd:
						resultStep = now >= State._2_Updating && now <= State._4_ExitLastFrame;
						break;
					case Condition.Type.AfterExitStart:
						resultStep = now >= State._3_ExitTransitioning && now <= State._4_ExitLastFrame;
						break;
					case Condition.Type.OnExitEnd:
						resultStep = now == State._4_ExitLastFrame;
						break;
					case Condition.Type.OnEnterStart:
						resultStep = now >= State._1_StartTransitioning && prev <= State._0_NotPlaying;
						break;
					case Condition.Type.OnEnterEnd:
						resultStep = now >= State._2_Updating && prev <= State._1_StartTransitioning;
						break;
					case Condition.Type.OnExitStart:
						resultStep = now >= State._3_ExitTransitioning && prev <= State._2_Updating;
						break;

					case Condition.Type.OnNormalizedTimePerLoop: {
						if (!stateInfo.loop) goto case Condition.Type.OnNormalizedTime;

						float offsetNow = stateInfo.normalizedTime - conditions[index].value;
						float offsetPrev = data.lastNormalizedTime - conditions[index].value;
						int div = (int) (conditions[index].value * 0.999f) + 1; // This takes into account normalized times greater than 1, to loop multiple times.
						int i = Math.Max(0, (int) offsetNow) / div;
						offsetNow /= div;
						offsetPrev /= div;
						resultStep = offsetNow >= i && (offsetPrev < i || data.lastNormalizedTime == -1); // Special case entry.conditions[index].value=0
						break;
					}
					case Condition.Type.OnNormalizedTime: {
						float offsetNow = stateInfo.normalizedTime - conditions[index].value;
						float offsetPrev = data.lastNormalizedTime - conditions[index].value;
						resultStep = offsetNow >= 0 && (offsetPrev < 0 || data.lastNormalizedTime == -1); // Special case entry.conditions[index].value=0
						break;
					}

					case Condition.Type.AfterNormalizedTimePerLoop:
						if (!stateInfo.loop) goto case Condition.Type.AfterNormalizedTime;
						resultStep = stateInfo.normalizedTime % 1 >= conditions[index].value;
						break;
					case Condition.Type.AfterNormalizedTime:
						resultStep = stateInfo.normalizedTime >= conditions[index].value;
						break;

					case Condition.Type.AfterFixedTime:
						resultStep = data.fixedTime >= conditions[index].value;
						break;

					case Condition.Type.AfterNormalizedTimeStartTransition:
						resultStep = now >= State._2_Updating || (now == State._1_StartTransitioning && animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime >= conditions[index].value);
						break;
					case Condition.Type.AfterNormalizedTimeExitTransition:
						resultStep = now == State._4_ExitLastFrame || (now == State._3_ExitTransitioning && animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime >= conditions[index].value);
						break;

					// PARAM CONDITIONS
					case Condition.Type.ParameterEquals: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) == conditions[index].value;
						conditions[index] = c;
						break;
					}
					case Condition.Type.ParameterGreaterThanOrEquals: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) >= conditions[index].value;
						conditions[index] = c;
						break;
					}
					case Condition.Type.ParameterGreaterThan: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) > conditions[index].value;
						conditions[index] = c;
						break;
					}
					case Condition.Type.ParameterLessThan: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) < conditions[index].value;
						conditions[index] = c;
						break;
					}
					case Condition.Type.ParameterLessThanOrEquals: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) <= conditions[index].value;
						conditions[index] = c;
						break;
					}
					case Condition.Type.ParameterNotEquals: {
						var c = conditions[index];
						resultStep = GetParamValue(animator, ref c.parameterType, conditions[index].parameterHash) != conditions[index].value;
						conditions[index] = c;
						break;
					}

					// LAYER CONDITIONS
					case Condition.Type.LayerEquals: {
						resultStep = layerIndex == conditions[index].parameterHash;
						break;
					}
					case Condition.Type.LayerGreaterThanOrEquals: {
						resultStep = layerIndex >= conditions[index].parameterHash;
						break;
					}
					case Condition.Type.LayerGreaterThan: {
						resultStep = layerIndex > conditions[index].parameterHash;
						break;
					}
					case Condition.Type.LayerLessThan: {
						resultStep = layerIndex < conditions[index].parameterHash;
						break;
					}
					case Condition.Type.LayerLessThanOrEquals: {
						resultStep = layerIndex <= conditions[index].parameterHash;
						break;
					}
					case Condition.Type.LayerNotEquals: {
						resultStep = layerIndex != conditions[index].parameterHash;
						break;
					}

					// LAYER WEIGHT CONDITIONS
					case Condition.Type.LayerWeightEquals: {
						resultStep = animator.GetLayerWeight(layerIndex) == conditions[index].value;
						break;
					}
					case Condition.Type.LayerWeightGreaterThanOrEquals: {
						resultStep = animator.GetLayerWeight(layerIndex) >= conditions[index].value;
						break;
					}
					case Condition.Type.LayerWeightGreaterThan: {
						resultStep = animator.GetLayerWeight(layerIndex) > conditions[index].value;
						break;
					}
					case Condition.Type.LayerWeightLessThan: {
						resultStep = animator.GetLayerWeight(layerIndex) < conditions[index].value;
						break;
					}
					case Condition.Type.LayerWeightLessThanOrEquals: {
						resultStep = animator.GetLayerWeight(layerIndex) <= conditions[index].value;
						break;
					}
					case Condition.Type.LayerWeightNotEquals: {
						resultStep = animator.GetLayerWeight(layerIndex) != conditions[index].value;
						break;
					}

					// BEFORE (Equivalent to NOT AFTER, but easier to understand)
					case Condition.Type.BeforeEnterEnd:
					case Condition.Type.BeforeExitStart:
					case Condition.Type.BeforeNormalizedTime:
					case Condition.Type.BeforeFixedTime:
					case Condition.Type.BeforeNormalizedTimeStartTransition:
					case Condition.Type.BeforeNormalizedTimeExitTransition:
					case Condition.Type.BeforeNormalizedTimePerLoop: {
						// Small hack to reuse logic from Condition.Type.AfterEtc...
						int fakeIndex = -1;
						var tmp = conditions[index];
						tmp.type -= Condition.BeforeCondTypeOffset;
						TmpConditionsForBeforeCondTypes[0] = tmp;
						resultStep = !RecursiveEvaluateConditions(entry, TmpConditionsForBeforeCondTypes, ref fakeIndex, animator, layerIndex, ref stateInfo, prev, now, ref data, ref entryData, groupOr, false);
						break;
					}


					// DELEGATE CONDITION
					case Condition.Type.DelegateCondition:
						var d = conditions[index].conditionDelegate;
						if (d == null) throw new Exception("Trying to evaluate a NULL delegate condition.");
						resultStep = d.Invoke(animator, layerIndex, stateInfo, prev, now, data, entryData);
						break;

					default:
						throw new Exception("Condition type not found.");
				}

				if (debugResult) DebugResult(conditions, indexNow, resultStep);

				result = groupOr ? (result | resultStep) : (result & resultStep);
			}
			return result;
		}

		// Before condition types are equal to NOT + After condition types. This List is used as a hack to reuse the logic of the After cond. type.
		static List<Condition> TmpConditionsForBeforeCondTypes = new List<Condition>() { default(Condition) };

		[System.Diagnostics.Conditional("DEBUG")]
		void DebugResult(List<Condition> conditions, int index, bool resultStep) {
#if DEBUG && UNITY_EDITOR
			var tmp = conditions[index];
			Extensions.AddLast(ref tmp.debugLastCheckWasTrue, resultStep ? 1 : 0);
			conditions[index] = tmp;
#endif
		}

		private void TryExecute(Entry entry, Animator animator, int layerIndex, ref AnimatorStateInfo stateInfo, ref Data data, ref Entry.Data entryData) {
			//Debug.LogWarning("Condition ok: " + entry.conditions.ToStringFull(), this);

			CurrentlyExecutingEntry = entry;
			CurrentlyExecutingEventSMB = this;

			// animator.GetLayerWeight(0); always returns 0 but it should be always 1
			float layerWeight = layerIndex == 0 ? 1 : animator.GetLayerWeight(layerIndex);

			for (int i = 0; i < entry.actions.Count; i++) {
				try {
#if DEBUG && UNITY_EDITOR
					Action action = entry.actions[i];
					action.
#else
					entry.actions[i].
#endif
						Execute(this, layerWeight, animator);
#if DEBUG && UNITY_EDITOR
					entry.actions[i] = action;
#endif
				}
				catch (Exception e) {
					var stackTrace = e.StackTrace;
					int endFirstLine = stackTrace.IndexOf("\n");
					int firstAnimatorEventsErrorPointer = stackTrace.IndexOf("Ashkatchap.AnimatorEvents");
					bool errorIsAnimatorEventsFault = firstAnimatorEventsErrorPointer != -1 && firstAnimatorEventsErrorPointer < endFirstLine;
					if (errorIsAnimatorEventsFault) {
						Debug.LogError("Error executing action number " + i + " \"" + entry.actions[i].ToString() + "\" in EventSMB in \"" + Extensions.GetFullPath(animator) + "\". Condition: \"" + entry.conditions.ToStringFull(animator.parameters) + "\". ConditionExtra: \"" + entry.conditionsExtra.ToStringFull(animator.parameters) + "\". This can happen if the AnimatorEvent component isn't enabled. Please check the full StackTrace.", this);
						var clips = animator.GetCurrentAnimatorClipInfo(layerIndex);
						for (int j = 0; j < clips.Length; j++) {
							Debug.LogError("Clip playing: " + clips[j].clip.name + " with weight " + clips[j].weight);
						}
					}
					Debug.LogException(e);
				}
			}

			entryData.lastExecutedFixedTime = data.fixedTime;
			entryData.lastExecutedNormalizedTime = stateInfo.normalizedTime;
			entryData.executionCountSinceStart++;
			entryData.executionCountSinceLoop++;

			CurrentlyExecutingEntry = null;
			CurrentlyExecutingEventSMB = null;
		}

		private void SetParamValue(Animator animator, double value, ref AnimatorControllerParameterType parameterType, int parameterHash) {
			ParameterTypeGet(ref parameterType, animator, parameterHash);
			switch (parameterType) {
				case AnimatorControllerParameterType.Trigger:
					if (value > 0) animator.SetTrigger(parameterHash); else animator.ResetTrigger(parameterHash); break;
				case AnimatorControllerParameterType.Bool:
					animator.SetBool(parameterHash, value > 0); break;
				case AnimatorControllerParameterType.Int:
					animator.SetInteger(parameterHash, (int) Math.Round(value)); break;
				case AnimatorControllerParameterType.Float:
					animator.SetFloat(parameterHash, (float) value); break;
			}
		}

		private double GetParamValue(Animator animator, ref AnimatorControllerParameterType parameterType, int parameterHash) {
			ParameterTypeGet(ref parameterType, animator, parameterHash);
			switch (parameterType) {
				case AnimatorControllerParameterType.Trigger:
				case AnimatorControllerParameterType.Bool:
					return animator.GetBool(parameterHash) ? 1 : 0;
				case AnimatorControllerParameterType.Int:
					return animator.GetInteger(parameterHash);
				case AnimatorControllerParameterType.Float:
					return animator.GetFloat(parameterHash);
				default: throw new Exception("Invalid Parameter Type in Animator. Parameter hash: " + parameterHash);
			}
		}

		private void ParameterTypeGet(ref AnimatorControllerParameterType _type, Animator animator, int parameterHash) {
			if (_type == 0) {
				if (parametersCache == null) {
					parametersCache = animator.GetComponent<AnimatorParametersCache>();
					if (parametersCache == null) parametersCache = animator.gameObject.AddComponent<AnimatorParametersCache>();
				}
				if (!parametersCache.TryGetParameterType(parameterHash, out _type)) {
					throw new Exception("parameter not found in AnimatorEvent, hash " + parameterHash);
				}
			}
		}
	}

	public static partial class Extensions {
		static string[] POpen = new string[] { "(", "{", "[" };
		static string[] PClose = new string[] { ")", "}", "]" };

		public static string ToStringFull(this IList<EventSMB.Condition> conditions, AnimatorControllerParameter[] parameters, bool isDark = false) {
			if (conditions.Count == 0) {
				return "Always";
			}
			else {
				var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
				System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
				StringBuilder conditionsStr = new StringBuilder();
				int index = -1;
				conditionsStr.Length = 0;
				Recursive(conditionsStr, parameters, conditions, ref index, isDark);
				System.Threading.Thread.CurrentThread.CurrentCulture = culture;
				return conditionsStr.ToString();
			}
		}

		static string ParamColor(bool isDark) {
			return isDark ? "#c4c4ff" : "#6464dd";
		}

		static void Recursive(StringBuilder str, AnimatorControllerParameter[] parameters, IList<EventSMB.Condition> conditions, ref int index, bool isDark, bool groupOr = false, int rLevel = 0) {
			string pOpen = POpen[Mathf.Min(rLevel, POpen.Length - 1)];
			string pClose = PClose[Mathf.Min(rLevel, PClose.Length - 1)];
			
			while (++index < conditions.Count) {
				var elem = conditions[index];
				switch (elem.type) {
					// OPERANDS
					case EventSMB.Condition.Type.PARENTHESIS_CLOSE:
						return;
					case EventSMB.Condition.Type.AND:
						Operand(str, 0, isDark);
						str.Append(pOpen);
						Recursive(str, parameters, conditions, ref index, isDark, groupOr, rLevel + 1);
						str.Append(pClose);
						continue;
					case EventSMB.Condition.Type.OR:
						Operand(str, 1, isDark);
						str.Append(pOpen);
						Recursive(str, parameters, conditions, ref index, isDark, groupOr, rLevel + 1);
						str.Append(pClose);
						continue;
					case EventSMB.Condition.Type.NOT:
						Operand(str, 2, isDark);
						str.Append(pOpen);
						Recursive(str, parameters, conditions, ref index, isDark, groupOr, rLevel + 1);
						str.Append(pClose);
						break;

					// OPERANDS 2
					case EventSMB.Condition.Type.AND_GROUP:
						str.Append(pOpen);
						Recursive(str, parameters, conditions, ref index, isDark, false, rLevel + 1);
						str.Append(pClose);
						break;
					case EventSMB.Condition.Type.OR_GROUP:
						str.Append(pOpen);
						Recursive(str, parameters, conditions, ref index, isDark, true, rLevel + 1);
						str.Append(pClose);
						break;


					// CONDITIONS
					case EventSMB.Condition.Type.MaxNTimesAfterStart:
						str.Append("Max <b>").Append(elem.value).Append("</b> Times after start");
						break;
					case EventSMB.Condition.Type.MaxNTimesPerLoop:
						str.Append("Max <b>").Append(elem.value).Append("</b> Times per loop");
						break;

					case EventSMB.Condition.Type.OnNormalizedTimePerLoop:
						str.Append("On <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time <i>Looping</i> ");
						break;
					case EventSMB.Condition.Type.OnNormalizedTime:
						str.Append("On <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time");
						break;
					case EventSMB.Condition.Type.AfterNormalizedTime:
						str.Append("After <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time");
						break;
					case EventSMB.Condition.Type.AfterNormalizedTimePerLoop:
						str.Append("After <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time <i>Looping</i> ");
						break;
					case EventSMB.Condition.Type.AfterFixedTime:
						str.Append("After <b>").Append(elem.value.ToString("0.00")).Append("</b> fixed time");
						break;

					case EventSMB.Condition.Type.AfterNormalizedTimeStartTransition:
						str.Append("After <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized start transition time");
						break;
					case EventSMB.Condition.Type.AfterNormalizedTimeExitTransition:
						str.Append("After <b>").Append(elem.value).Append("</b> normalized exit transition time");
						break;

					case EventSMB.Condition.Type.AfterEnterEnd:
						str.Append("After enter transition ends");
						break;
					case EventSMB.Condition.Type.AfterExitStart:
						str.Append("After exit transition starts");
						break;
					case EventSMB.Condition.Type.OnExitEnd:
						str.Append("On exit transition ends");
						break;
					case EventSMB.Condition.Type.OnEnterStart:
						str.Append("On enter transition starts");
						break;
					case EventSMB.Condition.Type.OnEnterEnd:
						str.Append("On enter transition ends");
						break;
					case EventSMB.Condition.Type.OnExitStart:
						str.Append("On exit transition starts");
						break;

					// PARAM CONDITIONS
					case EventSMB.Condition.Type.ParameterEquals: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" = <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.ParameterGreaterThanOrEquals: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" ≥ <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.ParameterGreaterThan: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" > <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.ParameterLessThan: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" < <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.ParameterLessThanOrEquals: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" ≤ <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.ParameterNotEquals: {
						var c = conditions[index];
						var p = FindParam(parameters, c.parameterHash);
						str.Append("\"<color=").Append(ParamColor(isDark)).Append("><b>").Append(p == null ? "NOT FOUND" : p.name).Append("</b></color>\" ≠ <b>").Append(c.value).Append("</b>");
						break;
					}

					// LAYER CONDITIONS
					case EventSMB.Condition.Type.LayerEquals: {
						var c = conditions[index];
						str.Append("Layer = <b>").Append(c.parameterHash).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerGreaterThanOrEquals: {
						var c = conditions[index];
						str.Append("Layer ≥ <b>").Append(c.parameterHash).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerGreaterThan: {
						var c = conditions[index];
						str.Append("Layer > <b>").Append(c.parameterHash).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerLessThan: {
						var c = conditions[index];
						str.Append("Layer < <b>").Append(c.parameterHash).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerLessThanOrEquals: {
						var c = conditions[index];
						str.Append("Layer ≤ <b>").Append(c.parameterHash).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerNotEquals: {
						var c = conditions[index];
						str.Append("Layer ≠ <b>").Append(c.parameterHash).Append("</b>");
						break;
					}

					// LAYER WEIGHT CONDITIONS
					case EventSMB.Condition.Type.LayerWeightEquals: {
						var c = conditions[index];
						str.Append("Layer weight = <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerWeightGreaterThanOrEquals: {
						var c = conditions[index];
						str.Append("Layer weight ≥ <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerWeightGreaterThan: {
						var c = conditions[index];
						str.Append("Layer weight > <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerWeightLessThan: {
						var c = conditions[index];
						str.Append("Layer weight < <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerWeightLessThanOrEquals: {
						var c = conditions[index];
						str.Append("Layer weight ≤ <b>").Append(c.value).Append("</b>");
						break;
					}
					case EventSMB.Condition.Type.LayerWeightNotEquals: {
						var c = conditions[index];
						str.Append("Layer weight ≠ <b>").Append(c.value).Append("</b>");
						break;
					}

					case EventSMB.Condition.Type.BeforeNormalizedTime:
						str.Append("Before <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time");
						break;
					case EventSMB.Condition.Type.BeforeNormalizedTimePerLoop:
						str.Append("Before <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized time <i>Looping</i> ");
						break;
					case EventSMB.Condition.Type.BeforeFixedTime:
						str.Append("Before <b>").Append(elem.value.ToString("0.00")).Append("</b> fixed time");
						break;

					case EventSMB.Condition.Type.BeforeNormalizedTimeStartTransition:
						str.Append("Before <b>").Append(elem.value.ToString("0.00")).Append("</b> normalized start transition time");
						break;
					case EventSMB.Condition.Type.BeforeNormalizedTimeExitTransition:
						str.Append("Before <b>").Append(elem.value).Append("</b> normalized exit transition time");
						break;

					case EventSMB.Condition.Type.BeforeEnterEnd:
						str.Append("Before enter transition ends");
						break;
					case EventSMB.Condition.Type.BeforeExitStart:
						str.Append("Before exit transition starts");
						break;

					default:
						// TO DO: Add remaining cases
						str.Append("<b>").Append(Regex.Replace(elem.type.ToString(), "(\\B[A-Z])", " $1")).Append("</b>");
						break;
				}

				if (index + 1 < conditions.Count) {
					var elem2 = conditions[index + 1];
					if (elem2.type != EventSMB.Condition.Type.PARENTHESIS_CLOSE &&
						elem2.type != EventSMB.Condition.Type.OR &&
						elem2.type != EventSMB.Condition.Type.AND) {
						Operand(str, groupOr ? 1 : 0, isDark);
					}
				}
			}
		}

		static AnimatorControllerParameter FindParam(AnimatorControllerParameter[] parameters, int hash) {
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters[i].nameHash == hash) return parameters[i];
			}
			return null;
		}

		static void Operand(StringBuilder str, int id, bool isDark) {
			str.Append("<color=").Append(isDark ?
				id == 0 ? "#00ff00" : id == 1 ? "#ffa54f" : "#ff5555" :
				id == 0 ? "#009900" : id == 1 ? "#888811" : "#ff2222"
			).Append("><b> ").Append(id == 0 ? "AND\n" : id == 1 ? "OR" : "NOT").Append(" </b></color>");
		}

		public static string GetFullPath(Animator animator) {
			Transform t = animator.transform;
			string s = t.name;
			while (t.parent != null) {
				t = t.parent;
				s = t.name + '/' + s;
			}
			return s;
		}

#if DEBUG && UNITY_EDITOR
		public struct DebugData<T> {
			public T value;
			public int frameCount;

			public DebugData(T value, int frameCount) {
				this.value = value;
				this.frameCount = frameCount;
			}
		}
		public static void AddLast<T>(ref List<DebugData<T>> list, T value) {
			if (list == null) list = new List<DebugData<T>>();
			AnimatorEvent.DebugArraysToClearEveryFrame.Add(list);
			list.Add(new DebugData<T>(value, Time.frameCount));
		}
		public static void SetLast<T>(ref List<DebugData<T>> list, T value) {
			if (list == null) list = new List<DebugData<T>>();
			list[list.Count - 1] = new DebugData<T>(value, list[list.Count - 1].frameCount);
		}
#endif
	}

	public struct TinyArray2<T> {
		public T e0, e1;

		public T this[int index] {
			get {
				if (index == 0) return e0;
				if (index == 1) return e1;
				throw new ArgumentOutOfRangeException(index.ToString());
			}
			set {
				if (index == 0) { e0 = value; return; }
				if (index == 1) { e1 = value; return; }
				throw new ArgumentOutOfRangeException(index.ToString());
			}
		}
	}
}
