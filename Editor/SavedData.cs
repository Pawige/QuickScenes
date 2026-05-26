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
	public struct SceneData : IEquatable<SceneData>
	{
		public string SceneName;
		public string SceneGuid;
		public bool Equals(SceneData other)
		{
			return SceneGuid == other.SceneGuid;
		}
		public override bool Equals(object obj)
		{
			return obj is SceneData other && Equals(other);
		}
		public override int GetHashCode()
		{
			return (SceneGuid != null ? SceneGuid.GetHashCode() : 0);
		}
	}
}