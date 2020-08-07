using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EazyParallax))]
public class EazyParallaxEditor : Editor
{
    EazyParallax parallax;

    private void OnEnable()
    {
        parallax = target as EazyParallax;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
       base.OnInspectorGUI();

        //var prop = serializedObject.FindProperty("side");
        //EditorGUILayout.PropertyField(prop);
        // prop = serializedObject.FindProperty("distance");
        //EditorGUILayout.PropertyField(prop);
        // prop = serializedObject.FindProperty("startSpeed");
        //EditorGUILayout.PropertyField(prop);
        // prop = serializedObject.FindProperty("maxSpeed");
        //EditorGUILayout.PropertyField(prop);
        // prop = serializedObject.FindProperty("durationToMaxSpeed");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("durationAtMaxSpeed");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("scroll");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("timeStop");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("countRoll");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("curveStop");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("isForever");
        //EditorGUILayout.PropertyField(prop);
        //prop = serializedObject.FindProperty("isFit");
        //EditorGUILayout.PropertyField(prop);
        //if (!prop.boolValue)
        //{
        //    prop = serializedObject.FindProperty("sizeControll");
        //    EditorGUILayout.PropertyField(prop);
        //}
        if (GUILayout.Button("Resort Position"))
        {
            parallax.cacheElement();
            parallax.resortElement();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
