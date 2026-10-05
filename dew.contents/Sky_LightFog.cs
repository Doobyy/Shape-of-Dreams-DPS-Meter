using UnityEngine;
using VolumetricFogAndMist2;

public class Sky_LightFog : SingletonBehaviour<Sky_LightFog>, IImportantFogVolume
{
	private VolumetricFog _fow;

	private Vector3 _fogSize;

	private float _tnxMin;

	private float _tnxMax;

	private float _tnzMin;

	private float _tnzMax;

	protected override void Awake()
	{
		base.Awake();
		EnsureReferences();
	}

	private void EnsureReferences()
	{
		if ((Object)(object)_fow == null)
		{
			_fow = GetComponent<VolumetricFog>();
			_fogSize = ((Component)(object)_fow).transform.localScale;
			Vector3 fogOfWarSize = _fow.fogOfWarSize;
			_tnxMin = 0.5f - _fogSize.x / fogOfWarSize.x * 0.5f;
			_tnxMax = 0.5f + _fogSize.x / fogOfWarSize.x * 0.5f;
			_tnzMin = 0.5f - _fogSize.z / fogOfWarSize.z * 0.5f;
			_tnzMax = 0.5f + _fogSize.z / fogOfWarSize.z * 0.5f;
		}
	}

	public float SampleFogOpacity(Vector3 worldPosition)
	{
		EnsureReferences();
		float x = worldPosition.x;
		float z = worldPosition.z;
		float t = Mathf.Clamp01(x / _fogSize.x + 0.5f);
		float t2 = Mathf.Clamp01(z / _fogSize.z + 0.5f);
		float u = Mathf.Lerp(_tnxMin, _tnxMax, t) - _fogSize.x / _fow.fogOfWarSize.x / (float)_fow.fogOfWarTexture.width * 2.5f;
		float v = Mathf.Lerp(_tnzMin, _tnzMax, t2) - _fogSize.z / _fow.fogOfWarSize.z / (float)_fow.fogOfWarTexture.height * 2.5f;
		return _fow.fogOfWarTexture.GetPixelBilinear(u, v).a;
	}
}
