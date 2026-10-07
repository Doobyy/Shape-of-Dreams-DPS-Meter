using UnityEngine;

public class FlickerGameObject : MonoBehaviour
{
	public float interval;

	private float _lastFlickerTime;

	private void Update()
	{
		if (!(Time.time - _lastFlickerTime < interval))
		{
			_lastFlickerTime = Time.time;
			gameObject.SetActive(value: false);
			gameObject.SetActive(value: true);
		}
	}
}
