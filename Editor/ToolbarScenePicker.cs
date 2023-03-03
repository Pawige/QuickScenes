using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityToolbarExtender;

namespace QuickScenes
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;
        public static readonly GUIStyle favoriteButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("DropDown")
            {
                fixedHeight = 20,
                fixedWidth = 100
            };
            favoriteButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 20,
                fixedWidth = 21,
                padding = new RectOffset(4,3,4,4),
            };
        }
    }

    [InitializeOnLoad]
    public class ToolbarScenePicker
    {
        private static readonly GUIContent _loadDropdown = new GUIContent("Load Scene");
        private static readonly GUIContent _addDropdown = new GUIContent("Add Scene");
        private static GUIContent _favorite;
        private static bool _additiveLoad;
        private static List<SceneFolder> _sceneList;
        private static SceneSelectionDropdown _dropdownSelectionMenu;

        static ToolbarScenePicker()
        {
            Texture star = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Refresh.tga");
            _favorite = new GUIContent(star);
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            InitSceneMenu();
        }

        static void OnToolbarGUI()
        {
            // Some ugly weird magic numbers in here but it works for now ¯\_(ツ)_/¯
            var dropdownRect = new Rect
            {
                width = EditorGUIUtility.singleLineHeight * 15,
                x = 0,
                yMax = Screen.height - EditorGUIUtility.singleLineHeight * 1.25f
            };
            if (EditorGUILayout.DropdownButton(_loadDropdown, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = false;
                AddDropdown(dropdownRect, "Load Scene");
            }
            if (EditorGUILayout.DropdownButton(_addDropdown, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = true;
                dropdownRect.x += ToolbarStyles.commandButtonStyle.fixedWidth;
                AddDropdown(dropdownRect, "Add Scene");
            }

            if (GUILayout.Button(_favorite, ToolbarStyles.favoriteButtonStyle))
            {
                InitSceneMenu();
            }
        }
        
        private static void AddDropdown(Rect dropdownRect, string title)  
        {  
            _dropdownSelectionMenu = new SceneSelectionDropdown(new AdvancedDropdownState(), title, _sceneList);  
            _dropdownSelectionMenu.SelectionMade += SelectionMade;  
            _dropdownSelectionMenu.Show(dropdownRect);  
        }  
  
        private static void SelectionMade(string selectedSceneGuid)  
        {  
            LoadSceneWithGuid(selectedSceneGuid);  
            _dropdownSelectionMenu.SelectionMade -= SelectionMade;  
        }
        
        private static void InitSceneMenu()
        {
            _sceneList = Utility.GenerateSceneLists();
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