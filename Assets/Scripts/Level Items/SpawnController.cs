using UnityEngine;
using System.Collections;

public class SpawnController : MonoBehaviour {
	
	public float spawnRotation = 0f;
	
	public void Awake()
	{
		Vector3 pos = transform.position;
		EventDispatcher.SendEvent(EventKey.PLAYER_SET_CHECKPOINT, new Vector4(pos.x, pos.y, pos.z, spawnRotation));
	}
	
	void OnDrawGizmosSelected()
	{
		Vector2 rotVec = VectorEx.AngleToVector(spawnRotation - 180f);
		GizmoEx.DrawNormal(transform.position, new Vector3(rotVec.x, 0f, rotVec.y), 2f);
	}
}
