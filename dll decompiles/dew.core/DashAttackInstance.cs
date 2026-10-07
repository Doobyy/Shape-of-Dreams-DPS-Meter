using System;
using Mirror;
using UnityEngine;

public class DashAttackInstance : DamageInstance
{
	public float damageGracePeriod = 0.075f;

	public bool isDirectionMode;

	public Dash dash;

	private Quaternion _rotation;

	private bool _isDashing;

	private Vector3 _damageDirection;

	private Vector3 _lastPosition;

	[NonSerialized]
	public DispByDestination currentDisplacement;

	protected override void OnCreate()
	{
		base.OnCreate();
		Entity caster = info.caster;
		Vector3 vector = (_lastPosition = caster.position);
		if (isDirectionMode)
		{
			_rotation = info.rotation;
		}
		else
		{
			_rotation = Quaternion.LookRotation(info.point - vector).Flattened();
		}
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			return;
		}
		_isDashing = true;
		if (isDirectionMode)
		{
			_damageDirection = info.forward;
			currentDisplacement = dash.ApplyByDirection(caster, info.forward, OnSetupDash);
		}
		else
		{
			Vector3 point = info.point;
			_damageDirection = (point - vector).normalized;
			currentDisplacement = dash.ApplyByDestination(caster, point, OnSetupDash);
		}
		if (damageGracePeriod < 1E-05f)
		{
			DoCollisionChecks();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isDashing = false;
		currentDisplacement = null;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		((Component)(object)this).transform.SetPositionAndRotation(info.caster.position, _rotation);
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer || !_isDashing || Time.time - creationTime < damageGracePeriod)
		{
			return;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			return;
		}
		Vector3 localPosition = range.transform.localPosition;
		Vector3 vector = _lastPosition + ((Component)(object)this).transform.rotation * localPosition;
		Vector3 vector2 = range.transform.position;
		float num = 0f;
		float num2 = Vector3.Distance(vector, vector2);
		float num3 = 2.5f;
		switch (range.shape)
		{
		case DewCollider.ColliderShape.Circle:
			num3 = range.radius;
			break;
		case DewCollider.ColliderShape.Box:
			num3 = Mathf.Min(range.size.x, range.size.y) * 0.5f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case DewCollider.ColliderShape.Polygon:
			break;
		}
		range.transform.position = vector;
		do
		{
			DoCollisionChecks();
			num += num3;
			range.transform.position += (vector2 - vector).normalized * num3;
		}
		while (!(num > num2));
		range.transform.localPosition = localPosition;
		_lastPosition = position;
	}

	protected virtual void OnSetupDash(DispByDestination disp)
	{
		if (info.caster.Status.hasImmobility)
		{
			disp.destination = info.caster.agentPosition + (disp.destination - info.caster.agentPosition).normalized * 0.01f;
		}
		disp.onFinish = (Action)Delegate.Combine(disp.onFinish, (Action)(() =>
		{
			ActiveLogicUpdate(0f);
			_isDashing = false;
			if (destroyWhenDone && isActive)
			{
				Destroy();
			}
		}));
		disp.onCancel = (Action)Delegate.Combine(disp.onCancel, (Action)(() =>
		{
			ActiveLogicUpdate(0f);
			_isDashing = false;
			if (destroyWhenDone && isActive)
			{
				Destroy();
			}
		}));
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		dmg.SetDirection(_damageDirection);
	}

	private void MirrorProcessed()
	{
	}
}
