using System;
using UnityEngine;

public class Se_Q_Suppression_Old : StatusEffect
{
	public bool resetAtk;

	public float duration;

	public float knockupAmount;

	public float stunDuration;

	public float hitRadius;

	public float mainTargetProcCoefficient;

	public float subTargetsProcCoefficient;

	public ScalingValue mainDamage;

	public ScalingValue subDamage;

	public GameObject mainTargetEffect;

	public GameObject subTargetEffect;

	[NonSerialized]
	public bool enableAreaStun;

	protected override void OnCreate()
	{
	}

	private void MirrorProcessed()
	{
	}
}
