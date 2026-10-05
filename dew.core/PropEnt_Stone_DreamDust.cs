using Mirror;
using UnityEngine;

public class PropEnt_Stone_DreamDust : PropEntity
{
	public Formula amountByZoneIndex;

	public float rotSpeed;

	private EntityTransformModifier _entTransform;

	private float _angle;

	public override bool isRegularReward => true;

	public int GetAmount()
	{
		return DewMath.RandomRoundToInt(amountByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex));
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		_entTransform = Visual.GetNewTransformModifier();
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_entTransform != null)
		{
			_angle += rotSpeed * dt;
			_entTransform.rotation = Quaternion.Euler(0f, _angle, 0f);
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (((NetworkBehaviour)this).isServer)
		{
			int amount = GetAmount();
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, amount * DewPlayer.gamePlayers.Count, ((Component)(object)this).transform.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
