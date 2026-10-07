using UnityEngine;

namespace VolFx;

[AddComponentMenu("")]
public class DestroySelf : MonoBehaviour
{
	public void Invoke()
	{
		Object.Destroy(gameObject);
	}
}
