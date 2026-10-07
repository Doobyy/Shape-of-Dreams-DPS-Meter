using System;

public class St_QR_InfernalTales : SkillTrigger
{
	private Ai_QR_InfernalTales _current;

	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		AbilityInstance abilityInstance = base.OnCastComplete(configIndex, info);
		if (!(abilityInstance is Ai_QR_InfernalTales ai_QR_InfernalTales))
		{
			return abilityInstance;
		}
		_current = ai_QR_InfernalTales;
		fillAmount = 0f;
		ai_QR_InfernalTales.ClientActorEvent_OnDestroyed += (Action<Actor>)((Actor _) =>
		{
			fillAmount = 0f;
			_current = null;
		});
		return abilityInstance;
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!_current.IsNullOrInactive())
		{
			fillAmount = 1f - _current.normalizedDuration;
		}
	}

	private void MirrorProcessed()
	{
	}
}
