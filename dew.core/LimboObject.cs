using UnityEngine;

public class LimboObject : MonoBehaviour, ILimboNotifyReceiver
{
	public bool showOnLimbo = true;

	public void OnLimboGame()
	{
		gameObject.SetActive(showOnLimbo);
		if (!showOnLimbo && TryGetComponent<EnvParticle>(out var component))
		{
			Object.Destroy(component);
		}
	}
}
