using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDrawerTest : MonoBehaviour
{

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
		this.Amount = (int)amount;
	}

	private List<string> letters = new List<string>()
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

	static Color redColor = Color.red;
	static Color greenColor = Color.green;
	static Color blueColor = Color.blue;
	
	private void Update()
	{
		var currentPos = MinimumPosition;
		for (int i = 0; i < Amount; i++)
		{
			var pos = currentPos;

			pos.y += (Mathf.Sin((Time.time + i) * VetticalPosDIsplacementSpeedFactor) * VerticalPosDisplacementFactor);

			var lerpFactor = Mathf.Sin(Time.time + i);

			var color = Color.Lerp(greenColor, blueColor, lerpFactor);
				
			if (lerpFactor < 0.0f)
			{
				color = Color.Lerp(greenColor, redColor, -lerpFactor);
			}
			TextDrawer.DrawText(letters[i%letters.Count],TextSize,color,Matrix4x4.TRS(pos ,Quaternion.LookRotation(-Vector3.up,Vector3.forward),Vector3.one ));

			currentPos.x += PositionIncrement.x;
			
			if (currentPos.x > MaximumPosition.x)
			{
				currentPos.x = MinimumPosition.x;
				currentPos.z += PositionIncrement.z;
			}
		}
	}
}
