using System.Collections.Generic;
using UnityEngine;

namespace Ashkatchap.AnimatorEvents {
	public class AnimatorParametersCache : MonoBehaviour, ISerializationCallbackReceiver {
		public AnimatorControllerParameter[] parameters;
		readonly Dictionary<int, AnimatorControllerParameterType> parameterType = new Dictionary<int, AnimatorControllerParameterType>();

		/// <summary>
		/// Used to be able to support Hot Reloading
		/// </summary>
		List<CachedParameter> parameterTypeCache = new List<CachedParameter>();

		private void Awake() {
			hideFlags = HideFlags.HideAndDontSave;
			parameters = GetComponent<Animator>().parameters;

			foreach (var p in parameters) {
				parameterType.Add(p.nameHash, p.type);
			}
		}


		public bool TryGetParameterType(int parameterHash, out AnimatorControllerParameterType type) {
			return parameterType.TryGetValue(parameterHash, out type);
		}

		public void OnBeforeSerialize() {
			foreach (var elem in parameterType) {
				parameterTypeCache.Add(new CachedParameter() { hash = elem.Key, type = elem.Value });
			}
		}

		public void OnAfterDeserialize() {
			parameterType.Clear();
			foreach (var elem in parameterTypeCache) {
				parameterType.Add(elem.hash, elem.type);
			}
			parameterTypeCache.Clear();
		}

		[System.Serializable]
		struct CachedParameter {
			public int hash;
			public AnimatorControllerParameterType type;
		}
	}
}
