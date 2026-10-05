using UnityEngine;

public class EntityCallbackTrigger : MonoBehaviour
{
	public DewCollider owner;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (owner != null && DewPhysics.TryGetEntity(other, out var entity))
		{
			owner.HandleEntityEnter(entity);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (owner != null && DewPhysics.TryGetEntity(other, out var entity))
		{
			owner.HandleEntityExit(entity);
		}
	}
}
