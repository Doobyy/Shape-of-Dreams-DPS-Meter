using UnityEngine;
using VolumetricFogAndMist2;

public class SnowMountain_RotateFogVolume : MonoBehaviour
{
	public VolumetricFog fog;

	private VolumetricFogProfile _fogProfileClone;

	private void Start()
	{
		_fogProfileClone = Object.Instantiate<VolumetricFogProfile>(fog.profile);
		fog.profile = _fogProfileClone;
		GameManager.CallOnReady(RotateFogVolume);
	}

	private void RotateFogVolume()
	{
		Quaternion quaternion = Quaternion.Euler(0f, ManagerBase<CameraManager>.instance.entityCamAngle, 0f);
		Vector3 windDirection = fog.profile.windDirection;
		Vector3 detailNoiseWindDirection = fog.profile.detailNoiseWindDirection;
		fog.profile.windDirection = quaternion * windDirection;
		fog.profile.detailNoiseWindDirection = quaternion * detailNoiseWindDirection;
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)_fogProfileClone);
	}
}
