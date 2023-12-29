#if UNITY_EDITOR
//#define DEBUG_AE
#endif

using UnityEngine;
using UnityEngine.Profiling;

namespace Ashkatchap.AnimatorEvents {
	/// <summary>
	/// Extented logic for StateMachineBehaviour
	/// </summary>
	// `dataIndex` can have values 0 and 1 only. It represents data specific to a running state, to support states transitioning to themselves.
	// DO NOT USE: OnStateMachineEnter or OnStateMachineExit. Transitions that don't got through SubStateMachine's Enter or Exit states won't trigger this events (like transitions to/from AnyState inside SubStateMachine). Sealed to avoid future problems
	public abstract class StateMachineBehaviourExtended : StateMachineBehaviour {
		public enum State : byte {
			_0_NotPlaying,
			_1_StartTransitioning,
			_2_Updating,
			_3_ExitTransitioning,
			_4_ExitLastFrame
		}

		private struct Group {
			/// <summary>
			/// 0 or 1, can't take other values
			/// </summary>
			public byte dataIndex;
			public State prevFrame, thisFrame;

			public void Advance(State newState) {
				prevFrame = thisFrame;
				thisFrame = newState;
				//Debug.Log("Advance called. " + prevFrame + " / " + thisFrame);
			}

#if DEBUG_AE
			public string debugName;
			public override string ToString() {
				return "[" +
					"(" + debugName + ") " +
					"(data " + dataIndex + ") " + prevFrame + " / " + thisFrame + "]";
			}
#endif
		}

		private Group stateNow, statePrev;
		private bool pendingInterruptToSelf;

		protected abstract void InitData(byte dataIndex);
#if DEBUG_AE
		protected abstract string ToStringData(byte dataIndex);
#endif

		/// <summary>
		/// Called every update frame, including start and last frames
		/// </summary>
		/// <param name="stateDataIndex">either 0 or 1. Needed to handle transition to self. points to an array with 2 elements where each one is data used for the current playing state, considering that when transitioning to self, each play state is considered it's own state that needs its own data.</param>
		public virtual void StateUpdate(Animator animator, ref AnimatorStateInfo stateInfo, int layerIndex, State prevFrame, State thisFrame, byte stateDataIndex) { }

		public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
#if DEBUG_AE
			Debug.LogWarning("OnStateEnter. " + stateInfo.ToStringFull());
#endif
			if (pendingInterruptToSelf) {
				pendingInterruptToSelf = false;
			}
			else {
				statePrev = stateNow;
			}
			stateNow = default(Group);
			stateNow.dataIndex = (byte) ((~statePrev.dataIndex) & 1);

			InitData(stateNow.dataIndex);
#if DEBUG_AE
			statePrev.debugName = "statePrev";
			stateNow.debugName = "stateNow";
#endif

			stateNow.Advance(animator.IsInTransition(layerIndex) ? 
				State._1_StartTransitioning :
				State._2_Updating);

			DebugGenericUpdate(this, animator, layerIndex, stateInfo, stateNow);

			StateUpdate(animator, ref stateInfo, layerIndex, stateNow.prevFrame, stateNow.thisFrame, stateNow.dataIndex);

			stateNow.Advance(stateNow.thisFrame);
			firstUpdateDone = false;
		}

		public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
#if DEBUG_AE
			Debug.LogWarning("OnStateExit. " + stateInfo.ToStringFull());
#endif

			if (statePrev.thisFrame == State._0_NotPlaying) {
				// Going to another state (not to itself).
				OnStateExit2(animator, ref stateInfo, layerIndex, ref stateNow);
			}
			else if (animator.IsInTransition(layerIndex)) {
				// Transition to self, after being interrupted. Exit now.
				pendingInterruptToSelf = true;
				OnStateExit2(animator, ref stateInfo, layerIndex, ref stateNow);
			}
			else {
				// Transition to self, normally transitioning.
				OnStateExit2(animator, ref stateInfo, layerIndex, ref statePrev);
			}
		}

		private void OnStateExit2(Animator animator, ref AnimatorStateInfo stateInfo, int layerIndex, ref Group group) {
			group.Advance(State._4_ExitLastFrame);

			DebugGenericUpdate(this, animator, layerIndex, stateInfo, group);

			StateUpdate(animator, ref stateInfo, layerIndex, group.prevFrame, group.thisFrame, group.dataIndex);
			group.Advance(State._0_NotPlaying);
			// DestroyData would be called here (to destroy data created in InitData), but it's not needed because there's no data that needs to be destroyed
		}

		private bool firstUpdateDone;
		public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
