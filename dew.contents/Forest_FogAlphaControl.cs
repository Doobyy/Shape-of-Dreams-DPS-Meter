using UnityEngine;
using VolumetricFogAndMist2;

public class Forest_FogAlphaControl : MonoBehaviour
{
	public VolumetricFog fog;

	public float maxAlpha;

	public int maxRoomCount;

	private VolumetricFogProfile _fogProfileClone;

	private void Start()
	{
		maxAlpha = fog.profile.albedo.a;
		_fogProfileClone = Object.Instantiate<VolumetricFogProfile>(fog.profile);
		fog.profile = _fogProfileClone;
		GameManager.CallOnReady(ChangeFogAlphaWithRoomIndex);
	}

	private void ChangeFogAlphaWithRoomIndex()
	{
		int currentZoneClearedNodes = NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes;
		fog.profile.albedo.a = Mathf.Lerp(0f, maxAlpha, 1f / (float)(maxRoomCount - 1) * (float)currentZoneClearedNodes);
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)_fogProfileClone);
	}
}
