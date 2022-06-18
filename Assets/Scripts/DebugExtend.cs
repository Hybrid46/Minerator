using UnityEngine;

public static class ExtendDebug
{
	public struct ColorScope : System.IDisposable
	{
		Color oldColor;
		public ColorScope(Color color)
		{
			oldColor = Gizmos.color;
			Gizmos.color = color == default(Color) ? oldColor : color;
		}

		public void Dispose()
		{
			Gizmos.color = oldColor;
		}
	}

	public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color = default(Color))
	{
		DrawBox(new Box(origin, halfExtents, orientation), color);
	}

	public static void DrawBox(Box box, Color color = default(Color))
	{
		using (new ColorScope(color))
		{
			Debug.DrawLine(box.frontTopLeft, box.frontTopRight);
			Debug.DrawLine(box.frontTopRight, box.frontBottomRight);
			Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft);
			Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft);

			Debug.DrawLine(box.backTopLeft, box.backTopRight);
			Debug.DrawLine(box.backTopRight, box.backBottomRight);
			Debug.DrawLine(box.backBottomRight, box.backBottomLeft);
			Debug.DrawLine(box.backBottomLeft, box.backTopLeft);

			Debug.DrawLine(box.frontTopLeft, box.backTopLeft);
			Debug.DrawLine(box.frontTopRight, box.backTopRight);
			Debug.DrawLine(box.frontBottomRight, box.backBottomRight);
			Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft);
		}
	}

	public struct Box
	{
		public Vector3 localFrontTopLeft { get; private set; }
		public Vector3 localFrontTopRight { get; private set; }
		public Vector3 localFrontBottomLeft { get; private set; }
		public Vector3 localFrontBottomRight { get; private set; }
		public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
		public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
		public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
		public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

		public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
		public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
		public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
		public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
		public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
		public Vector3 backTopRight { get { return localBackTopRight + origin; } }
		public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
		public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

		public Vector3 origin { get; private set; }

		public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
		{
			Rotate(orientation);
		}

		public Box(Vector3 origin, Vector3 halfExtents) : this()
		{
			this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
			this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
			this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
			this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

			this.origin = origin;
		}

		public void Rotate(Quaternion orientation)
		{
			localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
			localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
			localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
			localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
		}
	}

	private static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		Vector3 direction = point - pivot;
		return pivot + rotation * direction;
	}
}
