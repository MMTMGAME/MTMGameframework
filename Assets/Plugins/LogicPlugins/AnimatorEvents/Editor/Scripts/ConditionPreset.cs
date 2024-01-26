using System.Collections.Generic;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	[CreateAssetMenu(fileName = "EventSMB Condition Preset", menuName = "EventSMB Condition Preset", order = 406)]
	public class ConditionPreset : ScriptableObject {
		public List<EventSMB.Condition> conditions;
	}
}
