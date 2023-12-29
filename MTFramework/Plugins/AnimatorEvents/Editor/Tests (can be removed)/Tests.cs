#if TEST_FRAMEWORK
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashkatchap.AnimatorEvents {
	public class Tests {
		const string NoState = "New State";

#pragma warning disable 0414
		const string NoAnimation1 = "No Animation 1";
		const string NoAnimation2 = "No Animation 2";
		const string AnimationLoop1_1 = "Animation Loop 1s 1";
		const string AnimationLoop1_2 = "Animation Loop 1s 2";
		const string AnimationLoop10_1 = "Animation Loop 10s 1";
		const string AnimationLoop10_2 = "Animation Loop 10s 2";
		const string AnimationNoLoop1 = "Animation NoLoop 1";
		const string AnimationNoLoop2 = "Animation NoLoop 2";
		const string AnimationBlendTree = "Blend Tree";

		private static readonly int ToSelf_0 = Animator.StringToHash("toSelf_0");
		private static readonly int ToSelf_05 = Animator.StringToHash("toSelf_0.5");
		private static readonly int ToSelf_1 = Animator.StringToHash("toSelf_1");
		private static readonly int ToSelf_5 = Animator.StringToHash("toSelf_5");

		private static readonly int Tree0 = Animator.StringToHash("Tree0");
		private static readonly int Tree1 = Animator.StringToHash("Tree1");
		private static readonly int Tree2 = Animator.StringToHash("Tree2");

		private static readonly int ParamF = Animator.StringToHash("Float");
		private static readonly int ParamI = Animator.StringToHash("Int");
		private static readonly int ParanB = Animator.StringToHash("Bool");
		private static readonly int ParamT = Animator.StringToHash("Trigger");
#pragma warning restore 0414


		private Scene scene;
		private Animator animator;
		private Dictionary<string, EventSMB> states;
		private readonly EventSMB.Action testAction = new EventSMB.Action(() => calls++);

		private static int _calls;
		private static int calls {
			get { return _calls; }
			set {
				_calls = value;
				Debug.LogWarning("Calls set to " + _calls);
			}
		}

		[SetUp]
		public void Setup() {
			string sceneID = AssetDatabase.FindAssets("AnimatorEvents Test scene")[0];
			string scenePath = AssetDatabase.GUIDToAssetPath(sceneID);
			scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

			animator = GameObject.Find("Animator in AnimatorEvents Test scene").GetComponent<Animator>();

			SoftReset();
		}

		void SoftReset() {
			// The animator may have changed, so old EventSMB may not be valid
			var eventSMBs = animator.GetBehaviours<EventSMB>();
			if (states == null || (eventSMBs[0].entries.Count > 0 && eventSMBs[0].entries[0].actions[0].type == EventSMB.Action.Type.SendMessage)) {
				// Create dictionary of states and reset actions
				states = new Dictionary<string, EventSMB>();
				foreach (var elem in animator.GetBehaviours<EventSMB>()) {
					states.Add(elem.entries[0].actions[0].functionName, elem);
					elem.entries.Clear();
				}
			}
			else {
				// Remove any existing event
				foreach (var state in states) {
					state.Value.entries.Clear();
				}
			}


			// Reset parameters
			foreach (var elem in animator.parameters) {
				switch (elem.type) {
					case AnimatorControllerParameterType.Bool: animator.SetBool(elem.nameHash, false); break;
					case AnimatorControllerParameterType.Trigger: animator.ResetTrigger(elem.nameHash); break;
					case AnimatorControllerParameterType.Int: animator.SetInteger(elem.nameHash, 0); break;
					case AnimatorControllerParameterType.Float: animator.SetFloat(elem.nameHash, 0); break;
				}
			}
			// Queue next wanted state. CrossFade also queues the desired state to be set as current after next Update()
			animator.Play(NoState);
			animator.Update(0);
			calls = 0;
		}

		[TearDown]
		public void TearDown() {
			EditorSceneManager.CloseScene(scene, true);
		}

		/// <summary>
		/// Advance the time for the current state. Notice that calling Play or Crossfade only queues the state, so
		/// calling this just after those methods will set the new state to the queued state and not advance it.
		/// </summary>
		/// <param name="deltaTime">How much time to advance the animator forward.</param>
		/// <param name="deltaCalls">How many new calls will be executed after updating the animator.</param>
		public void AnimatorUpdate(float deltaTime, int deltaCalls) {
			string faultyLine = StackTraceUtility.ExtractStackTrace().Split('\n')[1];
			faultyLine = faultyLine.Substring(faultyLine.LastIndexOf(':') + 1);
			faultyLine = "Line: " + faultyLine.Substring(0, faultyLine.Length - 1);
			if (deltaTime == 0) {
				Debug.LogError("(Line " + faultyLine + ") Advancing the animator by " + 0 + " is not allowed. The result of that operations is not always the same as it depends on an internal state.");
			}
			//Debug.Log("----- Before Update (DeltaTime: " + deltaTime + ") (Line " + faultyLine + ") -----");
			int prevCallValue = calls;
			// Update animator, will set queued state as current state. If a transition starts, transition normalized time will be 0
			animator.Update(deltaTime);

			int actual = calls - prevCallValue;

			if (deltaCalls != actual) {
				Debug.LogError("NO at Line " + faultyLine + ". [Expected=" + deltaCalls + "] [Actual=" + actual + "] [Current time: " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString("G9") + "] [Next time: " + animator.GetNextAnimatorStateInfo(0).normalizedTime.ToString("G9") + "]");
				animator.speed = 0;
				throw new Exception("Stop");
			}
			else {
				//Debug.Log("Ok. [Expected=" + expected + "] [Actual=" + actual + "] [Current time: " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString("G9") + "] [Next time: " + animator.GetNextAnimatorStateInfo(0).normalizedTime.ToString("G9") + "]");
			}
		}

		#region Basic conditions + StateMachineBehaviourExtended
		[Test]
		public void TestCondition_OnceAfterStart() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.MaxNTimesAfterStart, 1) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);

			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(1f, 0);
			AnimatorUpdate(2f, 0);

			animator.Play(NoState);
			AnimatorUpdate(1f, 0);
			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);

			// Test transition to self
			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.9f, 0);

			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.9f, 0);

			animator.SetTrigger(ToSelf_05);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);

			// Test interrupt with transition to self
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.25f, 0);
			animator.SetTrigger(ToSelf_05);
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(1f, 0);
			AnimatorUpdate(1f, 0);
		}
		[Test]
		public void TestCondition_SeveralTimesAfterStart() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.MaxNTimesAfterStart, 5) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(1f, 1);
			AnimatorUpdate(2f, 1);

			animator.Play(NoState);
			AnimatorUpdate(1f, 0);
			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);

			// Test transition to self
			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 2);
			AnimatorUpdate(1f, 1);

			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 2);
			AnimatorUpdate(1f, 1);

			animator.SetTrigger(ToSelf_05);
			AnimatorUpdate(0.01f, 2);
			AnimatorUpdate(0.25f, 2);
			AnimatorUpdate(1f, 2);
			AnimatorUpdate(1f, 1);

			// Test interrupt with transition to self
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.01f, 2);
			AnimatorUpdate(0.25f, 1);
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.125f, 2);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(5f, 0);


			// Transition to something, then interrupt, then interrupt again.
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.25f, 1); // 1
			AnimatorUpdate(0.25f, 1); // 2

			// Test interrupt with transition to self, with prevState still alive
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.25f, 2); // 3 | 1
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.25f, 2); // 2 | 1
			AnimatorUpdate(0.25f, 1); // 2
			AnimatorUpdate(0.25f, 1); // 3
			AnimatorUpdate(0.25f, 1); // 4
			AnimatorUpdate(0.25f, 1); // 5
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
		}
		[Test]
		public void TestCondition_OncePerLoop() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.MaxNTimesPerLoop, 1) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);

			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);

			AnimatorUpdate(0.25f, 1);

			AnimatorUpdate(1f, 1);
			AnimatorUpdate(1f - 0.001953125f, 0);

			AnimatorUpdate(0.001953125f, 1);



			animator.Play(NoState);
			AnimatorUpdate(0.01f, 0);
			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.25f, 0);



			animator.Play(NoState);
			// Previous state will keep advancing for the last frame, so call will increase again before the state exits
			AnimatorUpdate(0.75f, 1);

			//AnimatorUpdate(0);
			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.75f, 1);
			AnimatorUpdate(1f, 1);
			// Advance two ore more loops, but they happen in a single frame, so the event happens only once
			AnimatorUpdate(2f, 1);
		}
		[Test]
		public void TestCondition_SeveralTimesPerLoop() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.MaxNTimesPerLoop, 5) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(1f, 1);
			AnimatorUpdate(2f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(1f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.01f, 0);

			animator.Play(NoState);
			AnimatorUpdate(1f, 1);
			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);

			// Test transition to self
			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 2);
			AnimatorUpdate(1f, 1);

			animator.SetTrigger(ToSelf_0);
			AnimatorUpdate(0.01f, 2); // ? | 1
			AnimatorUpdate(1f, 1); // 1

			animator.SetTrigger(ToSelf_05);
			AnimatorUpdate(0.01f, 2); // 2 | 1
			AnimatorUpdate(0.25f, 2); // 3 | 2
			AnimatorUpdate(1f, 2); // 1 | 1
			AnimatorUpdate(1f, 1); // 1

			// Test interrupt with transition to self
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.01f, 2); // 2 | 1
			AnimatorUpdate(0.25f, 2); // 3 | 2
			animator.SetTrigger(ToSelf_5);
			AnimatorUpdate(0.01f, 3); // 4 | 3 | 1
			AnimatorUpdate(0.01f, 2); // 5 | 2
			AnimatorUpdate(0.01f, 1); // 3
			AnimatorUpdate(0.01f, 1); // 4
			AnimatorUpdate(0.01f, 1); // 5
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(2f, 1); // 1
		}

		[Test]
		public void TestCondition_BeforeEnterEnd1() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() {
					new EventSMB.Condition(EventSMB.Condition.Type.NOT),
					new EventSMB.Condition(EventSMB.Condition.Type.AfterEnterEnd)
				},
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.CrossFade(AnimationLoop1_1, 0.5f, 0);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(1f, 0);
		}
		[Test]
		public void TestCondition_BeforeEnterEnd2() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() {
					new EventSMB.Condition(EventSMB.Condition.Type.NOT),
					new EventSMB.Condition(EventSMB.Condition.Type.AfterEnterEnd)
				},
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.CrossFade(AnimationLoop1_1, 0.5f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(1f, 0);
		}
		[Test]
		public void TestCondition_AfterEnterEnd() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterEnterEnd) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.CrossFade(AnimationLoop1_1, 0.5f, 0);
			AnimatorUpdate(0.005f, 0);
			AnimatorUpdate(0.120f, 0);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(1f, 1);
		}

		[Test]
		public void TestCondition_BeforeExitStart() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() {
					new EventSMB.Condition(EventSMB.Condition.Type.NOT),
					new EventSMB.Condition(EventSMB.Condition.Type.AfterExitStart)
				},
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.15f, 1);
			AnimatorUpdate(0.35f, 1);
			AnimatorUpdate(1f, 1);
			animator.Play(NoState);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);



			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(0.4f, 1);
			AnimatorUpdate(1f, 1);
			animator.CrossFade(NoState, 0.5f);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.15f, 0);
			// Last update for this state, no more increases to call after this last one
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
		}
		[Test]
		public void TestCondition_AfterExitStart() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterExitStart) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(1f, 0);
			animator.Play(NoState);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);



			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(1f, 0);
			animator.CrossFade(NoState, 0.5f);
			AnimatorUpdate(0.05f, 1);
			AnimatorUpdate(0.20f, 1);
			// Last update for this state, no more increases to call after this last one
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
		}

		[Test]
		public void TestCondition_OnExitEnd() {
			var smb = states[AnimationLoop1_1];
			smb.entries.Add(new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnExitEnd) },
				actions = new List<EventSMB.Action>() { testAction }
			});

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.49f, 0);
			AnimatorUpdate(1f, 0);
			animator.Play(NoState);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);



			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(0.4f, 0);
			AnimatorUpdate(1f, 0);
			animator.CrossFade(NoState, 0.5f);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.24f, 0);
			// Last update for this state, no more increases to call after this last one
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
		}

		[Test]
		public void TestCondition_AfterNormalizedTime1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterNormalizedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(10f, 0);
			AnimatorUpdate(10f, 0);
			AnimatorUpdate(5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterNormalizedTime2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterNormalizedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop0() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 0.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 0);

			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 0);

			AnimatorUpdate(0.125f, 0);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(10f, 1);

			AnimatorUpdate(10f, 1);

			AnimatorUpdate(5f, 0);
			AnimatorUpdate(2.5f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			//8.5

			AnimatorUpdate(5f, 0);
			//3.5
			AnimatorUpdate(5f, 1);
			//8.5
			AnimatorUpdate(1f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 0f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 1);

			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 1);

			AnimatorUpdate(0.125f, 0);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(10f, 1);
			AnimatorUpdate(10f, 1);
			AnimatorUpdate(5f, 0);
			AnimatorUpdate(5f, 1);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 1f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 1);

			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 1);

			AnimatorUpdate(0.125f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop3() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 1.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(1.5f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 0);

			AnimatorUpdate(1.5f, 0);
			AnimatorUpdate(0.25f, 1);
			AnimatorUpdate(0.25f, 0);
		}

		[Test]
		public void TestCondition_OnNormalizedTime0() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTime, 0.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.1f, 0);
			AnimatorUpdate(10f, 1);

			AnimatorUpdate(10f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTime1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTime, 0f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.1f, 1);
			AnimatorUpdate(10f, 0);

			AnimatorUpdate(10f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTime2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTime, 1f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);

			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.25f, 0);
			AnimatorUpdate(0.125f, 0);
			AnimatorUpdate(0.125f, 0);
		}

		[Test]
		public void TestCondition_AfterNormalizedTimeStartTransition1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterNormalizedTimeStartTransition, 0.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);

			animator.Play(AnimationLoop1_2);
			AnimatorUpdate(0.01f, 0);

			animator.CrossFadeInFixedTime(AnimationLoop1_1, 0.5f);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.125f, 0); // 0.125 / 0.5 / 0.75 = 0.333333
			AnimatorUpdate(0.125f, 0); // 0.25 / 0.5 / 0.75 = 0.666666
			AnimatorUpdate(0.125f, 1); // 0.375 / 0.5 / 0.75 = 1
			AnimatorUpdate(0.125f, 1);
			AnimatorUpdate(1f, 1);
			AnimatorUpdate(1f, 1);
		}
		[Test]
		public void TestCondition_AfterNormalizedTimeStartTransition2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterNormalizedTimeStartTransition, 0.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterNormalizedTimeExitTransition() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterNormalizedTimeExitTransition, 0.75f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop1_1].entries.Add(entry);

			animator.Play(AnimationLoop1_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);

			animator.CrossFadeInFixedTime(AnimationLoop1_2, 0.5f);
			AnimatorUpdate(0.125f, 0); // 0.125 / 0.5 / 0.75 = 0.333333
			AnimatorUpdate(0.125f, 0); // 0.25 / 0.5 / 0.75 = 0.666666
			AnimatorUpdate(0.125f, 1); // 0.375 / 0.5 / 0.75 = 1
			AnimatorUpdate(0.125f, 1); // OnExit
			AnimatorUpdate(1f, 0);
			AnimatorUpdate(1f, 0);
		}

		[Test]
		public void TestCondition_AfterFixedTime1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterFixedTime, 0f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterFixedTime2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterFixedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationLoop10_1].entries.Add(entry);

			animator.Play(AnimationLoop10_1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterFixedTime3() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterFixedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationBlendTree].entries.Add(entry);

			animator.Play(AnimationBlendTree);
			AnimatorUpdate(0.01f, 0);

			animator.SetFloat(Tree0, 0f);
			animator.SetFloat(Tree1, 0f);
			animator.SetFloat(Tree2, 0f);
			AnimatorUpdate(0.5f, 0);
			animator.SetFloat(Tree0, 2f);
			AnimatorUpdate(0.5f, 0);
			animator.SetFloat(Tree0, 3f);
			AnimatorUpdate(0.5f, 0);
			animator.SetFloat(Tree1, 1f);
			AnimatorUpdate(0.5f, 0);
			animator.SetFloat(Tree0, 4f);
			animator.SetFloat(Tree2, 1f);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterFixedTime4() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterFixedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationBlendTree].entries.Add(entry);

			animator.Play(AnimationBlendTree);
			AnimatorUpdate(0.01f, 0);

			animator.SetFloat(Tree0, 0f);
			animator.SetFloat(Tree1, 0f);
			animator.SetFloat(Tree2, 0f);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			// Even though this animation isn't looping, time still passes.
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_AfterFixedTime5() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.AfterFixedTime, 2.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			// Even though this animation isn't looping, time still passes.
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop_NoLoop_1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 0.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);

			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop_NoLoop_2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 1) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}
		[Test]
		public void TestCondition_OnNormalizedTimePerLoop_NoLoop_3() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnNormalizedTimePerLoop, 1.5f) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);

			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void Test_EnablingDisabling() {
			for (int i = 0; i < 5; i++) {
				TestCondition_OncePerLoop();
				animator.gameObject.SetActive(false);
				animator.gameObject.SetActive(true);

				SoftReset();
			}
		}



		[Test]
		public void TestCondition_OnEnterStart1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterStart) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterStart2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterStart) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop2].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterStart3() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterStart) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterStart4() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterStart) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			animator.CrossFade(AnimationNoLoop1, 1);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterStart5() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterStart) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			animator.CrossFade(AnimationNoLoop1, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}



		[Test]
		public void TestCondition_OnEnterEnd1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterEnd2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop2].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterEnd3() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterEnd4() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			animator.CrossFade(AnimationNoLoop1, 1);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterEnd5() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			animator.CrossFade(AnimationNoLoop1, 1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnEnterEnd6() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnEnterEnd) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			animator.CrossFade(AnimationNoLoop1, 1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 1);
			AnimatorUpdate(0.5f, 0);
		}



		[Test]
		public void TestCondition_OnExitStart1() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnExitStart) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 1);
		}

		[Test]
		public void TestCondition_OnExitStart2() {
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnExitStart) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);

			animator.CrossFade(AnimationNoLoop2, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnExitStart3() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnExitStart) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			animator.Play(AnimationNoLoop2);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}

		[Test]
		public void TestCondition_OnExitStart4() {
			object boolToReturn = false;
			var callback = new EventSMB.Condition.ConditionDelegate((Animator animator, int layerIndex, AnimatorStateInfo stateInfo, StateMachineBehaviourExtended.State prev, StateMachineBehaviourExtended.State now, EventSMB.Data data, EventSMB.Entry.Data entryData) => (bool) boolToReturn);
			var entry = new EventSMB.Entry() {
				conditions = new List<EventSMB.Condition>() { new EventSMB.Condition(EventSMB.Condition.Type.OnExitStart) },
				conditionsExtra = new List<EventSMB.Condition>() { new EventSMB.Condition(callback) },
				actions = new List<EventSMB.Action>() { testAction }
			};
			states[AnimationNoLoop1].entries.Add(entry);

			animator.Play(AnimationNoLoop1);
			AnimatorUpdate(0.01f, 0);
			boolToReturn = true;
			animator.CrossFade(AnimationNoLoop2, 1);
			AnimatorUpdate(0.01f, 1);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
			AnimatorUpdate(0.5f, 0);
		}








		#endregion
	}
}

/*
Findings:
- Animator.Play() is the same as Animator.CrossFade with 0 transition time.
- Animator.Play() and Animator.CrossFade() set a request to update the current state in the next fame. An Update will advance the previous state and then set the requested state
*/
#endif
