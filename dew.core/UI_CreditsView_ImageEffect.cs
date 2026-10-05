using UnityEngine;
using UnityEngine.UI;

public class UI_CreditsView_ImageEffect : MonoBehaviour
{
	private static readonly int OverlayTex = Shader.PropertyToID("_OverlayTex");

	public int randomSeed;

	private Material _instance;

	private Material _original;

	private Image _image;

	private void EnsureReady()
	{
		if (!(Object)(object)_image)
		{
			_image = GetComponent<Image>();
		}
	}

	public void RevertMaterial()
	{
		EnsureReady();
		if ((bool)_instance)
		{
			Object.Destroy(_instance);
			_instance = null;
			((Graphic)_image).material = _original;
			_original = null;
		}
	}

	public void CreateMaterial()
	{
		EnsureReady();
		if (!_instance)
		{
			_original = ((Graphic)_image).material;
			_instance = Object.Instantiate(_original);
			((Graphic)_image).material = _instance;
		}
	}

	private void OnValidate()
	{
		if (randomSeed == 0)
		{
			randomSeed = Random.Range(0, int.MaxValue);
		}
	}

	private void Update()
	{
		if ((bool)_instance)
		{
			Vector2 value = new Vector2(0f, Time.unscaledTime * 0.05f);
			_instance.SetTextureOffset(OverlayTex, value);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_instance)
		{
			Object.Destroy(_instance);
		}
	}
}
