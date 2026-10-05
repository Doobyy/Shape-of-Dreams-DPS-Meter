using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class St_Q_Fleche : SkillTrigger
{
	public int levelsPerCharge = 2;

	public bool doRefund;

	public float refundGraceTime = 0.5f;

	public GameObject refundEffect;

	[NonSerialized]
	public Dictionary<Entity, float> refundList = new Dictionary<Entity, float>();

	private SkillBonus _bonus;

	public override void OnStartServer()
	{
		base.OnStartServer();
		_bonus = new SkillBonus();
		AddSkillBonus(_bonus);
	}

	protected override void OnLevelChange(int oldLevel, int newLevel)
	{
		base.OnLevelChange(oldLevel, newLevel);
		if (((NetworkBehaviour)this).isServer)
		{
			_bonus.addedCharge = Mathf.Max((newLevel - 1) / levelsPerCharge, 0);
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!((NetworkBehaviour)this).isServer)
		{
			return;
		}
		List<Entity> list = null;
		foreach (KeyValuePair<Entity, float> refund in refundList)
		{
			if (Time.time - refund.Value > refundGraceTime)
			{
				if (list == null)
				{
					list = new List<Entity>();
				}
				list.Add(refund.Key);
			}
			else if (refund.Key.IsNullInactiveDeadOrKnockedOut())
			{
				if (list == null)
				{
					list = new List<Entity>();
				}
				list.Add(refund.Key);
				if (doRefund)
				{
					SetCharge(0, currentCharges[0] + 1);
					FxPlayNewNetworked(refundEffect, owner);
				}
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (Entity item in list)
		{
			refundList.Remove(item);
		}
	}

	private void MirrorProcessed()
	{
	}
}
