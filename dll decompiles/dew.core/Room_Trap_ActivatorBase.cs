using System;
using System.Collections;
using Mirror;
using UnityEngine;

public abstract class Room_Trap_ActivatorBase : DewNetworkBehaviour
{
	public bool toggleTraps;

	public float durationOfToggledTraps;

	public GameObject[] activatedObjects;

	public GameObject fxActivateEffect;

	[Server]
	public void Activate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_ActivatorBase::Activate()' called when server was not active");
			return;
		}
		FxPlayNetworked(fxActivateEffect);
		HandleGameObject(((Component)(object)this).gameObject);
		GameObject[] array = activatedObjects;
		foreach (GameObject gobj in array)
		{
			HandleGameObject(gobj);
		}
		void HandleGameObject(GameObject gameObject)
		{
			if (!(gameObject == null))
			{
				ListReturnHandle<IActivatableTrap> handle;
				foreach (IActivatableTrap item in gameObject.GetComponentsNonAlloc(out handle))
				{
					if (ValidateObject(item))
					{
						try
						{
							item.ActivateTrap();
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
				handle.Return();
				ListReturnHandle<IToggleableTrap> handle2;
				foreach (IToggleableTrap item2 in gameObject.GetComponentsNonAlloc(out handle2))
				{
					IToggleableTrap t = item2;
					if (ValidateObject(t))
					{
						try
						{
							if (toggleTraps)
							{
								if (t.isOn)
								{
									t.StopTrap();
								}
								else
								{
									t.StartTrap();
								}
							}
							else
							{
								((MonoBehaviour)(object)this).StartCoroutine(Routine());
							}
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
					IEnumerator Routine()
					{
						t.StartTrap();
						yield return new WaitForSeconds(durationOfToggledTraps);
						if (ValidateObject(t))
						{
							t.StopTrap();
						}
					}
				}
				handle2.Return();
			}
		}
		static bool ValidateObject(object obj)
		{
			if (obj is Actor { isActive: false })
			{
				return false;
			}
			if (obj is MonoBehaviour { isActiveAndEnabled: false })
			{
				return false;
			}
			NetworkBehaviour val = (NetworkBehaviour)((obj is NetworkBehaviour) ? obj : null);
			if (val != null && !val.isServer)
			{
				return false;
			}
			return true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
