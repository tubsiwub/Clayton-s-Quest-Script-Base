using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class MathFunctions {

	public static float ConvertNumberRanges(
		float OldValue, float OldMax, float OldMin, float NewMax, float NewMin)
	{
		float OldRange = (OldMax - OldMin);
		float NewRange = (NewMax - NewMin);
		float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
		return NewValue;
	}

	// Uses an object as a seed
	public static float GetRandomNumber(
		GameObject obj, float modulus)
	{
		float seg1 = obj.transform.position.x + obj.transform.position.y + obj.transform.position.z;
		float seg2 = obj.transform.rotation.x + obj.transform.rotation.y + obj.transform.rotation.z;
		float seg3 = obj.transform.localScale.x + obj.transform.localScale.y + obj.transform.localScale.z;
		float final = (((Random.Range(0,500) * (seg1 - seg2)) * (seg3 + seg1)) - seg2/seg1);
		final = Mathf.Abs (final) % modulus;	// Keep the value to the range of [modulus]
		return final;
	}

	// will always return the same number if the object is unchanged
	public static float GetRandomNumberConsistent(GameObject obj, float modulus)
	{
		float seg1 = obj.transform.position.x + obj.transform.position.y + obj.transform.position.z;
		float seg2 = obj.transform.rotation.x + obj.transform.rotation.y + obj.transform.rotation.z;
		float seg3 = obj.transform.localScale.x + obj.transform.localScale.y + obj.transform.localScale.z;
		float final = ((((seg1 - seg2)) * (seg3 + seg1)) - seg2 / seg1);
		final = Mathf.Abs(final) % modulus; // Keep the value to the range of [modulus]
		return final;
	}

	// Gets a point [percent] distance along a line between two Vector3s starting at VectorA
	public static Vector3 PositionBetweenTwoVector3(
		Vector3 vecA, Vector3 vecB, float percent)
	{
		return percent * (vecB - vecA) + vecA;
	}

	public static float RoundToZero(float num)
	{
		if (num < 0.00000001 && num > -0.00000001)
			num = 0;

		return num;
	}

	public static string GetStringFromPosition(Vector3 position)
	{
		string pos = RoundToZero(position.x).ToString() + RoundToZero(position.y).ToString() + RoundToZero(position.z).ToString();

		Regex rgx = new Regex("[^0-9]");    // only allow numbers in this string
		pos = rgx.Replace(pos, "");

		return pos;
	}

	// Returns the parent transform that has the component requested; max 10 checks
	public static Transform FindParentWithComponent(Transform childObj, string component){

		Transform parentObj = childObj.parent;

		int counter = 0;

		while (!parentObj.GetComponent(component) && counter < 10) {
			parentObj = parentObj.parent;
			counter += 1;

			if (!parentObj.parent) {
				counter = 10;
				break;
			}
		}

		if (counter >= 9)
			parentObj = childObj;

		return parentObj;
	}
}