#if DEBUG_AE
			Debug.LogWarning("OnStateUpdate. " + stateInfo.ToStringFull());
#endif
			// When transitioning to self, we use statePrev and stateCur
			// This is called twice per update while transitioning to itself
			// First call is for the oldest state (statePrev), second call is for the newest one (stateCur)
			// But the order is reversed when both states are the same

			if (statePrev.thisFrame == State._3_ExitTransitioning) {
				// Callign 2 times, is this the first or the second?
				// Don't use Time.unscaledTime to test if this was called before this update because it doesn't reflect when animator.Update is used instead of letting the animator run ingame.
				if (!firstUpdateDone) {
					// First call. Use previous
					AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
					OnStateUpdate2(animator, ref info, layerIndex, ref statePrev);
				}
				else {
					// Second call, Use current
					AnimatorStateInfo info = animator.GetNextAnimatorStateInfo(layerIndex);
					OnStateUpdate2(animator, ref info, layerIndex, ref stateNow);
				}
				firstUpdateDone = !firstUpdateDone;
			}
			else {
				OnStateUpdate2(animator, ref stateInfo, layerIndex, ref stateNow);
			}
		}

		private void OnStateUpdate2(Animator animator, ref AnimatorStateInfo stateInfo, int layerIndex, ref Group group) {
			// Analysis:
			// - If a `Enter Transition` is playing, `Exit Transition CANNOT start` (normally)
			// - If an interrupt is set, `Enter Transition` can be interrupted, but directly with `Exit` without `Exit Transition`
			// - If there's a transition that will be taken on the same frame that an `Enter Transition` ends, it takes 1 frame. First frame, `Exit` is called on old state, and `Update` is called on the new state while `IsInTransition` is false. Next frame, a new transition starts

			group.Advance(
				group.thisFrame == State._1_StartTransitioning && !animator.IsInTransition(layerIndex) ? State._2_Updating :
				group.thisFrame == State._2_Updating && animator.IsInTransition(layerIndex) ? State._3_ExitTransitioning :
				group.thisFrame // Repeat current mode
			);

			DebugGenericUpdate(this, animator, layerIndex, stateInfo, group);
			
			StateUpdate(animator, ref stateInfo, layerIndex, group.prevFrame, group.thisFrame, group.dataIndex);
		}


		[System.Diagnostics.Conditional("DEBUG_AE")]
		static void DebugGenericUpdate(StateMachineBehaviourExtended smb, Animator animator, int layerIndex, AnimatorStateInfo stateInfo, Group group) {
#if DEBUG_AE
			Debug.LogWarning("Generic Update. " + stateInfo.ToStringFull() + ". " + group.ToString() + ". Transition (" + animator.IsInTransition(layerIndex) + " | " + animator.GetAnimatorTransitionInfo(layerIndex).ToStringFull() + "). Data: " + smb.ToStringData(group.dataIndex));
#endif
		}
	}

	public static partial class Extensions {
		public static string ToStringFull(this AnimatorStateInfo info) {
			return "{Hash: " + info.fullPathHash + ", Animation progress: " + (info.normalizedTime * 100).ToString("0.00") + "%}";
		}
		public static string ToStringFull(this AnimatorClipInfo info) {
			return info.clip.name + " (" + (info.weight * 100).ToString("0.00") + "%)";
		}
		public static string ToStringFull(this AnimatorTransitionInfo info) {
			string t = (info.normalizedTime * 100).ToString("0.00");
			return "[" + t + "%, AnyState: " + info.anyState + ", fullPathHash: " + info.fullPathHash + ", duration: " + info.duration + ", durationUnit: " + info.durationUnit + "]";
		}
	}
}
