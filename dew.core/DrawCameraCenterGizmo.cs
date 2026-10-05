using UnityEngine;

public class DrawCameraCenterGizmo : MonoBehaviour
{
	public Camera targetCamera;

	public float distance = 50f;

	public float size = 5f;

	public Color color = Color.red;

	public bool drawOnlyWhenSelected;

	private void OnDrawGizmos()
	{
		if (!drawOnlyWhenSelected)
		{
			Draw();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (drawOnlyWhenSelected)
		{
			Draw();
		}
	}

	private void Draw()
	{
		Camera camera = ((targetCamera != null) ? targetCamera : Camera.main);
		if (!(camera == null))
		{
			Transform transform = camera.transform;
			Vector3 vector = transform.position + transform.forward * distance;
			Gizmos.color = color;
			Gizmos.DrawLine(vector - transform.right * size, vector + transform.right * size);
			Gizmos.DrawLine(vector - transform.up * size, vector + transform.up * size);
		}
	}
}
