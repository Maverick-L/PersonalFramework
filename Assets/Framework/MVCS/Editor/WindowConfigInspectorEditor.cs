using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using UnityEngine;
using System.IO;

namespace Framework.MVC
{
    [CustomEditor(typeof(BaseViewWindow))]
    public class WindowConfigInspectorEditor : Editor
    {

        public override void OnInspectorGUI()

        {
            BaseViewWindow view = (BaseViewWindow)target;
            FieldInfo field = view.GetType().GetField("_viewConfig", BindingFlags.Instance | BindingFlags.NonPublic);
            WindowConfig config = (WindowConfig)field.GetValue(view);

            config.window = (EWindow)EditorGUILayout.EnumPopup("Window",config.window);
            config.parentWindow = (EWindow)EditorGUILayout.EnumPopup("Parent Window", config.parentWindow);
            
            config.windowType = (EWindowType)EditorGUILayout.EnumPopup("WindowType", config.windowType);

            EditorGUILayout.BeginHorizontal();
            config.viewMediatorName = EditorGUILayout.TextField("mediator", config.viewMediatorName);
            if (GUILayout.Button("select"))
            {
                string path = PlayerPrefs.GetString("WindowConfigInspectorEditorFile", Application.dataPath);
                string mediator = EditorUtility.OpenFilePanel("select mediator", path, "cs");
                if (!string.IsNullOrEmpty(mediator))
                {
                    config.viewMediatorName =Path.GetFileNameWithoutExtension(mediator);
                    PlayerPrefs.SetString("WindowConfigInspectorEditorFile", Path.GetDirectoryName(mediator));
                }
            }
            EditorGUILayout.EndHorizontal();
            if(config.windowType == EWindowType.Fixed)
            {
                config.fixedOrderLayer = EditorGUILayout.IntField("fixedOrderLayer",config.fixedOrderLayer);
            }

            config.cache = EditorGUILayout.Toggle("cache", config.cache);

            config.openAnimation = EditorGUILayout.ObjectField("open Ani", config.openAnimation, typeof(AnimationClip),false) as AnimationClip;
            config.loopAnimation = EditorGUILayout.ObjectField("loop Ani", config.openAnimation, typeof(AnimationClip),false) as AnimationClip;
            config.closeAnimation = EditorGUILayout.ObjectField("close Ani", config.openAnimation, typeof(AnimationClip), false) as AnimationClip;

            field.SetValue(view, config);
        }
    }
}