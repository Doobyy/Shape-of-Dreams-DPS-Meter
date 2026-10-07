using UnityEngine;

public class DestroyInPlayMode : MonoBehaviour
{
	private void Awake()
	{
		Object.Destroy(gameObject);
	}
}
