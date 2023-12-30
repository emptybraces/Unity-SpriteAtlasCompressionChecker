using UnityEngine;
using UnityEditor;

namespace Emptybraces.Editor.SpriteAtlasCompressionChecker
{
	public static class Constant
	{
		public static GUIStyle GUIStyleLabelOK;
		public static GUIStyle GUIStyleLabelNG;
		const float sat = 0.5f;
		static Constant()
		{
			GUIStyleLabelOK ??= new GUIStyle(EditorStyles.label)
			{
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.HSVToRGB(0.6f, sat, 1f) }
			};
			GUIStyleLabelNG ??= new GUIStyle(EditorStyles.label)
			{
				normal = { textColor = Color.HSVToRGB(0, sat, 0.9f) }
			};
		}
	}
}