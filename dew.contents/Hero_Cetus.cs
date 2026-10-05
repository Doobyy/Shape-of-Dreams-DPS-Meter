using UnityEngine;

public class Hero_Cetus : Hero
{
	public override void OnModelLoaded()
	{
		base.OnModelLoaded();
		if ((bool)Visual.model)
		{
			PhysicBonesCore[] componentsInChildren = Visual.model.GetComponentsInChildren<PhysicBonesCore>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Behaviour)(object)componentsInChildren[i]).enabled = false;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
