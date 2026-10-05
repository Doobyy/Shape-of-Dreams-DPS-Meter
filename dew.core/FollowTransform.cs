using UnityEngine;

public class FollowTransform : MonoBehaviour
{
	public Transform target;

	public bool followPosition = true;

	public bool followRotation = true;

	public bool followScale;

	private void OnEnable()
	{
		Follow();
	}

	private void Update()
	{
		Follow();
	}

	private void Follow()
	{
		if ((bool)target)
		{
			if (followPosition)
			{
				transform.position = target.position;
			}
			if (followRotation)
			{
				transform.rotation = target.rotation;
			}
			if (followScale)
			{
				float num = (transform.parent ? transform.parent.lossyScale.x : 1f);
				transform.localScale = target.lossyScale / num;
			}
		}
	}
}
