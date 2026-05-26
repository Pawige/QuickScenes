using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickScenes
{
    [InitializeOnLoad]
    public class ToolbarScenePicker
    {
        private static MainToolbarContent _favoriteContent;
        const string k_ToolbarElementName = "Quick Scenes/Scene Loader";
        
        private static readonly GUIContent _loadDropdownContent = new GUIContent("Load Scene", "Pick a scene to replace whatever is loaded.");
        private static readonly GUIContent _addDropdownContent = new GUIContent("Add Scene", "Pick a scene to load additively.");
        private static bool _additiveLoad;
        private static List<SceneFolder> _sceneList;
        private static SceneSelectionDropdown _dropdownSelectionMenu;
        private static SavedData _cachedData;
        private static HashSet<string> _favoriteSceneGuids;
        private static Texture2D _star;
        private static Texture2D _outlinedStar;
        private const string _addFavoriteTooltip = "Remove active scene from favorites list.";
        
        static ToolbarScenePicker()
        {
            _star = (Texture2D)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star.tga");
            _outlinedStar = (Texture2D)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star_Outline.tga");
            InitSceneMenu();
            
            RefreshSavedDataCache();
            
            SceneManager.activeSceneChanged += SceneSwitched;
            EditorSceneManager.activeSceneChangedInEditMode += SceneSwitched;
        }
        
        private static void SceneSwitched(Scene arg0, Scene arg1)
        {
            MainToolbar.Refresh(k_ToolbarElementName);
        }

        [MainToolbarElement(k_ToolbarElementName, defaultDockPosition = MainToolbarDockPosition.Left)]
        private static IEnumerable<MainToolbarElement> CreateSceneLoadingBar()
        {
            var loadSceneContent = new MainToolbarContent("Load Scene", "Loads scene non-additively.");
            var addSceneContent = new MainToolbarContent("Add Scene", "Loads scene additively.");

            string sceneGuid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
            bool activeSceneIsFavorite = _favoriteSceneGuids != null && _favoriteSceneGuids.Contains(sceneGuid);
            _favoriteContent = new MainToolbarContent(activeSceneIsFavorite ? _star : _outlinedStar, _addFavoriteTooltip);

            yield return new MainToolbarDropdown(loadSceneContent, ShowLoadDropdownMenu);
            yield return new MainToolbarDropdown(addSceneContent, ShowAddDropdownMenu);
            yield return new MainToolbarButton(_favoriteContent, FavoriteButtonPressed);
        }
        
        private static void FavoriteButtonPressed()
        {
            string sceneGuid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
            bool activeSceneIsFavorite = _favoriteSceneGuids.Contains(sceneGuid);
            if (activeSceneIsFavorite)
            {
                Utility.RemoveFavorite(SceneManager.GetActiveScene().name, sceneGuid);
            }
            else
            {
                Utility.AddFavorite(SceneManager.GetActiveScene().name, sceneGuid);
            }
            RefreshSavedDataCache();
            MainToolbar.Refresh(k_ToolbarElementName);
        }

        private static void ShowLoadDropdownMenu(Rect dropDownRect)
        {
            _additiveLoad = false;
            AddDropdown(dropDownRect, "Load Scene");
        }

        private static void ShowAddDropdownMenu(Rect dropDownRect)
        {
            _additiveLoad = true;
            AddDropdown(dropDownRect, "Add Scene");
        }
        
        /*static void OnToolbarGUI()
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
            if (GUILayout.Button(activeSceneIsFavorite ? _favoriteContent : _notFavoriteContent, ToolbarStyles.toolbarButtonStyle))
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
        }*/
        
        private static void AddDropdown(Rect dropdownRect, string title)
        {
            RefreshSavedDataCache();
            _sceneList = Utility.GetSceneFolders();

            if (_dropdownSelectionMenu != null)
            {
                _dropdownSelectionMenu.SelectionMade -= SelectionMade;
            }

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
            _sceneList = Utility.GetSceneFolders();
        }

        private static void RefreshSavedDataCache()
        {
            _cachedData = Utility.GetSavedData();
            _favoriteSceneGuids = Utility.GetFavoriteSceneGuids();
        }

        private static void LoadSceneWithGuid(object sceneGuid)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath((string)sceneGuid);

            // Check if the asset exists
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                // Check if this scene is in the favorites list
                string guid = (string)sceneGuid;
                SceneData sceneToRemove = default;
                foreach (var favoriteScene in _cachedData.FavoriteScenes)
                {
                    if (favoriteScene.SceneGuid == guid)
                    {
                        sceneToRemove = favoriteScene;
                        break;
                    }
                }

                if (sceneToRemove.SceneGuid == guid)
                {
                    bool shouldRemove = EditorUtility.DisplayDialog(
                        "Scene Not Found",
                        $"The scene '{sceneToRemove.SceneName}' no longer exists. Would you like to remove it from your favorites list?",
                        "Yes",
                        "No"
                    );

                    if (shouldRemove)
                    {
                        Utility.RemoveFavorite(sceneToRemove.SceneName, sceneToRemove.SceneGuid);
                        RefreshSavedDataCache();
                    }
                }
                else
                {
                    Debug.LogError($"Scene with GUID '{guid}' does not exist.");
                }

                return;
            }

            if (_additiveLoad)
                Utility.AddScene(scenePath);
            else
                Utility.LoadScene(scenePath);
        }
    }
}
