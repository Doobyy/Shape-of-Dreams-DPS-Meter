using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Ai_Mon_Ink_DivineAnimal_Missile : AbilityInstance
{
	public int missileCount;

	public float startDelay;

	public float minRange;

	public float maxRange;

	public float gapForEachInstance;

	public float delayForEachInstance;

	public GameObject telegraph;

	public float delayForTelegraph;

	private Vector3 _previousPos;

	private List<Vector3> _posList;

	private int _baseMissileCount;

	protected override void Awake()
	{
		base.Awake();
		_baseMissileCount = missileCount;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		missileCount = _baseMissileCount;
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!((NetworkBehaviour)this).isServer)
		{
			yield break;
		}
		DestroyOnDeath(info.caster);
		_posList = new List<Vector3>();
		_previousPos = Vector3.zero;
		int count = 0;
		int calCount = 0;
		yield return new SI.WaitForSeconds(startDelay);
		while (count < missileCount)
		{
			if (calCount > 2)
			{
				_posList.Add(_previousPos);
				count++;
				calCount = 0;
				continue;
			}
			Vector3 normalized = Random.insideUnitSphere.Flattened().normalized;
			Vector3 end = info.caster.agentPosition + normalized * Random.Range(minRange, maxRange);
			end = Dew.GetValidAgentDestination_Closest(info.caster.agentPosition, end);
			if (Vector3.Distance(end, _previousPos) < gapForEachInstance && count != 0)
			{
				calCount++;
				continue;
			}
			_posList.Add(end);
			count++;
			calCount = 0;
			_previousPos = end;
		}
		foreach (Vector3 pos in _posList)
		{
			FxPlayNewNetworked(telegraph, pos, Quaternion.identity);
			yield return new SI.WaitForSeconds(delayForEachInstance);
			CreateAbilityInstance(pos, null, info, (Ai_Mon_Ink_DivineAnimal_Missile_Sub p) =>
			{
				p.startDelay = delayForTelegraph;
			});
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
