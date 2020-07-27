using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(I2String))]
public class I2StringEditor : PropertyDrawer
{
    float pHeight = 0;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        pHeight = 0;
        Rect pRect = position;
        pRect.height = 18;
        pRect.width -= 20;
        SerializedProperty prop = property.FindPropertyRelative("normalString");
        EditorGUI.PropertyField(pRect, prop, label);
        pRect.x  += pRect.width;
        pRect.width = 20;
        prop = property.FindPropertyRelative("isI2String");
        EditorGUI.PropertyField(pRect, prop, new GUIContent(""));
        pHeight += 20;
        pRect.y += 20;
        pRect.x = position.x;
        pRect.width = position.width;
        if (prop.boolValue)
        {
            
            prop = property.FindPropertyRelative("value");
            prop.stringValue = I2.Loc.LocalizationManager.GetTranslation(property.FindPropertyRelative("normalString").stringValue);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(pRect, prop, new GUIContent(""));
            EditorGUI.EndDisabledGroup();
            pHeight += 20;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return pHeight;
    }
}
