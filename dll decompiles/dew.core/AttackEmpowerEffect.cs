using System;

public class AttackEmpowerEffect : BasicEffect
{
	public Action<EventInfoAttackEffect, int> onAttackEffect;

	public Action onDepleted;

	public int maxTriggerCount = int.MaxValue;

	internal int _nextIndex;

	public override BasicEffectMask mask => BasicEffectMask.AttackEmpower;
}
