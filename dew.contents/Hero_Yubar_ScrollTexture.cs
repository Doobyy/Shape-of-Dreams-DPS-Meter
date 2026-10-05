using System.Collections.Generic;
using UnityEngine;

public class Hero_Yubar_ScrollTexture : MonoBehaviour
{
	public string[] scrolledProperties = new string[1] { "_BaseMap" };

	public float scrollSpeed = 0.05f;

	private SkinnedMeshRenderer _renderer;

	private MaterialPropertyBlock _mpb;

	private int _materialIndex = -1;

	private Vector4[] _st;

	private int[] _stIds;

	private void Awake()
	{
		_renderer = GetComponent<SkinnedMeshRenderer>();
		_mpb = new MaterialPropertyBlock();
	}

	private void Update()
	{
		if (_materialIndex < 0)
		{
			List<Material> list = DewPool.GetList(out ListReturnHandle<Material> handle);
			_renderer.GetSharedMaterials(list);
			if (list.Count == 3 && list[2] != null)
			{
				_materialIndex = 2;
			}
			else if (list.Count == 1 && list[0] != null)
			{
				_materialIndex = 0;
			}
			if (_materialIndex >= 0)
			{
				Material material = list[_materialIndex];
				_st = new Vector4[scrolledProperties.Length];
				_stIds = new int[scrolledProperties.Length];
				for (int i = 0; i < scrolledProperties.Length; i++)
				{
					Vector2 textureScale = material.GetTextureScale(scrolledProperties[i]);
					Vector2 textureOffset = material.GetTextureOffset(scrolledProperties[i]);
					_st[i] = new Vector4(textureScale.x, textureScale.y, textureOffset.x, textureOffset.y);
					_stIds[i] = Shader.PropertyToID(scrolledProperties[i] + "_ST");
				}
			}
			handle.Return();
		}
		else
		{
			_renderer.GetPropertyBlock(_mpb, _materialIndex);
			for (int j = 0; j < scrolledProperties.Length; j++)
			{
				Vector4 value = _st[j];
				value.z = Time.time * scrollSpeed;
				_mpb.SetVector(_stIds[j], value);
			}
			_renderer.SetPropertyBlock(_mpb, _materialIndex);
		}
	}
}
