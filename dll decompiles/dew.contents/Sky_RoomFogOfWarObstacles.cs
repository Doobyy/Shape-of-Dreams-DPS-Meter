using Unity.AI.Navigation;
using UnityEngine;
using VolumetricFogAndMist2;

public class Sky_RoomFogOfWarObstacles : RoomBakeProcessor
{
	public VolumetricFog fowComponent;

	public float density;

	public float threshold;

	public float capsuleRadius;

	public float capsuleHeight;

	public int area;

	private GameObject _createdObstacleParent;

	public override void BeforeNavMeshBake()
	{
		base.BeforeNavMeshBake();
		Sky_LightFog sky_LightFog = Object.FindObjectOfType<Sky_LightFog>(includeInactive: true);
		_createdObstacleParent = new GameObject("Sky Obstacle Parent");
		Vector3 localScale = ((Component)(object)fowComponent).transform.localScale;
		float num = density * 2f;
		float num2 = capsuleRadius / 2f;
		int num3 = Mathf.RoundToInt(localScale.x * num);
		int num4 = Mathf.RoundToInt(localScale.z * num);
		Vector3 fogOfWarSize = fowComponent.fogOfWarSize;
		_ = localScale.x / fogOfWarSize.x;
		_ = localScale.x / fogOfWarSize.x;
		_ = localScale.z / fogOfWarSize.z;
		_ = localScale.z / fogOfWarSize.z;
		for (int i = 0; i < num3; i++)
		{
			float x = ((float)i / (float)num3 - 0.5f) * localScale.x;
			for (int j = 0; j < num4; j++)
			{
				float z = ((float)j / (float)num4 - 0.5f) * localScale.z;
				if (sky_LightFog.SampleFogOpacity(new Vector3(x, 0f, z)) > threshold)
				{
					Transform transform = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
					transform.parent = _createdObstacleParent.transform;
					transform.SetLocalPositionAndRotation(new Vector3(x, 0f, z), Quaternion.identity);
					transform.localScale = new Vector3(num2 * 2f, capsuleHeight * 0.5f, num2 * 2f);
					transform.gameObject.layer = 17;
					NavMeshModifier val = transform.gameObject.AddComponent<NavMeshModifier>();
					val.overrideArea = true;
					val.area = area;
				}
			}
		}
	}

	public override void AfterNavMeshBake()
	{
		base.AfterNavMeshBake();
		if (!(_createdObstacleParent == null))
		{
			Object.DestroyImmediate(_createdObstacleParent);
			_createdObstacleParent = null;
		}
	}
}
