using System.Collections.Generic;
using UnityEngine;

public static class PhysX3DSimulationDriver
{
	private class PhysX3DSimulationRunner : MonoBehaviour
	{
		private void FixedUpdate()
		{
			if (_activeOwners.Count > 0)
			{
				Physics.Simulate(Time.fixedDeltaTime);
			}
			else
			{
				Physics.SyncTransforms();
			}
		}
	}

	private static GameObject _driverObject;

	private static PhysX3DSimulationRunner _driver;

	private static readonly HashSet<object> _activeOwners = new HashSet<object>();

	public static int activeOwnerCount => _activeOwners.Count;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		Physics.simulationMode = (SimulationMode)2;
		_activeOwners.Clear();
		EnsureDriver();
	}

	public static void Register(object owner)
	{
		if (owner != null)
		{
			EnsureDriver();
			_activeOwners.Add(owner);
		}
	}

	public static void Unregister(object owner)
	{
		if (owner != null)
		{
			_activeOwners.Remove(owner);
		}
	}

	private static void EnsureDriver()
	{
		if (!(_driver != null))
		{
			_driverObject = new GameObject("[PhysX3DSimulationDriver]");
			_driverObject.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(_driverObject);
			_driver = _driverObject.AddComponent<PhysX3DSimulationRunner>();
		}
	}
}
