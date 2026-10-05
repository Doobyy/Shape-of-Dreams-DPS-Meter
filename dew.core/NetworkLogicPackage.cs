using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkLogicPackage : ManagerBase<NetworkLogicPackage>
{
	public List<NetworkIdentity> networkIdentities;

	protected override void Awake()
	{
		base.Awake();
		networkIdentities = GetComponentsInChildren<NetworkIdentity>(includeInactive: true).ToList();
	}

	private void Start()
	{
		if (ManagerBase<NetworkLogicPackage>.instance != null && ManagerBase<NetworkLogicPackage>.instance != this)
		{
			Object.Destroy(gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(gameObject);
		}
	}

	private void OnDestroy()
	{
		NetworkIdentity[] array = Object.FindObjectsOfType<NetworkIdentity>(true);
		foreach (NetworkIdentity val in array)
		{
			if ((!(DewResources.variantsParent != null) || !(((Component)(object)val).transform.parent == DewResources.variantsParent)) && (!(ManagerBase<SpawnManager>.softInstance != null) || !(((Component)(object)val).transform.parent == ManagerBase<SpawnManager>.softInstance.transform)))
			{
				Object.Destroy(((Component)(object)val).gameObject);
			}
		}
	}
}
