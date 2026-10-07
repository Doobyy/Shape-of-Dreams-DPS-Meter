using Mirror;
using UnityEngine;

public class Room_Trap_CyclicActivator : Room_Trap_ActivatorBase
{
	public float cycleInterval = 3f;

	public float[] activateTimes = new float[1];

	public float minDistanceFromPlayerToBeActive = 25f;

	public float timeScale = 1f;

	private float _currentCycleTime = float.NaN;

	private float _lastTickTime;

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
		{
			return;
		}
		dt *= timeScale;
		float num = Time.time * timeScale;
		if (float.IsNaN(_currentCycleTime))
		{
			_currentCycleTime = num;
			_lastTickTime = _currentCycleTime - dt;
		}
		float[] array = activateTimes;
		foreach (float num2 in array)
		{
			float num3 = _currentCycleTime + num2;
			if (num > num3 && num3 >= _lastTickTime && Dew.GetClosestHeroDistance(((Component)(object)this).transform.position) < minDistanceFromPlayerToBeActive)
			{
				Activate();
			}
		}
		if (num - _currentCycleTime >= cycleInterval)
		{
			_currentCycleTime += cycleInterval;
			_lastTickTime = _currentCycleTime;
		}
		else
		{
			_lastTickTime = num;
		}
	}

	private void MirrorProcessed()
	{
	}
}
