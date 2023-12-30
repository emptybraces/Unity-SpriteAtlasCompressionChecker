using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.U2D;

namespace Emptybraces.Editor.SpriteAtlasCompressionChecker
{
	public class MainEditorWindow : EditorWindow
	{
		SpriteAtlas[] _atlases;
		Vector2 scr_pos;
		List<List<(string, Texture2D)>> _spriteRefs = new();
		bool _isFix, _isDone;

		[MenuItem("Window/SpriteAtlasCompressionCheckerEditor")]
		public static void Open()
		{
			var window = EditorWindow.GetWindow<MainEditorWindow>("SpriteAtlasCompressionCheckerEditor");
			window.ShowPopup();
		}

		void OnEnable()
		{
			_atlases = null;
			_isFix = false;
		}

		void OnGUI()
		{
			_DrawHelpBox();
			if (GUILayout.Button("Find SpriteAtlas"))
			{
				_atlases = AssetDatabase.FindAssets("t:SpriteAtlas")
							.Select(AssetDatabase.GUIDToAssetPath)
							.Where(e => e.EndsWith(".spriteatlasv2"))
							.Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
							.ToArray();
				_spriteRefs.Clear();
				foreach (var atlas in _atlases)
				{
					var list = new List<(string, Texture2D)>();
					var objs = SpriteAtlasExtensions.GetPackables(atlas);
					var tex2ds = _ExtractTextures(objs);
					foreach (var tex in tex2ds)
					{
						var importer = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(tex)) as TextureImporter;
						list.Add((importer.textureCompression == TextureImporterCompression.Uncompressed ? "OK" : "NG", tex));
						_isFix |= importer.textureCompression != TextureImporterCompression.Uncompressed;
					}
					_spriteRefs.Add(list);
				}
			}

			if (_atlases == null)
				return;

			using (var scr = new EditorGUILayout.ScrollViewScope(scr_pos))
			{
				scr_pos = scr.scrollPosition;
				for (int i = 0; i < _atlases.Length; i++)
				{
					var atlas = _atlases[i];
					EditorGUILayout.ObjectField(atlas, typeof(SpriteAtlas), false);

					using (var scope = new EditorGUI.IndentLevelScope())
					{
						foreach (var (label, sprite) in _spriteRefs[i])
						{
							var rect = EditorGUILayout.GetControlRect();
							EditorGUI.LabelField(rect, label, label == "OK" ? Constant.GUIStyleLabelOK : Constant.GUIStyleLabelNG);
							rect.x += 30;
							EditorGUI.ObjectField(rect, sprite, typeof(Sprite), false);
						}
					}
				}
			}
			GUI.enabled = _isFix;
			if (GUILayout.Button("FIX!", GUILayout.MaxHeight(50)))
			{
				for (int i = 0; i < _spriteRefs.Count; ++i)
				{
					for (int j = 0; j < _spriteRefs[i].Count; ++j)
					{
						var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_spriteRefs[i][j].Item2)) as TextureImporter;
						if (importer.textureCompression != TextureImporterCompression.Uncompressed)
						{
							importer.textureCompression = TextureImporterCompression.Uncompressed;
							importer.SaveAndReimport();
							_spriteRefs[i][j] = ("OK", _spriteRefs[i][j].Item2);
						}
					}
				}
				_isFix = false;
				_isDone = true;
			}
		}

		void _DrawHelpBox()
		{
			if (_atlases == null)
				EditorGUILayout.HelpBox("Press <Find SpriteAtlas> button to find SpriteAtlasV2 in whole project.", MessageType.Warning);
			else if (_atlases.Length == 0)
				EditorGUILayout.HelpBox("There is no SpriteAtlasV2 in project!", MessageType.Info);
			else if (_isFix)
				EditorGUILayout.HelpBox("Some sprite that needs to be fixed.", MessageType.Warning);
			else if (_isDone)
				EditorGUILayout.HelpBox("Done!", MessageType.Info);
		}
		Texture2D[] _ExtractTextures(Object[] objs)
		{
			var list = new List<Texture2D>();
			foreach (var o in objs)
			{
				if (o is Texture2D tex2d)
				{
					list.Add(tex2d);
				}
				else if (o is DefaultAsset dir)
				{
					var path = AssetDatabase.GetAssetPath(dir);
					if (AssetDatabase.IsValidFolder(path))
					{
						var tex2ds = AssetDatabase.FindAssets("t:Texture2D", new[] { path })
									.Select(AssetDatabase.GUIDToAssetPath)
									.Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
									.ToArray();
						list.AddRange(tex2ds);
					}
					else
					{
						Debug.LogError("unsupported:" + o);
					}
				}
				else
				{
					Debug.LogError("unsupported:" + o);
				}
			}
			return list.ToArray();
		}
	}
}