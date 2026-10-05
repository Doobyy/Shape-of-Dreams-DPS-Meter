using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class ImageComponent : MonoBehaviour
{
	private bool _needToDestroyMaterial;

	public Image image { get; private set; }

	public Material material { get; private set; }

	public virtual void Start()
	{
		image = GetComponent<Image>();
		if ((UnityEngine.Object)(object)image == null || ((Graphic)image).material == null)
		{
			return;
		}
		if (((Graphic)image).material.name.EndsWith("(Clone)"))
		{
			material = ((Graphic)image).material;
		}
		else
		{
			material = UnityEngine.Object.Instantiate(((Graphic)image).material);
			((Graphic)image).material = material;
			_needToDestroyMaterial = true;
		}
		if ((UnityEngine.Object)(object)image == null || material == null)
		{
			return;
		}
		try
		{
			Init();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void OnDestroy()
	{
		if (_needToDestroyMaterial && material != null)
		{
			UnityEngine.Object.Destroy(material);
		}
	}

	public virtual void Update()
	{
		if ((UnityEngine.Object)(object)image == null || material == null)
		{
			return;
		}
		try
		{
			Tick();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public virtual void Init()
	{
	}

	public virtual void Tick()
	{
	}
}
