using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Paire;

[CustomEditor(typeof(Paire.RoomManager))]
public class RoomManagerEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("This script is responsbile for creating and joining rooms", MessageType.Info);

        Paire.RoomManager roomManager = (Paire.RoomManager)target;


        if (GUILayout.Button("Join School Room"))
        {
            roomManager.OnEnterButtonClicked_School();
        }

        if (GUILayout.Button("Join Outdoor Room"))
        {
            roomManager.OnEnterButtonClicked_Outdoor();
        }

    }
}
