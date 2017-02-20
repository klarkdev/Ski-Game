using System;
using UnityEditor;
using UnityEngine;

namespace DsLib
{
    [CustomPropertyDrawer(typeof(DsLib.CyclicTimer))]
    public class CyclicTimerDrawer : PropertyDrawer
    {
        const float labelWidth = 75f;
        const float fieldSpace = 3f;
        const float lineSpace = 3f;
        const float lineHeight = 16f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = position;
            foldoutRect.height = lineHeight;
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            EditorGUI.indentLevel++;
            Rect contentRect = EditorGUI.PrefixLabel(position, GUIContent.none);
            EditorGUI.indentLevel = 0;

            if (property.isExpanded)
            {
                float xCurrent = contentRect.x;
                float yCurrent = position.y + lineHeight;
                float fieldWidth = (contentRect.width - fieldSpace) / 2;

                // Line One
                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, labelWidth, lineHeight), "Warmup");
                xCurrent += labelWidth;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, fieldWidth - labelWidth - fieldSpace, lineHeight), property.FindPropertyRelative("warmupTime"), GUIContent.none);
                xCurrent += fieldWidth - labelWidth + fieldSpace;

                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, (fieldWidth - fieldSpace) - 14, lineHeight), "Loop Iterations");
                xCurrent += (fieldWidth - fieldSpace) - 14;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, 14, lineHeight), property.FindPropertyRelative("loopIterations"), GUIContent.none);
                xCurrent += 14 + fieldSpace;

                yCurrent += lineHeight + lineSpace;
                xCurrent = contentRect.x;

                // Line Two
                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, labelWidth, lineHeight), "It. Padding");
                xCurrent += labelWidth;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, fieldWidth - labelWidth - fieldSpace, lineHeight), property.FindPropertyRelative("iterationPadding"), GUIContent.none);
                xCurrent += fieldWidth - labelWidth + fieldSpace;

                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, labelWidth, lineHeight), "Iterations");
                xCurrent += labelWidth;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, fieldWidth - labelWidth, lineHeight), property.FindPropertyRelative("iterations"), GUIContent.none);

                yCurrent += lineHeight + lineSpace;
                xCurrent = contentRect.x;

                // Line Three

                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, labelWidth, lineHeight), "Cooldown");
                xCurrent += labelWidth;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, fieldWidth - labelWidth - fieldSpace, lineHeight), property.FindPropertyRelative("cooldownTime"), GUIContent.none);
                xCurrent += fieldWidth - labelWidth + fieldSpace;

                EditorGUI.LabelField(new Rect(xCurrent, yCurrent, (fieldWidth - fieldSpace) - 14, lineHeight), "Iteration/Update Limit");
                xCurrent += (fieldWidth - fieldSpace) - 14;
                EditorGUI.PropertyField(new Rect(xCurrent, yCurrent, 14, lineHeight), property.FindPropertyRelative("limitUpdate"), GUIContent.none);

                yCurrent += lineHeight + lineSpace;
                xCurrent = contentRect.x;

                // Line Four

                float timeStarted = property.FindPropertyRelative("cycleTimeStarted").floatValue;
                float timeRemaining = property.FindPropertyRelative("cycleTimeRemaining").floatValue;

                int iterationsRemaining = property.FindPropertyRelative("iterationsRemaining").intValue;

                float remainingPercent = 0f;
                if (timeStarted != 0)
                    remainingPercent = Mathf.Clamp(timeRemaining / timeStarted, 0.0f, 1f);

                string timeLeft = String.Format("{0:0.00}", timeRemaining) + " / " + String.Format("{0:0.00}", timeStarted);

                int state = property.FindPropertyRelative("state").intValue;
                string stateString = ((DsLib.CyclicTimer.State)state).ToString();

                EditorGUI.ProgressBar(new Rect(xCurrent + fieldSpace, yCurrent, contentRect.width - fieldSpace, lineHeight), remainingPercent,
                    timeLeft + " (+" + iterationsRemaining + ") (" + stateString + ")");

            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
                return lineHeight * 5 + lineSpace * 3;
            else
                return lineHeight;
        }
    }
}