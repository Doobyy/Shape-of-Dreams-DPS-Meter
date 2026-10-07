using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FxMeshTrail : FxInterpolatedEffectBase, IAttachableToEntity
{
	private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	public Gradient trailGradient;

	public Material meshTrailMaterial;

	public int trailCount = 10;

	public float trailInterval = 0.1f;

	public Vector3 localVelocity;

	public Vector3 worldVelocity;

	private Mesh[] _meshPool;

	private int[] _meshRefCounts;

	private int[] _trailMeshIndices;

	private int _lastUsedMeshIndex = -1;

	private Matrix4x4[] _matrices;

	private float[] _opacities;

	private MaterialPropertyBlock[] _propBlocks;

	private Dew.BakedMeshHandle[] _bakeHandles;

	private CombineInstance[] _combineInstances;

	private int _currentIndex;

	private float _lastTrailUpdateTime;

	private int _skinCount;

	private List<SkinnedMeshRenderer> _skins;

	private ListReturnHandle<SkinnedMeshRenderer> _skinsHandle;

	private Entity _target;

	private Color _originalBaseColor;

	private Color _originalEmissionColor;

	public void OnAttachToEntity(Entity target)
	{
		if ((bool)(UnityEngine.Object)(object)_target)
		{
			_target.Visual.ClientEvent_OnModelLoaded -= new Action(InitRenderers);
		}
		_target = target;
		if (!((UnityEngine.Object)(object)_target == null) && !GraphicsManager.WasLowFpsInLast5Seconds())
		{
			if ((bool)_target.Visual.model)
			{
				InitRenderers();
			}
			_target.Visual.ClientEvent_OnModelLoaded += new Action(InitRenderers);
		}
	}

	private void InitRenderers()
	{
		if (_meshPool != null)
		{
			Cleanup();
		}
		if ((bool)this)
		{
			_skins = ((Component)(object)_target).GetComponentsInChildrenNonAlloc(out _skinsHandle);
			_skinCount = _skins.Count;
			_meshPool = new Mesh[trailCount];
			_meshRefCounts = new int[trailCount];
			_trailMeshIndices = new int[trailCount];
			_matrices = new Matrix4x4[trailCount];
			_opacities = ArrayPool<float>.Shared.Rent(trailCount);
			_propBlocks = new MaterialPropertyBlock[trailCount];
			_bakeHandles = new Dew.BakedMeshHandle[_skinCount];
			_combineInstances = new CombineInstance[_skinCount];
			_originalBaseColor = meshTrailMaterial.GetColor(BaseColor);
			_originalEmissionColor = meshTrailMaterial.GetColor(EmissionColor);
			for (int i = 0; i < trailCount; i++)
			{
				_opacities[i] = 0f;
				_meshPool[i] = new Mesh
				{
					indexFormat = IndexFormat.UInt32
				};
				_trailMeshIndices[i] = -1;
				_meshRefCounts[i] = 0;
				_matrices[i] = Matrix4x4.identity;
				_propBlocks[i] = new MaterialPropertyBlock();
				_propBlocks[i].SetColor(BaseColor, Color.clear);
				_propBlocks[i].SetColor(EmissionColor, Color.clear);
			}
			_currentIndex = -1;
			_lastUsedMeshIndex = -1;
			UpdateMeshTrail();
		}
	}

	private void LateUpdate()
	{
		if (GraphicsManager.WasLowFpsInLast5Seconds())
		{
			return;
		}
		if (_meshPool == null)
		{
			if (!((UnityEngine.Object)(object)_target == null) && !(currentValue < 0.001f))
			{
				OnAttachToEntity(_target);
			}
			return;
		}
		if (currentValue < 0.001f)
		{
			bool flag = false;
			for (int i = 0; i < trailCount; i++)
			{
				if (_opacities[i] > 0.01f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Cleanup();
				return;
			}
		}
		if (Time.time - _lastTrailUpdateTime > trailInterval)
		{
			UpdateMeshTrail();
		}
		float deltaTime = Time.deltaTime;
		for (int j = 0; j < trailCount; j++)
		{
			if (!(_opacities[j] <= 0f))
			{
				int num = _trailMeshIndices[j];
				if (num >= 0)
				{
					Vector3 position = _matrices[j].GetPosition();
					Quaternion rotation = _matrices[j].rotation;
					position += worldVelocity * deltaTime + rotation * localVelocity * deltaTime;
					_matrices[j] = Matrix4x4.TRS(position, rotation, Vector3.one);
					Graphics.DrawMesh(_meshPool[num], _matrices[j], meshTrailMaterial, 0, null, 0, _propBlocks[j]);
				}
			}
		}
	}

	protected override void ValueSetter(float value)
	{
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)(UnityEngine.Object)(object)_target)
		{
			_target.Visual.ClientEvent_OnModelLoaded -= new Action(InitRenderers);
		}
		_target = null;
		Cleanup();
	}

	private void UpdateMeshTrail()
	{
		if (GraphicsManager.WasLowFpsInLast5Seconds() || _meshPool == null || (UnityEngine.Object)(object)_target == null || trailCount <= 0)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		try
		{
			_lastTrailUpdateTime = Time.time;
			_currentIndex++;
			if (_currentIndex >= trailCount)
			{
				_currentIndex = 0;
			}
			if (_trailMeshIndices[_currentIndex] >= 0)
			{
				_meshRefCounts[_trailMeshIndices[_currentIndex]]--;
				_trailMeshIndices[_currentIndex] = -1;
			}
			Matrix4x4 matrix4x = Matrix4x4.TRS(((Component)(object)_target).transform.position, ((Component)(object)_target).transform.rotation, Vector3.one);
			Matrix4x4 inverse = matrix4x.inverse;
			for (int i = 0; i < _skinCount; i++)
			{
				Dew.GetBakedMeshOptimized(_skins[i], out var mesh, out _bakeHandles[i], out var isNew);
				num++;
				if (isNew)
				{
					flag = true;
				}
				Transform transform = _skins[i].transform;
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
				_combineInstances[i] = new CombineInstance
				{
					mesh = mesh,
					transform = inverse * matrix4x2
				};
			}
			if (flag || _lastUsedMeshIndex == -1)
			{
				int num2 = 0;
				for (int j = 0; j < trailCount; j++)
				{
					if (_meshRefCounts[j] == 0)
					{
						num2 = j;
						break;
					}
				}
				_meshPool[num2].CombineMeshes(_combineInstances, mergeSubMeshes: true, useMatrices: true);
				_lastUsedMeshIndex = num2;
			}
			_trailMeshIndices[_currentIndex] = _lastUsedMeshIndex;
			_meshRefCounts[_lastUsedMeshIndex]++;
			_matrices[_currentIndex] = matrix4x;
			for (int k = 0; k < trailCount; k++)
			{
				_opacities[k] = ((!_target.Visual.isRendererOff && k == _currentIndex) ? currentValue : Mathf.MoveTowards(_opacities[k], 0f, 1f / (float)trailCount));
				Color color = trailGradient.Evaluate(1f - _opacities[k]);
				_propBlocks[k].SetColor(BaseColor, color * _originalBaseColor);
				_propBlocks[k].SetColor(EmissionColor, color * _originalEmissionColor);
			}
		}
		catch (Exception)
		{
			Cleanup();
		}
		finally
		{
			if (_bakeHandles != null)
			{
				for (int l = 0; l < num; l++)
				{
					_bakeHandles[l].Return();
				}
			}
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (_meshPool == null)
		{
			return;
		}
		for (int i = 0; i < trailCount; i++)
		{
			if ((bool)_meshPool[i])
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(_meshPool[i]);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(_meshPool[i]);
				}
			}
		}
		_skins = null;
		_skinsHandle.Return();
		ArrayPool<float>.Shared.Return(_opacities, false);
		_meshPool = null;
		_meshRefCounts = null;
		_trailMeshIndices = null;
		_matrices = null;
		_opacities = null;
		_propBlocks = null;
		_bakeHandles = null;
		_combineInstances = null;
	}
}
