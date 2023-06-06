using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using UnityEngine;
using System.IO;
using UnityEditor;
namespace Framework.MVC
{

    [CustomEditor(typeof(WindowConfigMono))]
    public class WindowConfigInspectorEditor : Editor
    {
        SerializedProperty property;

        SerializedProperty _fixedOrderLayer;
        SerializedProperty _cache;
        SerializedProperty _openAnimation;
        SerializedProperty _loopAnimation;
        SerializedProperty _closeAnimation;
        SerializedProperty _window;
        SerializedProperty _parentWindow;
        SerializedProperty _windowType;
        SerializedProperty _mediator;

        private void OnEnable()
        {
            property = serializedObject.FindProperty("_viewConfig");
            _fixedOrderLayer = property.FindPropertyRelative("fixedOrderLayer");
            _cache = property.FindPropertyRelative("cache");
            _openAnimation = property.FindPropertyRelative("openAnimation");
            _loopAnimation = property.FindPropertyRelative("loopAnimation");
            _closeAnimation = property.FindPropertyRelative("closeAnimation");
            _window = property.FindPropertyRelative("window");
            _parentWindow = property.FindPropertyRelative("parentWindow");
            _windowType = property.FindPropertyRelative("windowType");
            _mediator = property.FindPropertyRelative("viewMediatorName");

            if (_windowType.enumValueIndex == -1)
            {
                _windowType.enumValueIndex = (int)EWindowType.Normal;
            }
            if (_window.enumValueIndex == -1)
            {
                _window.enumValueIndex = 0;
            }
            if (_parentWindow.enumValueIndex == -1)
            {
                _parentWindow.enumValueIndex = 0;
            }
        }

        

        public override void OnInspectorGUI()
        {
            
            WindowConfigMono view = (WindowConfigMono)target;
            _window.enumValueIndex = (int)(EWindow)EditorGUILayout.EnumPopup("Window", (EWindow)_window.enumValueIndex);
            _parentWindow.enumValueIndex = (int)(EWindow)EditorGUILayout.EnumPopup("Parent Window", (EWindow)_parentWindow.enumValueIndex);

            _windowType.enumValueIndex = (int)(EWindowType)EditorGUILayout.EnumPopup("Window Type", _windowType.enumValueIndex <= 0 ? EWindowType.Normal : (EWindowType)_windowType.enumValueIndex);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("mediator", _mediator.stringValue);
            if (GUILayout.Button("select"))
            {
                string path = PlayerPrefs.GetString("WindowConfigInspectorEditorFile", Application.dataPath);
                string mediator = EditorUtility.OpenFilePanel("select mediator", path, "cs");
                if (!string.IsNullOrEmpty(mediator))
                {
                    _mediator.stringValue = Path.GetFileNameWithoutExtension(mediator);
                    Type t = Assembly.GetAssembly(typeof(Framework.MVC.BaseViewWindow)).GetType(_mediator.stringValue);
                    if (t.BaseType != typeof(Framework.MVC.BaseViewMediator))
                    {
                        _mediator.stringValue = "";
                        EditorUtility.DisplayDialog("文件选择错误", $"{mediator}文件不是对应的中介器","确定");
                    }
                    PlayerPrefs.SetString("WindowConfigInspectorEditorFile", Path.GetDirectoryName(mediator));
                }
            }
            EditorGUILayout.EndHorizontal();
            if(_windowType.enumValueIndex == (int)EWindowType.Fixed)
            {
                _fixedOrderLayer.intValue = EditorGUILayout.IntField("fixedOrderLayer", _fixedOrderLayer.intValue);
            }

            _cache.boolValue = EditorGUILayout.Toggle("cache", _cache.boolValue);
            _openAnimation.objectReferenceValue = EditorGUILayout.ObjectField("open Ani",_openAnimation.objectReferenceValue, typeof(AnimationClip),false) as AnimationClip;
            _loopAnimation.objectReferenceValue = EditorGUILayout.ObjectField("loop Ani", _loopAnimation.objectReferenceValue, typeof(AnimationClip),false) as AnimationClip;
            _closeAnimation.objectReferenceValue = EditorGUILayout.ObjectField("close Ani", _closeAnimation.objectReferenceValue, typeof(AnimationClip), false) as AnimationClip;

            serializedObject.ApplyModifiedProperties();
        }
    }
}