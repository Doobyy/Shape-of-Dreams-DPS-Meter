using UnityEngine;

public class BoneOffset : MonoBehaviour
{
	public Vector3 localPosition;

	public Vector3 localRotation;

	public Vector3 localScale = Vector3.one;

	private void LateUpdate()
	{
		transform.localScale = localScale;
		transform.localPosition += localPosition;
		transform.localRotation *= Quaternion.Euler(localRotation);
	}
}
