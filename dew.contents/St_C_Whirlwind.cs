public class St_C_Whirlwind : SkillTrigger
{
	public override AbilityInstance OnCastComplete(int configIndex, CastInfo info)
	{
		if (configIndex == 1)
		{
			Actor[] array = children.ToArray();
			foreach (Actor actor in array)
			{
				if (actor is Ai_C_Whirlwind)
				{
					actor.Destroy();
				}
			}
		}
		return base.OnCastComplete(configIndex, info);
	}

	private void MirrorProcessed()
	{
	}
}
