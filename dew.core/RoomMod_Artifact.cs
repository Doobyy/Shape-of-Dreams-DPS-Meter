using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class RoomMod_Artifact : RoomModifierBase
{
	public float dropChance = 0.1f;

	private Vector3 _lastKillPosition;

	private bool _didDrop;

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (isNewInstance)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath += new Action<EventInfoKill>(HandleMonsterDeath);
			SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(DropArtifact);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (((NetworkBehaviour)this).isServer && (UnityEngine.Object)(object)NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnDeath -= new Action<EventInfoKill>(HandleMonsterDeath);
		}
	}

	private void HandleMonsterDeath(EventInfoKill obj)
	{
		if (!_didDrop && obj.victim is Monster)
		{
			_lastKillPosition = obj.victim.agentPosition;
			if (UnityEngine.Random.value < dropChance)
			{
				DropArtifact();
			}
		}
	}

	private void DropArtifact()
	{
		if (!_didDrop)
		{
			_didDrop = true;
			((MonoBehaviour)(object)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1f));
			if (!((UnityEngine.Object)(object)this == null) && ((NetworkBehaviour)this).isServer && !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
			{
				Artifact byType = DewResources.GetByType<Artifact>(Dew.SelectRandomWeightedInReadOnlyList(NetworkedManagerBase<QuestManager>.instance.artifactPool, (Type t) => (!NetworkedManagerBase<QuestManager>.instance.undiscoveredArtifacts.Contains(t.Name)) ? 1f : 2.5f), default(ResourceLoadSettings));
				CreateActor(byType, Dew.GetGoodRewardPosition(_lastKillPosition), null);
				RemoveModifier();
			}
		}
	}

	public override bool IsAvailableInGame()
	{
		if (!NetworkedManagerBase<QuestManager>.instance.didCollectArtifactThisLoop)
		{
			return NetworkedManagerBase<QuestManager>.instance.currentArtifact == null;
		}
		return false;
	}

	private void MirrorProcessed()
	{
	}
}
