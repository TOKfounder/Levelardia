using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor
{

	public override void OnInspectorGUI()
	{
		MapGenerator map = target as MapGenerator;
		// base.OnInspectorGUI();
		if (DrawDefaultInspector())
		{
			map.GenerateMap ();
		}

		if (GUILayout.Button("Generate Map"))
		{
			map.GenerateMap ();
		}
	}
}
