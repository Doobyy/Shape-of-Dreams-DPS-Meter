using System.Collections;
using ch.sycoforge.Decal;
using ch.sycoforge.Decal.Wrapper;
using UnityEngine;

public class RoomMod_Limbo_Decorator : RoomModifierBase
{
	public DecorationSettings lv2InnerDeco = new DecorationSettings();

	public DecorationSettings lv2OuterDeco = new DecorationSettings();

	public DecorationSettings lv4InnerDeco = new DecorationSettings();

	public DecorationSettings lv4OuterDeco = new DecorationSettings();

	public DecorationSettings lv6InnerDeco = new DecorationSettings();

	public DecorationSettings lv6OuterDeco = new DecorationSettings();

	public int depthOverride;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		GameMod_Limbo gameMod_Limbo = Dew.FindActorOfType<GameMod_Limbo>();
		int num = 1;
		if ((Object)(object)gameMod_Limbo != null)
		{
			num = gameMod_Limbo.depth;
		}
		if (depthOverride > 0)
		{
			num = depthOverride;
		}
		Decorate(lv2InnerDeco, lv2OuterDeco);
		if (num >= 3)
		{
			Decorate(lv4InnerDeco, lv4OuterDeco);
		}
		if (num >= 5)
		{
			Decorate(lv6InnerDeco, lv6OuterDeco);
		}
		EasyDecal[] componentsInChildren = ((Component)(object)this).GetComponentsInChildren<EasyDecal>();
		foreach (EasyDecal obj in componentsInChildren)
		{
			((EasyDecal)obj).ProjectionDistance = ((EasyDecal)obj).ProjectionDistance * Random.Range(0.8f, 1.2f);
			((DecalBase)obj).Distance = ((DecalBase)obj).Distance * Random.Range(0.8f, 1.2f);
			((EasyDecal)obj).MaxDistance = ((EasyDecal)obj).MaxDistance * Random.Range(0.8f, 1.2f);
		}
		((MonoBehaviour)(object)this).StartCoroutine(Routine());
		void Decorate(DecorationSettings inner, DecorationSettings outer)
		{
			if (inner.decorations != null && inner.decorations.Length != 0)
			{
				spawnedDecorations.AddRange(RoomModifierBase.PlaceDecorations(inner, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices));
			}
			if (outer.decorations != null && outer.decorations.Length != 0)
			{
				spawnedDecorations.AddRange(RoomModifierBase.PlaceDecorations(outer, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.outerPropNodeIndices));
			}
		}
		IEnumerator Routine()
		{
			yield return null;
			yield return null;
			foreach (GameObject spawnedDecoration in spawnedDecorations)
			{
				EasyDecal[] componentsInChildren2 = spawnedDecoration.GetComponentsInChildren<EasyDecal>();
				foreach (EasyDecal val in componentsInChildren2)
				{
					if (((DecalBase)val).BakeOnAwake && !((DecalBase)val).Baked && (int)((DecalBase)val).Technique == 4)
					{
						((DecalBase)val).Technique = (ProjectionTechnique)0;
					}
				}
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
