using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;


public class TextDrawer : MonoBehaviour
{

	struct TextCacheKey : IEquatable<TextCacheKey>
	{
		public bool Equals(TextCacheKey other)
		{
			return string.Equals(_text, other._text) && _fontSize.Equals(other._fontSize) && _fontAsset.Equals(other._fontAsset);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TextCacheKey && Equals((TextCacheKey) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _text.GetHashCode();
				hashCode = (hashCode * 397) ^ _fontSize.GetHashCode();
				hashCode = (hashCode * 397) ^ _fontAsset.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(TextCacheKey left, TextCacheKey right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TextCacheKey left, TextCacheKey right)
		{
			return !left.Equals(right);
		}

		public TextCacheKey(string text, float fontSize, TMP_FontAsset font)
		{
			_text = text;
			_fontSize = fontSize;
			_fontAsset = font;
		}

		private readonly string _text;
		private readonly float _fontSize;
		private readonly TMP_FontAsset _fontAsset;
	}

	private readonly LRUDictionary<TextCacheKey, Mesh> _textMeshCache = new LRUDictionary<TextCacheKey, Mesh>(400);
	private TextMeshPro _textMeshPro;
	private MaterialPropertyBlock _materialPropertyBlock;
	private TextDrawer _textDrawer;
	private static TextDrawer _instance;
	private static bool _instanced;
	private int _materialTextColorPropertyId;
	private Color _materialPropertyBlockLastColorSet = Color.white;
	private TMP_FontAsset _defaultFontAsset;

	/// <summary>
	///  Draws a 3D text mesh. This works in immediate mode, so you need to call this every frame you want to draw it.
	/// </summary>
	/// <param name="text">Text to be displayed</param>
	/// <param name="fontSize">Font size</param>
	/// <param name="color">Text color</param>
	/// <param name="mat">TRS matrix in world space</param>
	public static void DrawText(string text, float fontSize, Color color, Matrix4x4 mat)
	{
		Instance.DrawTextInternal(text,fontSize,color,mat);
	}
	
	public static void DrawText(string text, float fontSize, Color color, Matrix4x4 mat, TMP_FontAsset font)
	{
		Instance.DrawTextInternal(text,fontSize,color,mat,font);
	}
	
	private static TextDrawer Instance
	{
		get
		{
			if (_instanced)
			{
				return _instance;
			}

			_instance = new GameObject("TextDrawer").AddComponent<TextDrawer>();
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
		_instanced = true;
		DontDestroyOnLoad(gameObject);
		_textMeshPro = gameObject.AddComponent<TextMeshPro>();
		_textMeshPro.alignment = TextAlignmentOptions.Center;
		_textMeshPro.GetComponent<MeshRenderer>().enabled = false;
		_materialPropertyBlock = new MaterialPropertyBlock();
		_materialTextColorPropertyId = Shader.PropertyToID("_FaceColor");
		_defaultFontAsset = _textMeshPro.font;
	}

	Mesh GenerateMeshForText(string text, float fontSize, TMP_FontAsset font)
	{
		var textCacheKey = new TextCacheKey(text,fontSize,font);

		Mesh cachedMesh;
		if (_textMeshCache.TryGetValue(textCacheKey, out cachedMesh))
		{
			return cachedMesh;
		}
		
		_textMeshPro.text = text;
		_textMeshPro.fontSize = fontSize;
		
		if (font != _textMeshPro.font)
		{
			_textMeshPro.font = font;
			_textMeshPro.UpdateFontAsset();
		}
		
		_textMeshPro.ClearMesh();
		_textMeshPro.ForceMeshUpdate();
		var textMesh = Instantiate(_textMeshPro.mesh);
		
		_textMeshCache.Add(textCacheKey,textMesh);
		
		return textMesh;
	}

	/// <summary>
	/// Size of the underlying text mesh cache (in number of entries). If a given text mesh is not in cache
	/// a new one must be created by the text mesh generator backend. This is an expensive operation 
	/// and should be avoided. Every time you ask TextDrawer to draw a new text/font-size combination,
	/// it needs to be generated and put back in the cache. Once the cache capacity overflows, the oldest item
	/// is discarded to make room for the new one.
	/// </summary>
	public static int CacheCapacity
	{
		get { return Instance.CacheCapacityInternal; }
		set { Instance.CacheCapacityInternal = value; }
	}
	

	private int CacheCapacityInternal
	{
		set { _textMeshCache.Capacity = value; }
		get { return _textMeshCache.Capacity; }
	}

	private void DrawTextInternal(string text, float fontSize, Color color, Matrix4x4 mat, TMP_FontAsset font = null)
	{
		if (_materialPropertyBlockLastColorSet != color)
		{
			_materialPropertyBlockLastColorSet = color;
			_materialPropertyBlock.SetColor(_materialTextColorPropertyId,color);
		}
		if (font == null) font = _defaultFontAsset;
		
		//Since TMP generates meshes that, by default, face the -z direction, we need to rotate it by 180 degrees in the y axis
		//Rotating by 180 degrees in the y axis is equivalent to negate the scale in the x and z axis
		mat.m00 = -mat.m00;
		mat.m22 = -mat.m22;
		
		Graphics.DrawMesh(GenerateMeshForText(text,fontSize,font),mat,font.material,0,null,0,_materialPropertyBlock);
	}
}
