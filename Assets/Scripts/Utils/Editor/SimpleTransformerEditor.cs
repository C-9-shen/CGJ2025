using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SimpleTransform))]
public class SimpleTransformDrawer : PropertyDrawer
{
    private Vector2 scrollPosition; // 滚动位置变量

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 开始滚动视图，动态调整高度
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        var transTypeProperty = property.FindPropertyRelative("TransType");
        var UseWorldSpaceProperty = property.FindPropertyRelative("UseWorldSpace");
        var startPositionProperty = property.FindPropertyRelative("startPosition");
        var endPositionProperty = property.FindPropertyRelative("endPosition");
        var startRotationProperty = property.FindPropertyRelative("startRotation");
        var endRotationProperty = property.FindPropertyRelative("endRotation");
        var startScaleProperty = property.FindPropertyRelative("startScale");
        var endScaleProperty = property.FindPropertyRelative("endScale");
        var startAlphaProperty = property.FindPropertyRelative("startAlpha");
        var endAlphaProperty = property.FindPropertyRelative("endAlpha");
        var alphaEventProperty = property.FindPropertyRelative("alphaEvent");
        var activeEventProperty = property.FindPropertyRelative("activeEvent");
        var StartOrderProperty = property.FindPropertyRelative("startOrder");   
        var TargetOrderProperty = property.FindPropertyRelative("TargetOrder");
        var updateTypeProperty = property.FindPropertyRelative("updateType");
        var durationProperty = property.FindPropertyRelative("duration");
        var delayProperty = property.FindPropertyRelative("delay");
        var curveProperty = property.FindPropertyRelative("curve");

        //draw split line
        EditorGUILayout.LabelField(label.text, EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(transTypeProperty);
        if (transTypeProperty.enumValueIndex < 4 && transTypeProperty.enumValueIndex > 0)
            EditorGUILayout.PropertyField(UseWorldSpaceProperty);

        TransformType transType = (TransformType)transTypeProperty.enumValueIndex;

        if (transType == TransformType.Translate)
        {
            EditorGUILayout.PropertyField(startPositionProperty);
            EditorGUILayout.PropertyField(endPositionProperty);
        }
        else if (transType == TransformType.Rotate)
        {
            EditorGUILayout.PropertyField(startRotationProperty);
            EditorGUILayout.PropertyField(endRotationProperty);
        }
        else if (transType == TransformType.Scale)
        {
            EditorGUILayout.PropertyField(startScaleProperty);
            EditorGUILayout.PropertyField(endScaleProperty);
        }
        else if (transType == TransformType.Alpha)
        {
            EditorGUILayout.PropertyField(startAlphaProperty);
            EditorGUILayout.PropertyField(endAlphaProperty);
            EditorGUILayout.PropertyField(alphaEventProperty, true);
        }
        else if (transType == TransformType.Active)
        {
            EditorGUILayout.PropertyField(activeEventProperty, true);
        }
        else if (transType == TransformType.Order)
        {
            EditorGUILayout.PropertyField(StartOrderProperty);
            EditorGUILayout.PropertyField(TargetOrderProperty);
        }

        EditorGUILayout.PropertyField(updateTypeProperty);
        EditorGUILayout.PropertyField(durationProperty);
        EditorGUILayout.PropertyField(delayProperty);

        if ((UpdateType)updateTypeProperty.enumValueIndex == UpdateType.Spline)
        {
            EditorGUILayout.PropertyField(curveProperty);
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 结束滚动视图
        GUILayout.EndScrollView();

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = 0f;

        var transTypeProperty = property.FindPropertyRelative("TransType");
        var updateTypeProperty = property.FindPropertyRelative("updateType");

        // totalHeight += EditorGUIUtility.singleLineHeight; // For label

        totalHeight += EditorGUI.GetPropertyHeight(transTypeProperty);

        TransformType transType = (TransformType)transTypeProperty.enumValueIndex;

        if (transType == TransformType.None ||transType == TransformType.Translate 
        || transType == TransformType.Rotate || transType == TransformType.Scale 
        || transType == TransformType.Alpha || transType == TransformType.Active 
        || transType == TransformType.Order)
        {
            totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("UseWorldSpace"), true);
        }
        {
            totalHeight += EditorGUIUtility.singleLineHeight * 2; // For start and end properties
            totalHeight -= EditorGUIUtility.singleLineHeight * 7;
        }

        if (transType == TransformType.Alpha)
        {
            // totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alphaEvent"), true);
            // totalHeight -= EditorGUIUtility.singleLineHeight * 5;
        }

        else if (transType == TransformType.Active)
        {
            // totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("activeEvent"), true);
            // totalHeight -= EditorGUIUtility.singleLineHeight * 5;
        }

        totalHeight += EditorGUI.GetPropertyHeight(updateTypeProperty);

        if ((UpdateType)updateTypeProperty.enumValueIndex == UpdateType.Spline)
        {
            totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("curve"));
        }

        totalHeight += EditorGUIUtility.singleLineHeight * 3; // For duration, delay, and padding

        return totalHeight;
    }
}