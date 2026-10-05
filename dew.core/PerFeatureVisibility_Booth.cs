using UnityEngine;

public class PerFeatureVisibility_Booth : MonoBehaviour
{
	public bool destroyIfBooth;

	public bool destroyIfNotBooth;

	private void Start()
	{
		bool flag = DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth);
		if (flag && destroyIfBooth)
		{
			Object.Destroy(gameObject);
		}
		if (!flag && destroyIfNotBooth)
		{
			Object.Destroy(gameObject);
		}
	}
}
