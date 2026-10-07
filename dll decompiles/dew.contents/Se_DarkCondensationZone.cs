using UnityEngine;

public class Se_DarkCondensationZone : StatusEffect
{
	public float fullVisibleDistance;

	public float fullInvisibleDistance;

	private EntityColorModifier _entModifier;

	private Hero _target;

	protected override void OnCreate()
	{
		base.OnCreate();
		_target = DewPlayer.local.hero;
		_entModifier = victim.Visual.GetNewColorModifier();
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float num = Vector3.Distance(victim.agentPosition, _target.agentPosition);
		if (num <= fullVisibleDistance)
		{
			_entModifier.opacity = 1f;
		}
		else if (num > fullVisibleDistance)
		{
			if (num > fullInvisibleDistance)
			{
				_entModifier.opacity = 0f;
				return;
			}
			float opacity = (fullInvisibleDistance - num) / fullInvisibleDistance;
			_entModifier.opacity = opacity;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entModifier != null)
		{
			_entModifier.Stop();
			_entModifier = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
