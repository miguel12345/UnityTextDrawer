using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDrawerTest : MonoBehaviour
{

	public TMP_FontAsset[] Fonts;
	public int Amount = 5;
	public float SphereRadius = 2f;
	public float TextSize = 10f;
	public Vector3 MinimumPosition = new Vector3(-5f,0.5f,-5f);
	public Vector3 MaximumPosition = new Vector3(5f,0.5f,-5f);
	public Vector3 PositionIncrement = new Vector3(0.3f,0f,0.3f);
	public float VerticalPosDisplacementFactor = 0.08f;
	public float VetticalPosDIsplacementSpeedFactor = 2f;

	public void SetAmount(float amount)
	{
		Amount = (int)amount;
	}

	private readonly List<string> _letters = new List<string>()
	{
		"1",
		"2",
		"3",
		"4",
		"5",
		"6",
		"7",
		"8",
		"9",
		"A",
		"B",
		"C",
		"D",
		"E",
		"F",
		"G",
		"H",
	};

	private static readonly Color RedColor = Color.red;
	private static readonly Color GreenColor = Color.green;
	private static readonly Color BlueColor = Color.blue;
	
	private void Update()
	{
		var currentPos = MinimumPosition;
		for (var i = 0; i < Amount; i++)
		{
			var pos = currentPos;

			pos.y += (Mathf.Sin((Time.time + i) * VetticalPosDIsplacementSpeedFactor) * VerticalPosDisplacementFactor);

			var lerpFactor = Mathf.Sin(Time.time + i);

			var color = Color.Lerp(GreenColor, BlueColor, lerpFactor);
				
			if (lerpFactor < 0.0f)
			{
				color = Color.Lerp(GreenColor, RedColor, -lerpFactor);
			}
			TextDrawer.DrawText(_letters[i%_letters.Count],TextSize,color,Matrix4x4.TRS(pos ,Quaternion.LookRotation(Vector3.up,Vector3.forward),Vector3.one ),Fonts[i%Fonts.Length]);

			currentPos.x += PositionIncrement.x;
			
			if (currentPos.x > MaximumPosition.x)
			{
				currentPos.x = MinimumPosition.x;
				currentPos.z += PositionIncrement.z;
			}
		}
	}
}
