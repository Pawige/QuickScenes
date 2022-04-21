using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace QuickScenes
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("DropDown")
            {
                //alignment = TextAnchor.MiddleCenter,
                //imagePosition = ImagePosition.ImageAbove,
                //fontStyle = FontStyle.Bold,
                fixedHeight = 20,
                fixedWidth = 100
            };
        }
    }

    [InitializeOnLoad]
    public class ToolbarScenePicker
    {
        private static GenericMenu _loadMenu;
        private static GUIContent _loadDropdown = new GUIContent("Load Scene");
        private static GUIContent _addDropdown = new GUIContent("Add Scene");
        private static bool _additiveLoad;

        static ToolbarScenePicker()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            InitSceneMenu();
        }

        static void OnToolbarGUI()
        {
            if (EditorGUILayout.DropdownButton(_loadDropdown, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = false;
                _loadMenu.ShowAsContext();
            }
            if (EditorGUILayout.DropdownButton(_addDropdown, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = true;
                _loadMenu.ShowAsContext();
            }
            //GUILayout.FlexibleSpace();
        }
        
        private static void InitSceneMenu()
        {
            _loadMenu = new GenericMenu();
            List<SceneFolder> sceneList = Utility.GenerateSceneLists();
            foreach (SceneFolder sceneFolder in sceneList)
            {
                foreach (string sceneGuid in sceneFolder.SceneGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    string[] scenePathSplit = path.Split('/', '.');
                    // Reminder: [^2] is the same as writing [scenePathSplit.Length - 2];
                    string sceneName = scenePathSplit[^2];
                    _loadMenu.AddItem(new GUIContent($"{sceneFolder.FolderName}/{sceneName}"), false, LoadSceneWithGuid, sceneGuid);
                }
            }
        }

        private static void LoadSceneWithGuid(object sceneGuid)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath((string)sceneGuid);
            if (_additiveLoad)
                Utility.AddScene(scenePath);
            else 
                Utility.LoadScene(scenePath);
        }
    }
}