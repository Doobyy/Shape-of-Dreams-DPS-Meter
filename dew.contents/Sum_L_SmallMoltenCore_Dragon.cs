using Mirror;
using UnityEngine;

public class Sum_L_SmallMoltenCore_Dragon : Summon
{
	public float empowerDuration = 3f;

	private ActorRef<Se_L_SmallMoltenCore_AttackLink> _link;

	private float _empoweredUntil;

	private float _nextRepositionTime;

	private Vector3 _hoverDest;

	private bool _hasDest;

	public bool IsEmpowered => Time.time < _empoweredUntil;

	public void Empower()
	{
		_empoweredUntil = Mathf.Max(_empoweredUntil, Time.time + empowerDuration);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (((NetworkBehaviour)this).isServer && !((Object)(object)info.caster == null))
		{
			_link = CreateStatusEffect(info.caster, new CastInfo(info.caster), (Se_L_SmallMoltenCore_AttackLink se) =>
			{
				se.dragon = this;
			});
		}
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			bool flag = Vector2.Distance(info.caster.agentPosition.ToXY(), agentPosition.ToXY()) > 5f;
			if ((!_hasDest || Time.time >= _nextRepositionTime) | flag)
			{
				_nextRepositionTime = Time.time + Random.Range(3f, 5f);
				_hoverDest = Dew.GetGoodRewardPosition(info.caster.agentPosition, Random.Range(2f, 3f));
				_hasDest = true;
			}
			Control.MoveToDestination(_hoverDest, immediately: false);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer)
		{
			if (!_link.IsNullOrInactive())
			{
				_link.Get().Destroy();
			}
			_link = null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
