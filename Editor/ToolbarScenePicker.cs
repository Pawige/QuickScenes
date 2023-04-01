using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace QuickScenes
{
    [InitializeOnLoad]
    public class ToolbarScenePicker
    {
        private static readonly GUIContent _loadDropdownContent = new GUIContent("Load Scene", "Pick a scene to replace whatever is loaded.");
        private static readonly GUIContent _addDropdownContent = new GUIContent("Add Scene", "Pick a scene to load additively.");
        private static readonly GUIContent _favoriteContent;
        private static readonly GUIContent _notFavoriteContent;
        private static bool _additiveLoad;
        private static List<SceneFolder> _sceneList;
        private static SceneSelectionDropdown _dropdownSelectionMenu;
        private static SavedData _cachedData;

        static ToolbarScenePicker()
        {
            Texture star = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star.tga");
            Texture starOutline = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star_Outline.tga");
            _favoriteContent = new GUIContent(star, "Remove active scene from favorites list.");
            _notFavoriteContent = new GUIContent(starOutline, "Add active scene to favorites list.");
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            InitSceneMenu();
            _cachedData = Utility.GetSavedData();
        }

        static void OnToolbarGUI()
        {
            if (Utility.DirtyFromWindow)
            {
                Utility.DirtyFromWindow = false;
                _cachedData = Utility.GetSavedData();
            }
            
            // Some ugly weird magic numbers in here but it works for now ¯\_(ツ)_/¯
            var dropdownRect = new Rect
            {
                width = EditorGUIUtility.singleLineHeight * 15,
                x = 0,
                yMax = Screen.height - EditorGUIUtility.singleLineHeight * 1.25f
            };
            if (EditorGUILayout.DropdownButton(_loadDropdownContent, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = false;
                AddDropdown(dropdownRect, "Load Scene");
            }
            if (EditorGUILayout.DropdownButton(_addDropdownContent, FocusType.Keyboard, ToolbarStyles.commandButtonStyle))
            {
                _additiveLoad = true;
                dropdownRect.x += ToolbarStyles.commandButtonStyle.fixedWidth;
                AddDropdown(dropdownRect, "Add Scene");
            }

            string sceneGuid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
            bool activeSceneIsFavorite = _cachedData.FavoriteScenes.Contains(new SceneData { SceneName = SceneManager.GetActiveScene().name, SceneGuid = sceneGuid });
            if (GUILayout.Button(activeSceneIsFavorite ? _favoriteContent : _notFavoriteContent, ToolbarStyles.iconButtonStyle))
            {
                if (activeSceneIsFavorite)
                {
                    Utility.RemoveFavorite(SceneManager.GetActiveScene().name, sceneGuid);
                }
                else
                {
                    Utility.AddFavorite(SceneManager.GetActiveScene().name, sceneGuid);
                }
                Utility.DirtyFromToolbar = true;
                _cachedData = Utility.GetSavedData();
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