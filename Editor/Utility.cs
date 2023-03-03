using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace QuickScenes
{
	public static class Utility
	{
		public static List<SceneFolder> GenerateSceneLists()
		{
			string[] sceneList = AssetDatabase.FindAssets("t:scene", new[] { "Assets/" });
			List<SceneFolder> sceneFolders = new List<SceneFolder>();

			foreach (string sceneGuid in sceneList)
			{
				string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
				string[] scenePathSplit = path.Split('/', '.');
				string folderName = scenePathSplit[scenePathSplit.Length - 3];
				string sceneName = scenePathSplit[scenePathSplit.Length - 2];

				bool folderFound = false;
				foreach (SceneFolder folder in sceneFolders)
				{
					if (folder.FolderName != folderName)
						continue;

					folder.SceneGuids.Add(sceneGuid);
					folderFound = true;
					break;
				}

				if (!folderFound)
				{
					SceneFolder newFolder = new SceneFolder { FolderName = folderName, SceneGuids = new List<string> { sceneGuid } };
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
	}
}