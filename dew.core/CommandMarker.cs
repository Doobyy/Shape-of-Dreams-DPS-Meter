using System.Collections.Generic;
using UnityEngine;

public class CommandMarker : MonoBehaviour
{
	private class Ring
	{
		public CommandMarker[] instances;

		public int cursor;
	}

	public Transform followTransform;

	private const float GracePeriod = 0.1f;

	private const float CheckInterval = 0.5f;

	private bool _isPooled;

	private ParticleSystem[] _particles;

	private IEffectComponent[] _fxComponents;

	private float _spawnTime;

	private float _nextCheckTime;

	private static bool _creatingPooled;

	private static readonly Dictionary<CommandMarker, Ring> _pool = new Dictionary<CommandMarker, Ring>();

	private const int RingSize = 8;

	private static Transform _poolRoot;

	private void OnEnable()
	{
		if (!_creatingPooled && !_isPooled)
		{
			EffectAutoDestroy.Register(gameObject);
		}
	}

	private void Update()
	{
		if (followTransform != null)
		{
			transform.position = followTransform.position;
		}
		if (!_isPooled)
		{
			return;
		}
		float time = Time.time;
		if (!(time < _nextCheckTime))
		{
			_nextCheckTime = time + 0.5f;
			if (!(time - _spawnTime < 0.1f) && !IsStillPlaying())
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private bool IsStillPlaying()
	{
		for (int i = 0; i < _particles.Length; i++)
		{
			ParticleSystem val = _particles[i];
			if ((Object)(object)val != null && val.IsAlive(false))
			{
				return true;
			}
		}
		for (int j = 0; j < _fxComponents.Length; j++)
		{
			IEffectComponent effectComponent = _fxComponents[j];
			if ((!(effectComponent is Object obj) || (bool)obj) && effectComponent.isPlaying)
			{
				return true;
			}
		}
		return false;
	}

	public static CommandMarker Spawn(CommandMarker prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			return null;
		}
		if (!_pool.TryGetValue(prefab, out var value))
		{
			value = new Ring
			{
				instances = new CommandMarker[8],
				cursor = 0
			};
			_pool[prefab] = value;
		}
		int cursor = value.cursor;
		value.cursor = (cursor + 1) % value.instances.Length;
		CommandMarker commandMarker = value.instances[cursor];
		if (commandMarker == null)
		{
			_creatingPooled = true;
			commandMarker = Object.Instantiate(prefab, GetPoolRoot());
			_creatingPooled = false;
			commandMarker._isPooled = true;
			commandMarker._particles = commandMarker.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
			commandMarker._fxComponents = commandMarker.GetComponentsInChildren<IEffectComponent>(includeInactive: true);
			value.instances[cursor] = commandMarker;
		}
		commandMarker.followTransform = null;
		GameObject gameObject = commandMarker.gameObject;
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
		}
		commandMarker.transform.SetPositionAndRotation(position, rotation);
		commandMarker._spawnTime = Time.time;
		commandMarker._nextCheckTime = Time.time + 0.5f;
		gameObject.SetActive(value: true);
		return commandMarker;
	}

	public static void ClearPool()
	{
		foreach (Ring value in _pool.Values)
		{
			for (int i = 0; i < value.instances.Length; i++)
			{
				if (value.instances[i] != null)
				{
					Object.Destroy(value.instances[i].gameObject);
				}
			}
		}
		_pool.Clear();
	}

	private static Transform GetPoolRoot()
	{
		if (_poolRoot == null)
		{
			GameObject gameObject = new GameObject("Pooled Markers");
			Object.DontDestroyOnLoad(gameObject);
			_poolRoot = gameObject.transform;
		}
		return _poolRoot;
	}
}
