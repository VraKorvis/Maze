
	using UnityEngine;

	public class DebugRay : MonoBehaviour {
		private void Update() {
			Debug.DrawRay(transform.position, transform.up, Color.red, 1);
		}
	}
