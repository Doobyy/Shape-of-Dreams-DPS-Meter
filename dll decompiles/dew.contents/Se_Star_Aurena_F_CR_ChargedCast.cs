using System;
using Mirror;

public class Se_Star_Aurena_F_CR_ChargedCast : StarEffect
{
	public DewAnimationClip chargeStart;

	public override Type heroType => typeof(Hero_Aurena);

	public override Type skillType => typeof(St_R_ChainReaction);

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		DoSkill((SkillTrigger s) =>
		{
			TriggerConfig original = s.configs[0];
			s.configs[0] = s.configs[0].Clone();
			s.configs[0].channel.duration = 0f;
			s.configs[0].effectOnCast = null;
			s.configs[0].castMethod.type = CastMethodType.None;
			s.configs[0].startAnim = chargeStart;
			s.configs[0].endAnim = null;
			s.configs[0].spawnedInstance = DewResources.GetByType<Ai_Star_Aurena_F_CR_ChargedCast_InstanceCharger>(default(ResourceLoadSettings));
			return () =>
			{
				s.configs[0] = original;
			};
		});
	}

	private void MirrorProcessed()
	{
	}
}
