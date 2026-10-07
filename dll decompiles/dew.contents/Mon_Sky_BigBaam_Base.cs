using UnityEngine;

public class Mon_Sky_BigBaam_Base : Monster
{
	public float fleeChance;

	public float fleeStartDistance;

	public float fleeDisableAITime = 4f;

	public Vector2 fleeDistance;

	public float fleeCooldownTime;

	public float fleeLookDuration;

	public float fleeAngleDeviation;

	public int fleeDestinationSteps;

	public bool activelyChase;

	private float _lastFleeTime;

	protected override void AIUpdate(ref EntityAIContext context)
	{
		base.AIUpdate(ref context);
		if (!((Object)(object)context.targetEnemy == null) && !MainSkillRoutine() && !(Time.time - _lastFleeTime < fleeDisableAITime))
		{
			if (Time.time > _lastFleeTime + fleeCooldownTime && Random.value < fleeChance * context.deltaTime && Vector2.Distance(context.targetEnemy.agentPosition.ToXY(), agentPosition.ToXY()) < fleeStartDistance)
			{
				_lastFleeTime = Time.time;
				Vector3 fleeDestination = GetFleeDestination();
				Control.MoveToDestination(fleeDestination, immediately: true);
				Control.RotateTowards(context.targetEnemy, immediately: false, fleeLookDuration);
			}
			else if (AI.Helper_CanBeCast<At_Mon_Sky_BigBaam_Melee>() && AI.Helper_IsTargetInRange<At_Mon_Sky_BigBaam_Melee>())
			{
				AI.Helper_CastAbilityAuto<At_Mon_Sky_BigBaam_Melee>();
			}
			else if (activelyChase || AI.Helper_IsTargetInRangeOfAttack())
			{
				AI.Helper_ChaseTarget();
			}
		}
	}

	protected virtual Vector3 GetFleeDestination()
	{
		Vector3 vector = AI.context.targetEnemy.agentPosition;
		Vector3 vector2 = agentPosition;
		float num = float.NegativeInfinity;
		Vector3 result = agentPosition;
		float num2 = Random.Range(fleeDistance.x, fleeDistance.y);
		for (int i = 0; i < fleeDestinationSteps; i++)
		{
			float y = Mathf.Lerp(0f - fleeAngleDeviation, fleeAngleDeviation, (float)i / (float)(fleeDestinationSteps - 1));
			Vector3 end = vector2 + Quaternion.Euler(0f, y, 0f) * (vector2 - vector).normalized * num2;
			end = Dew.GetValidAgentDestination_Closest(vector2, end);
			float num3 = Vector2.Distance(vector2.ToXY(), end.ToXY());
			float num4 = 100f - Mathf.Abs(num3 - num2) + Random.value;
			if (!(num4 <= num))
			{
				num = num4;
				result = end;
			}
		}
		return result;
	}

	protected virtual bool MainSkillRoutine()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
