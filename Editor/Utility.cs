using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuickScenes
{
	public static class Utility
	{
		public static bool DirtyFromWindow;
		public static bool DirtyFromToolbar;
		
		public static List<SceneFolder> GenerateSceneLists()
		{
			string[] sceneList = AssetDatabase.FindAssets("t:scene", new[] { "Assets/" });
			List<SceneFolder> sceneFolders = new List<SceneFolder>();

			foreach (string sceneGuid in sceneList)
			{
				string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
				string[] scenePathSplit = path.Split('/', '.');
				string folderName = scenePathSplit[^3];
				string sceneName = scenePathSplit[^2];

				bool folderFound = false;
				foreach (SceneFolder folder in sceneFolders)
				{
					if (folder.FolderName != folderName)
						continue;

					folder.SceneGuids.Add(sceneGuid);
					folder.SceneNames.Add(sceneName);
					folderFound = true;
					break;
				}

				if (!folderFound)
				{
					SceneFolder newFolder = new SceneFolder
					{
						FolderName = folderName, 
						SceneGuids = new List<string> { sceneGuid }, 
						SceneNames = new List<string> { sceneName }
					};
					sceneFolders.Add(newFolder);
				}
			}

			return sceneFolders;
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
			string fileText = System.IO.File.ReadAllText("QuickScenesData.json");
			// read string array from json file
			SavedData favoritesList = JsonUtility.FromJson<SavedData>(fileText);
			return favoritesList;
		}
		
		// Add favorite to favorites list
		public static void AddFavorite(string sceneName, string sceneGuid)
		{
			SavedData savedData = GetSavedData();
			savedData.FavoriteScenes.Add(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid });
			WriteToJsonFile(savedData);
		}
		
		// Remove favorite from favorites list
		public static void RemoveFavorite(string sceneName, string sceneGuid)
		{
			SavedData savedData = GetSavedData();
			savedData.FavoriteScenes.Remove(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid});
			WriteToJsonFile(savedData);
		}
		
		// Add scene to hidden list
		public static void AddToHiddenList(string sceneName, string sceneGuid)
		{
			SavedData savedData = GetSavedData();
			savedData.HiddenScenes.Add(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid });
			WriteToJsonFile(savedData);
		}
		
		// Remove scene from hidden list
		public static void RemoveFromHiddenList(string sceneName, string sceneGuid)
		{
			SavedData savedData = GetSavedData();
			savedData.HiddenScenes.Remove(new SceneData { SceneName = sceneName, SceneGuid = sceneGuid});
			WriteToJsonFile(savedData);
		}

		private static void WriteToJsonFile(SavedData savedData)
		{
			string updatedData = JsonUtility.ToJson(savedData, true);
			System.IO.File.WriteAllText("QuickScenesData.json", updatedData);
		}

		public static bool AreAllScenesInFolderHidden(SavedData savedData, SceneFolder sceneFolder)
		{
			foreach (string sceneGuid in sceneFolder.SceneGuids)
			{
				// If any scene in the folder is not in the hidden list, return false
				if (savedData.HiddenScenes.FindIndex(sceneData => sceneData.SceneGuid == sceneGuid) == -1)
					return false;
			}
			return true;
		}

		public static void ShowAllScenes()
		{
			SavedData savedData = GetSavedData();
			savedData.HiddenScenes.Clear();
			WriteToJsonFile(savedData);
		}
		
		public static void HideAllScenes()
		{
			SavedData savedData = GetSavedData();
			savedData.HiddenScenes.Clear();
			List<SceneFolder> sceneFolders = GenerateSceneLists();
			foreach (SceneFolder sceneFolder in sceneFolders)
			{
				for (var i = 0; i < sceneFolder.SceneGuids.Count; i++)
				{
					savedData.HiddenScenes.Add(new SceneData { SceneName = sceneFolder.SceneNames[i], SceneGuid = sceneFolder.SceneGuids[i] });
				}
			}
			WriteToJsonFile(savedData);
		}
	}
}