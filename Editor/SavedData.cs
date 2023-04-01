using System;
using System.Collections.Generic;

namespace QuickScenes
{
	[Serializable]
	public class SavedData
	{
		public List<SceneData> FavoriteScenes = new List<SceneData>();
		public List<SceneData> HiddenScenes = new List<SceneData>();
	}

	[Serializable]
	public struct SceneData
	{
		public string SceneName;
		public string SceneGuid;
	}
}