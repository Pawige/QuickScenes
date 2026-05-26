using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuickScenes
{
	public static class Utility
	{
		private static List<SceneFolder> _cachedSceneFolders;
		private static bool _sceneFoldersDirty = true;
		private static SavedData _cachedSavedData;
		private static HashSet<string> _favoriteSceneGuids;
		private static HashSet<string> _hiddenSceneGuids;

		static Utility()
		{
			EditorApplication.projectChanged += MarkSceneListsDirty;
		}
		
		public static List<SceneFolder> GetSceneFolders()
		{
			if (_cachedSceneFolders == null || _sceneFoldersDirty)
			{
				RebuildSceneFolders();
			}

			return _cachedSceneFolders;
		}

		public static List<SceneFolder> RefreshSceneFolders()
		{
			RebuildSceneFolders();
			return _cachedSceneFolders;
		}

		private static void MarkSceneListsDirty()
		{
			_sceneFoldersDirty = true;
		}

		private static void RebuildSceneFolders()
		{
			string[] sceneList = AssetDatabase.FindAssets("t:scene", new[] { "Assets/" });
			var sceneFolders = new List<SceneFolder>();
			var sceneFoldersByName = new Dictionary<string, SceneFolder>();

			foreach (string sceneGuid in sceneList)
			{
				string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
				string[] scenePathSplit = path.Split('/', '.');
				string folderName = scenePathSplit[^3];
				string sceneName = scenePathSplit[^2];

				if (!sceneFoldersByName.TryGetValue(folderName, out SceneFolder folder))
				{
					folder = new SceneFolder
					{
						FolderName = folderName,
						SceneGuids = new List<string>(),
						SceneNames = new List<string>(),
						ScenePaths = new List<string>()
					};
					sceneFoldersByName.Add(folderName, folder);
					sceneFolders.Add(folder);
				}

				folder.SceneGuids.Add(sceneGuid);
				folder.SceneNames.Add(sceneName);
				folder.ScenePaths.Add(path);
			}

			_cachedSceneFolders = sceneFolders;
			_sceneFoldersDirty = false;
		}

		public static void LoadScene(string scenePath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			}
		}

		public static void AddScene(string scenePath)
		{
			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
		}
		
		// Create favorites list text file
		public static void CreateSavedDataFile()
		{
			string jsonData = JsonUtility.ToJson(new SavedData());
			System.IO.File.WriteAllText("QuickScenesData.json", jsonData);
		}
		
		// Read favorites list text file
		public static SavedData GetSavedData()
		{
			EnsureSavedDataLoaded();
			return _cachedSavedData;
		}

		public static HashSet<string> GetFavoriteSceneGuids()
		{
			EnsureSavedDataLoaded();
			return _favoriteSceneGuids;
		}

		public static HashSet<string> GetHiddenSceneGuids()
		{
			EnsureSavedDataLoaded();
			return _hiddenSceneGuids;
		}
		
		// Add favorite to favorites list
		public static bool AddFavorite(string sceneName, string sceneGuid)
		{
			EnsureSavedDataLoaded();
			if (!_favoriteSceneGuids.Add(sceneGuid))
				return false;

			_cachedSavedData.FavoriteScenes.Add(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid });
			SaveSavedData();
			return true;
		}
		
		// Remove favorite from favorites list
		public static bool RemoveFavorite(string sceneName, string sceneGuid)
		{
			EnsureSavedDataLoaded();
			if (!_favoriteSceneGuids.Remove(sceneGuid))
				return false;

			_cachedSavedData.FavoriteScenes.RemoveAll(sceneData => sceneData.SceneGuid == sceneGuid);
			SaveSavedData();
			return true;
		}
		
		// Add scene to hidden list
		public static bool AddToHiddenList(string sceneName, string sceneGuid)
		{
			EnsureSavedDataLoaded();
			if (!_hiddenSceneGuids.Add(sceneGuid))
				return false;

			_cachedSavedData.HiddenScenes.Add(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid });
			SaveSavedData();
			return true;
		}
		
		// Remove scene from hidden list
		public static bool RemoveFromHiddenList(string sceneName, string sceneGuid)
		{
			EnsureSavedDataLoaded();
			if (!_hiddenSceneGuids.Remove(sceneGuid))
				return false;

			_cachedSavedData.HiddenScenes.RemoveAll(sceneData => sceneData.SceneGuid == sceneGuid);
			SaveSavedData();
			return true;
		}

		private static void EnsureSavedDataLoaded()
		{
			if (_cachedSavedData != null)
				return;

			if (!System.IO.File.Exists("QuickScenesData.json"))
			{
				CreateSavedDataFile();
			}

			string fileText = System.IO.File.ReadAllText("QuickScenesData.json");
			_cachedSavedData = JsonUtility.FromJson<SavedData>(fileText) ?? new SavedData();
			_cachedSavedData.FavoriteScenes ??= new List<SceneData>();
			_cachedSavedData.HiddenScenes ??= new List<SceneData>();
			RebuildSavedDataLookups();
		}

		private static void SaveSavedData()
		{
			string updatedData = JsonUtility.ToJson(_cachedSavedData, true);
			System.IO.File.WriteAllText("QuickScenesData.json", updatedData);
		}

		private static void RebuildSavedDataLookups()
		{
			_favoriteSceneGuids = BuildSceneGuidLookup(_cachedSavedData.FavoriteScenes);
			_hiddenSceneGuids = BuildSceneGuidLookup(_cachedSavedData.HiddenScenes);
		}

		public static bool AreAllScenesInFolderHidden(HashSet<string> hiddenSceneGuids, SceneFolder sceneFolder)
		{
			foreach (string sceneGuid in sceneFolder.SceneGuids)
			{
				// If any scene in the folder is not in the hidden list, return false
				if (!hiddenSceneGuids.Contains(sceneGuid))
					return false;
			}
			return true;
		}

		private static HashSet<string> BuildSceneGuidLookup(List<SceneData> scenes)
		{
			var sceneGuids = new HashSet<string>();
			foreach (SceneData scene in scenes)
			{
				if (!string.IsNullOrEmpty(scene.SceneGuid))
				{
					sceneGuids.Add(scene.SceneGuid);
				}
			}

			return sceneGuids;
		}

		public static void ShowAllScenes()
		{
			EnsureSavedDataLoaded();
			if (_cachedSavedData.HiddenScenes.Count == 0)
				return;

			_cachedSavedData.HiddenScenes.Clear();
			_hiddenSceneGuids.Clear();
			SaveSavedData();
		}
		
		public static void HideAllScenes()
		{
			EnsureSavedDataLoaded();
			_cachedSavedData.HiddenScenes.Clear();
			List<SceneFolder> sceneFolders = GetSceneFolders();
			foreach (SceneFolder sceneFolder in sceneFolders)
			{
				for (var i = 0; i < sceneFolder.SceneGuids.Count; i++)
				{
					_cachedSavedData.HiddenScenes.Add(new SceneData { SceneName = sceneFolder.SceneNames[i], SceneGuid = sceneFolder.SceneGuids[i] });
				}
			}
			_hiddenSceneGuids = BuildSceneGuidLookup(_cachedSavedData.HiddenScenes);
			SaveSavedData();
		}
	}
}
