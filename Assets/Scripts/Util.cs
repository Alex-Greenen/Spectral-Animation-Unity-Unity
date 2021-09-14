using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{

	public static float Sigmoid(float x)
    {
		return 1f / (1f + Mathf.Exp(-x));
    }

	public static float RandomGaussian(float mean = 0.0f, float sigma = 1.0f)
	{
		float u, v, S;

		do
		{
			u = 2.0f * UnityEngine.Random.value - 1.0f;
			v = 2.0f * UnityEngine.Random.value - 1.0f;
			S = u * u + v * v;
		}
		while (S >= 1.0f);

		// Standard Normal Distribution
		float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

		// Normal Distribution centered between the min and max value
		// and clamped following the "three-sigma rule"
		return std * sigma + mean;
	}

	public static (Vector3, Vector3) CorrectSixDimRot(Vector3 first, Vector3 second)
	{
		first.Normalize();
		second.Normalize();
		float c = Vector3.Dot(first, second);
		c = (1 - Mathf.Sqrt(1 - Mathf.Pow(c, 2.0f))) / (c + Mathf.Pow(10.0f, -8.0f));
		Vector3 temp = first;
		first -= c * second;
		second -= c * temp;
		first.Normalize();
		second.Normalize();
		return (first, second);
	}

	public static Quaternion SixDim_to_quat(Vector3 v0, Vector3 v1)
	{
		Vector3 v2 = Vector3.Cross(v0, v1).normalized;

		float tr = v0.x + v1.y + v2.z;
		float qw, qx, qy, qz;

		if (tr > 0)
		{
			float S = Mathf.Sqrt(tr + 1f) * 2;
			qw = 0.25f * S;
			qx = (v2.y - v1.z) / S;
			qy = (v0.z - v2.x) / S;
			qz = (v1.x - v0.y) / S;
		}
		else if ((v0.x > v1.y) & (v0.x > v2.z))
		{
			float S = Mathf.Sqrt(1f + v0.x - v1.y - v2.z) * 2;
			qw = (v2.y - v1.z) / S;
			qx = 0.25f * S;
			qy = (v0.y + v1.x) / S;
			qz = (v0.z + v2.x) / S;
		}
		else if (v1.y > v2.z)
		{
			float S = Mathf.Sqrt(1f + v1.y - v0.x - v2.z) * 2;
			qw = (v0.z - v2.x) / S;
			qx = (v0.y + v1.x) / S;
			qy = 0.25f * S;
			qz = (v1.z + v2.y) / S;
		}
		else
		{
			float S = Mathf.Sqrt(1f + v2.z - v0.x - v1.y) * 2;
			qw = (v1.x - v0.y) / S;
			qx = (v0.z + v2.x) / S;
			qy = (v1.z + v2.y) / S;
			qz = 0.25f * S;
		}
		return Quaternion.Normalize(new Quaternion(-qy, qx, qz, qw));
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
		q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
		q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
		q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
		q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
		q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
		q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
		return q;
	}

	public static Matrix4x4 RotationFromSixDim(Vector3 first, Vector3 second)
	{
		return Matrix4x4.LookAt(Vector3.zero, first, second);
	}

	public static float deg_to_rad(float deg)
	{
		deg = Mathf.Repeat(deg + 540f, 360f) - 180f;
		return deg * Mathf.Deg2Rad;
	}
	public static float rad_to_deg(float rad)
	{
		return Mathf.Rad2Deg * rad;
	}
	public static float wrap_degrees(float deg)
	{
		return (deg + 540f) % 360f - 180f;
	}
	public static float wrap_radians(float deg)
	{
		return (deg + 3 * Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
	}
	public static void print(float[] inp)
	{
		string result = "";
		foreach (var item in inp)
		{
			result += item.ToString() + ",";
		}
		Debug.Log(result);
	}
}
