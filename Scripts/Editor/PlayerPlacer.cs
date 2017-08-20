using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(PlayerHandler))]
[CanEditMultipleObjects]
public class PlayerPlacer : Editor
{
	Transform player;
	const float cursorUpOffset = 35;		//ugh
	const float playerUpOffset = 1.75f;
	const int playerIgnoreLayer = -257;

	void OnEnable()
	{
		player = GameObject.FindWithTag("Player").transform;
	}

	void OnSceneGUI()
	{
		if (player != null && Event.current.control && Camera.current != null)
		{
			RaycastHit hit;

			Vector2 mousePos = Event.current.mousePosition;
			mousePos.y = Screen.height - mousePos.y - cursorUpOffset;

			if (mousePos.x < 0 || mousePos.x > Screen.width) return;
			if (mousePos.y < 0 || mousePos.y > Screen.height - cursorUpOffset) return;

			Ray ray = Camera.current.ScreenPointToRay(mousePos);
			if (Physics.Raycast(ray, out hit, 10000, playerIgnoreLayer, QueryTriggerInteraction.Ignore))
				PlacePlayer(hit.point);
		}
	}

	void PlacePlayer(Vector3 putPos)
	{
		Undo.RecordObject(player.transform, "Player Move");
		player.position = putPos + (Vector3.up * playerUpOffset);
	}
}
