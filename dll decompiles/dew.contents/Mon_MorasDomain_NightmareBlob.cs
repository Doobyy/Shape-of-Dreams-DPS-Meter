using System.Runtime.CompilerServices;
using UnityEngine;

public class Mon_MorasDomain_NightmareBlob : Monster
{
	private float _nextDecayTime;

	public static int instancesCount
	{
		[CompilerGenerated]
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[CompilerGenerated]
		private set
		{
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
	}

	protected override void OnCreate()
	{
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	public static void SpawnByProjectile(Vector3 from, Vector3 to)
	{
	}

	private void MirrorProcessed()
	{
	}
}
