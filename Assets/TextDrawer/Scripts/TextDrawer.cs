using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;


public class TextDrawer : MonoBehaviour
{

	struct TextCacheKey : IEquatable<TextCacheKey>
	{
		public TextCacheKey(string text, float fontSize)
		{
			_text = text;
			_fontSize = fontSize;
		}

		public bool Equals(TextCacheKey other)
		{
			return string.Equals(_text, other._text) && _fontSize.Equals(other._fontSize);
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
				return ((_text != null ? _text.GetHashCode() : 0) * 397) ^ _fontSize.GetHashCode();
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

		private readonly string _text;
		private readonly float _fontSize;
	}

	private readonly LRUDictionary<TextCacheKey, Mesh> _textMeshCache = new LRUDictionary<TextCacheKey, Mesh>(400);
	private TextMeshPro _textMeshPro;
	private MaterialPropertyBlock _materialPropertyBlock;
	private TextDrawer _textDrawer;
	private static TextDrawer _instance;
	private static bool _instanced;
	private int _materialTextColorPropertyId;
	private Color _materialPropertyBlockLastColorSet = Color.white;
	private Material _fontMaterial;

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
		_fontMaterial = _textMeshPro.fontMaterial;
	}

	Mesh GenerateMeshForText(string text, float fontSize)
	{
		var textCacheKey = new TextCacheKey(text,fontSize);

		Mesh cachedMesh;
		if (_textMeshCache.TryGetValue(textCacheKey, out cachedMesh))
		{
			return cachedMesh;
		}
		
		_textMeshPro.text = text;
		_textMeshPro.fontSize = fontSize;
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

	void DrawTextInternal(string text, float fontSize, Color color, Matrix4x4 mat)
	{
		if (_materialPropertyBlockLastColorSet != color)
		{
			_materialPropertyBlockLastColorSet = color;
			_materialPropertyBlock.SetColor(_materialTextColorPropertyId,color);
		}
		
		Graphics.DrawMesh(GenerateMeshForText(text,fontSize),mat,_fontMaterial,0,null,0,_materialPropertyBlock);
	}
}
