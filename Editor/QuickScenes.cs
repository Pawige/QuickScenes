using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickScenes
{
	public class QuickScenes : EditorWindow
	{
		private List<SceneFolder> _sceneFolders;

		private Texture2D _buttonBg;
		private Vector2 _scrollPosition;
		private List<SceneAsset> _quickSceneAccessList = new List<SceneAsset>(1);

		private GUIContent _favorite;
		private GUIContent _notFavorite;
		private GUIContent _shown;
		private GUIContent _hidden;
		private SavedData _cachedData;
		private GUILayoutOption[] _buttonLayoutOptions;
		
		[MenuItem("Window/Quick Scenes")]
		public static void ShowWindow()
		{
			GetWindow(typeof(QuickScenes));
		}

		private void OnEnable()
		{
			// try to load favorites list and create one if it doesn't exist
			if (!System.IO.File.Exists("QuickScenesData.json"))
			{
				Utility.CreateSavedDataFile();
			}
			_cachedData = Utility.GetSavedData();

			Texture favoriteIcon = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star.tga");
			Texture notFavoriteIcon = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Star_Outline.tga");
			Texture shownIcon = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/Eye.tga");
			Texture hiddenIcon = (Texture)EditorGUIUtility.Load("Packages/com.paulgerla.quickscenes/Editor/Images/NoEye.tga");
			_favorite = new GUIContent(favoriteIcon);
			_notFavorite = new GUIContent(notFavoriteIcon);
			_shown = new GUIContent(shownIcon);
			_hidden = new GUIContent(hiddenIcon);

			_buttonLayoutOptions = new[] { GUILayout.Height(EditorGUIUtility.singleLineHeight - 2), GUILayout.Width(20) };
			
			_sceneFolders = Utility.GenerateSceneLists();
			AssetDatabase.FindAssets("t:scene", new[] { "Assets/Scenes" });
			titleContent = new GUIContent("Quick Scenes");
		}

		private void OnGUI()
		{
			if (EditorApplication.isPlaying)
				return;

			if (Utility.DirtyFromToolbar)
			{
				Utility.DirtyFromToolbar = false;
				_cachedData = Utility.GetSavedData();
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Quick Scene Access", EditorStyles.boldLabel);
			GUI.skin.button.fontStyle = FontStyle.Bold;
			Color guiColor = GUI.color;
			for (var i = 0; i <= _quickSceneAccessList.Count; i++)
			{
				if (i < _quickSceneAccessList.Count && _quickSceneAccessList[i] == null)
				{
					_quickSceneAccessList.RemoveAt(i);
					Repaint();
					continue;
				}
				
				GUILayout.BeginHorizontal();
				// Disable button if no scene is selected. This is cursed but it works...
				bool enableButton = i < _quickSceneAccessList.Count && _quickSceneAccessList[i] != null;
				EditorGUI.BeginDisabledGroup(!enableButton);
				
				if (GUILayout.Button("Load Scene", GUILayout.Width(150)))
				{
					if (_quickSceneAccessList[i] == null)
						ShowNotification(new GUIContent("No scene asset selected. Please select a scene to load."));
					else
					{
						string assetPath = AssetDatabase.GetAssetPath(_quickSceneAccessList[i]);
						Utility.LoadScene(assetPath);
					}
				}
				EditorGUI.EndDisabledGroup();
				if (i >= _quickSceneAccessList.Count)
				{
					EditorGUI.BeginChangeCheck();
					SceneAsset newSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(null, typeof(SceneAsset), false);
					if (EditorGUI.EndChangeCheck() && newSceneAsset != null && newSceneAsset is SceneAsset)
					{
						_quickSceneAccessList.Add(newSceneAsset);
					}
				}
				else
				{
					_quickSceneAccessList[i] = (SceneAsset)EditorGUILayout.ObjectField(_quickSceneAccessList[i], typeof(SceneAsset), false);
				}

				GUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Full Scene List", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Refresh", GUILayout.Width(100)))
			{
				_sceneFolders = Utility.GenerateSceneLists();
			}
			GUILayout.EndHorizontal();
			
			if (_sceneFolders == null)
				return;

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			foreach (SceneFolder sceneFolder in _sceneFolders)
			{
				EditorGUILayout.BeginHorizontal();
				sceneFolder.FolderOpen = EditorGUILayout.Foldout(sceneFolder.FolderOpen, sceneFolder.FolderName);
				EditorGUILayout.LabelField(sceneFolder.SceneGuids.Count.ToString(), GUILayout.Width(50));
				EditorGUILayout.EndHorizontal();
				if (!sceneFolder.FolderOpen)
					continue;

				foreach (string guid in sceneFolder.SceneGuids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					string[] scenePathSplit = path.Split('/', '.');
					string sceneName = scenePathSplit[scenePathSplit.Length - 2];

					EditorGUILayout.BeginHorizontal();
					if (SceneManager.GetActiveScene().name != sceneName)
					{
						if (GUILayout.Button(sceneName))
						{
							Utility.LoadScene(path);
						}

						if (GUILayout.Button("+", GUILayout.Width(22)))
						{
							Utility.AddScene(path);
						}
					}
					else
					{
						GUI.skin.button.fontStyle = FontStyle.Bold;
						GUILayout.Button($"Active: {sceneName}");
						GUI.skin.button.fontStyle = FontStyle.Normal;
					}

					GUI.color = guiColor;
					if (GUILayout.Button("Find", GUILayout.Width(38)))
					{
						Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
					}

					bool isHidden = _cachedData.HiddenScenes.Contains(new SceneData { SceneName = sceneName, SceneGuid = guid });
					if (GUILayout.Button(isHidden ? _hidden : _shown, ToolbarStyles.iconButtonStyle, _buttonLayoutOptions))
					{
						if (isHidden)
							Utility.RemoveFromHiddenList(sceneName, guid);
						else
							Utility.AddToHiddenList(sceneName, guid);
						_cachedData = Utility.GetSavedData();
						Utility.DirtyFromWindow = true;
					}
					
					bool isFavorite = _cachedData.FavoriteScenes.Contains(new SceneData { SceneName = sceneName, SceneGuid = guid });
					if (GUILayout.Button(isFavorite ? _favorite : _notFavorite, ToolbarStyles.iconButtonStyle, _buttonLayoutOptions))
					{
						if (isFavorite)
							Utility.RemoveFavorite(sceneName, guid);
						else
							Utility.AddFavorite(sceneName, guid);
						_cachedData = Utility.GetSavedData();
						Utility.DirtyFromWindow = true;
					}
					EditorGUILayout.EndHorizontal();
					// Draw buttons to hide all and show all scenes
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Set All Scenes Hidden"))
			{
				Utility.HideAllScenes();
				_cachedData = Utility.GetSavedData();
				Utility.DirtyFromWindow = true;
			}
			if (GUILayout.Button("Set All Scenes Shown"))
			{
				Utility.ShowAllScenes();
				_cachedData = Utility.GetSavedData();
				Utility.DirtyFromWindow = true;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
		}
	}
}