using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessoryPreviewRenderer
{
	private readonly Camera _previewRenderer;

	private readonly RawImage _rawImage;

	private readonly GameObject _previewSceneObject;

	private Accessory _current;

	private RenderTexture _rt;

	public AccessoryPreviewRenderer(Camera previewRenderer, RawImage rawImage, GameObject previewSceneObject = null)
	{
		_previewRenderer = previewRenderer;
		_rawImage = rawImage;
		_previewSceneObject = previewSceneObject;
	}

	public void Show(Accessory prefab)
	{
		Clear();
		if (prefab == null)
		{
			return;
		}
		UpdateRT();
		_current = Object.Instantiate(prefab, null);
		Dew.SetUnscaledTimeUpdate(_current.gameObject);
		_current.gameObject.SetLayerRecursive(20);
		_current.transform.rotation = Quaternion.identity;
		if ((bool)_current.previewCustomCenter)
		{
			Transform previewCustomCenter = _current.previewCustomCenter;
			Vector3 vector = _current.transform.InverseTransformPoint(previewCustomCenter.position);
			Quaternion rotation = Quaternion.Inverse(_current.transform.rotation) * previewCustomCenter.rotation;
			_current.transform.rotation = Quaternion.Inverse(rotation);
			_current.transform.position = -(_current.transform.rotation * vector);
		}
		else
		{
			ListReturnHandle<Accessory_AttachPoint> handle;
			foreach (Accessory_AttachPoint item in ((Component)_current).GetComponentsInChildrenNonAlloc(out handle))
			{
				item.transform.localRotation = Quaternion.identity;
				item.transform.localPosition = Vector3.zero;
			}
			handle.Return();
			List<Renderer> componentsInChildrenNonAlloc = ((Component)_current).GetComponentsInChildrenNonAlloc(out ListReturnHandle<Renderer> handle2);
			if (componentsInChildrenNonAlloc.Count > 0)
			{
				bool flag = false;
				Bounds bounds = default;
				foreach (Renderer item2 in componentsInChildrenNonAlloc)
				{
					if (item2 is MeshRenderer || item2 is SkinnedMeshRenderer)
					{
						if (!flag)
						{
							bounds = item2.bounds;
							flag = true;
						}
						else
						{
							bounds.Encapsulate(item2.bounds);
						}
					}
				}
				if (flag)
				{
					Vector3 vector2 = -bounds.center;
					ListReturnHandle<Accessory_AttachPoint> handle3;
					foreach (Accessory_AttachPoint item3 in ((Component)_current).GetComponentsInChildrenNonAlloc(out handle3))
					{
						item3.transform.position += vector2;
					}
					handle3.Return();
				}
			}
			handle2.Return();
		}
		Quaternion quaternion = Quaternion.LookRotation(_current.transform.rotation * RuntimePreviewGenerator.PreviewDirection, _current.transform.up);
		Bounds bounds2 = default;
		RuntimePreviewGenerator.CalculateBounds(_current.transform, true, quaternion, ref bounds2);
		_previewRenderer.aspect = (float)_rt.width / (float)_rt.height;
		_previewRenderer.transform.rotation = quaternion;
		_previewRenderer.backgroundColor = Color.black.WithA(0f);
		RuntimePreviewGenerator.CalculateCameraPosition(_previewRenderer, bounds2, _current.previewPadding);
		if (_previewRenderer.orthographicSize <= 0.0001f)
		{
			_previewRenderer.orthographicSize = 0.8f;
		}
		_previewRenderer.nearClipPlane = 0.001f;
		_previewRenderer.orthographic = true;
		if (_current.previewCustomCenter != null)
		{
			_previewRenderer.transform.LookAt(_current.previewCustomCenter);
		}
		if (_previewSceneObject != null)
		{
			_previewSceneObject.transform.position = ((_current.previewCustomCenter != null) ? _current.previewCustomCenter.position : _current.transform.position);
			_previewSceneObject.transform.rotation = _previewRenderer.transform.rotation;
			_previewSceneObject.transform.localScale /= _previewSceneObject.transform.lossyScale.x;
		}
	}

	public void Tick()
	{
		UpdateRT();
		if (_current != null)
		{
			Transform transform = ((_current.previewCustomCenter != null) ? _current.previewCustomCenter : _current.transform);
			_current.transform.RotateAround(transform.position, Vector3.up, 80f * Time.unscaledDeltaTime);
		}
	}

	public void Clear()
	{
		if (_current != null)
		{
			Object.Destroy(_current.gameObject);
			_current = null;
		}
	}

	public void Dispose()
	{
		Clear();
		if (_rt != null)
		{
			Object.Destroy(_rt);
			_rt = null;
		}
	}

	private void UpdateRT()
	{
		Rect screenSpaceRect = ((Graphic)_rawImage).rectTransform.GetScreenSpaceRect();
		if (_rt == null || _rt.width != Mathf.RoundToInt(screenSpaceRect.width) || _rt.height != Mathf.RoundToInt(screenSpaceRect.height))
		{
			if (_rt != null)
			{
				Object.Destroy(_rt);
			}
			_rt = new RenderTexture(Mathf.RoundToInt(screenSpaceRect.width), Mathf.RoundToInt(screenSpaceRect.height), 24, RenderTextureFormat.ARGB32);
			_previewRenderer.targetTexture = _rt;
			_rawImage.texture = _rt;
		}
	}
}
