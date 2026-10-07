using Mirror;
using UnityEngine;

[RequireComponent(typeof(GenericTransformSync))]
public class Ai_Mon_Primus_BossPrimusAeron_Adapt_Doom_PyranasFireball : TickDamageInstance
{
	public float acceleration = 10f;

	public float angleSpeedFar = 360f;

	public float angleSpeedNear = 60f;

	public float maxSpeed = 30f;

	private Vector3 _currentVel;

	private float _lastHitTime;

	public override bool reuseInRoom => true;

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		Hero hero = Dew.SelectRandomAliveHero(fallbackToDead: true, skipStealthed: true);
		if (!((Object)(object)hero == null))
		{
			Vector3 vector = hero.GetAIAgentPosition(info.caster) - position;
			float t = Mathf.Clamp01((vector.magnitude - 6f) / 5f);
			float num = Mathf.Lerp(angleSpeedNear, angleSpeedFar, t);
			if (_currentVel.magnitude < 0.0001f)
			{
				_currentVel = Random.insideUnitSphere * 0.25f;
			}
			if (Vector3.Dot(_currentVel, vector) >= 0f)
			{
				_currentVel = _currentVel.normalized * Mathf.Clamp(_currentVel.magnitude + acceleration * dt, 0f, maxSpeed);
			}
			else
			{
				_currentVel = _currentVel.normalized * Mathf.Clamp(_currentVel.magnitude - acceleration * dt, 0f, maxSpeed);
			}
			float y = Quaternion.LookRotation(vector).eulerAngles.y;
			float y2 = Quaternion.LookRotation(_currentVel, Vector3.up).eulerAngles.y;
			y2 = Mathf.MoveTowardsAngle(y2, y, num * dt);
			_currentVel = Quaternion.Euler(0f, y2, 0f) * Vector3.forward * _currentVel.magnitude;
			position += _currentVel * dt;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_currentVel = Vector3.zero;
	}

	private void MirrorProcessed()
	{
	}
}
