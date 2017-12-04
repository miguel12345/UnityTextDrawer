using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDrawerPivotTest : MonoBehaviour {

	public TextDrawer.TextPivot Pivot;
	
	private void LateUpdate()
	{
		TextDrawer.DrawText("Pivot",12f,Color.white, transform.localToWorldMatrix,Pivot);
	}
}
