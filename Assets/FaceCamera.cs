// ---------------------------------------------------------------------
// Copyright (c) 2016 Magic Leap. All Rights Reserved.
// Magic Leap Confidential and Proprietary
// ---------------------------------------------------------------------

using UnityEngine;

namespace Invaders.Dev.BDJ {
	public enum Axis {
		None,
		X,
		Y,
		Z
	}

	public class FaceCamera : MonoBehaviour {
		public bool Flip;
		public bool Reorient = true;
		public bool OnlyOnStart;
		public Axis LockAxis = Axis.None;
		private void Start() { UpdateFacing(); }

		private void LateUpdate() {
			if (OnlyOnStart) { return; }
			UpdateFacing();
		}

		private void UpdateFacing() {
			Transform t = transform;
			if (Reorient) { t.rotation = Quaternion.identity; }
			Vector3 lookAtPos = Camera.main.transform.position;
			Vector3 lookAtPosLocalised = t.InverseTransformPoint(lookAtPos);

			switch (LockAxis) {
				case Axis.X:
					lookAtPosLocalised.x = 0f;
					break;
				case Axis.Y:
					lookAtPosLocalised.y = 0f;
					break;
				case Axis.Z:
					lookAtPosLocalised.z = 0f;
					break;
			}

			lookAtPos = t.TransformPoint(lookAtPosLocalised);
			t.LookAt(lookAtPos);
			if (Flip) { t.Rotate(0, 180, 0, Space.Self); }
		}
	}
}