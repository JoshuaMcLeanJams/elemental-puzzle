/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEditor : Editor
{
    [System.Serializable]
    class Coord
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    class TileData
    {
        public string element;
        public int level;
        public Coord pos;
    }

    [System.Serializable]
    class LevelData
    {
        public Coord size;
        public TileData[] crystalList;
    }

    private int m_level = 0;
    private LevelData m_curLevelData = null;

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Level" );
        m_level = EditorGUILayout.IntField( m_level );
        EditorGUILayout.EndHorizontal();

        if( GUILayout.Button("Load Level") ) {
            // TODO load level data from JSON
        }

        if ( m_curLevelData == null ) return;
        // TODO construct level data display

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
*/
