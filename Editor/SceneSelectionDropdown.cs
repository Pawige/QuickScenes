using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace QuickScenes
{
	class SceneSelectionDropdown : AdvancedDropdown
	{
		public Action<string> SelectionMade;

		private readonly List<SceneFolder> _folderList;
		private Dictionary<AdvancedDropdownItem, string> _scenes;
		private string DropdownTitle;
		private readonly SavedData _cachedData;

		public SceneSelectionDropdown(AdvancedDropdownState state, string title, List<SceneFolder> folderList) : base(state)
		{
			_folderList = folderList;
			_cachedData = Utility.GetSavedData();
			minimumSize = new Vector2(minimumSize.x, CalculateNeededHeight(folderList));
			DropdownTitle = title;
		}

		private float CalculateNeededHeight(ICollection folderList)
		{
			int favoritesCount = Utility.GetSavedData().FavoriteScenes.Count;
			return EditorGUIUtility.singleLineHeight * (folderList.Count + 3 + favoritesCount);
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			var root = new AdvancedDropdownItem(DropdownTitle);

			_scenes = new Dictionary<AdvancedDropdownItem, string>();
			foreach (SceneData favoriteScene in _cachedData.FavoriteScenes)
			{
				var sceneItem = new AdvancedDropdownItem(favoriteScene.SceneName);
				root.AddChild(sceneItem);	
				_scenes.TryAdd(sceneItem, favoriteScene.SceneGuid);
			}
			root.AddSeparator();
			foreach (SceneFolder sceneFolder in _folderList)
			{
				if (Utility.AreAllScenesInFolderHidden(_cachedData, sceneFolder))
					continue;
				
				var folderItem = new AdvancedDropdownItem(sceneFolder.FolderName);
				root.AddChild(folderItem);
				foreach (string sceneGuid in sceneFolder.SceneGuids)
				{
					if (_cachedData.HiddenScenes.Exists(sceneData => sceneData.SceneGuid == sceneGuid))
						continue;
					
					string path = AssetDatabase.GUIDToAssetPath(sceneGuid);
					string[] scenePathSplit = path.Split('/', '.');
					// Reminder: [^2] is the same as writing [scenePathSplit.Length - 2];
					string sceneName = scenePathSplit[^2];
					var sceneItem = new AdvancedDropdownItem(sceneName);
					folderItem.AddChild(sceneItem);	
					_scenes.TryAdd(sceneItem, sceneGuid);
				}
			}

			return root;
		}

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			SelectionMade.Invoke(_scenes[item]);
		}
	}
}