using UnityEngine;

namespace C0
{
	public class CameraFollow : MonoBehaviour
	{
		public bool DebugDraw;

		private RectShape targetRectShape;

		private Vector3 smoothDampVelocity;
		private float smoothDampTime;

		private Vector2 offset;

		private FocusArea focusArea;

		void Start()
		{
			DebugDraw = false;

			targetRectShape = GameObject.Find("Player/Body").GetComponent<RectShape>();

			smoothDampTime = 0.05f;

			offset = Vector2.zero;
			focusArea = new FocusArea(targetRectShape, new Vector2(3, 5));

			UpdateCameraPosition();
		}

		void LateUpdate()
		{
			UpdateCameraPosition();
		}

		private void UpdateCameraPosition()
		{
			focusArea.UpdatePosition();

			Vector3 focusPosition = Vector3.SmoothDamp(
				transform.position,
				focusArea.Center + offset,
				ref smoothDampVelocity,
				smoothDampTime
			);

			transform.position = focusPosition - 10 * Vector3.forward;
		}

		void OnDrawGizmos()
		{
			if (DebugDraw)
			{
				Gizmos.color = new Color(1, 0, 1, 0.1f);

				Gizmos.DrawWireCube(focusArea.Center + offset, focusArea.Size);
			}
		}
	}
}