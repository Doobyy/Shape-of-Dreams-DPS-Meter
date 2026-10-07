using UnityEngine;

public class PerFeatureVisibility : MonoBehaviour
{
	public BuildFeatureTag requiredFeature;

	public bool invertVisibility;

	private void Start()
	{
		if (DewBuildProfile.current.HasFeature(requiredFeature) == invertVisibility)
		{
			Object.Destroy(gameObject);
		}
	}
}
