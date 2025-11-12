using Godot;

namespace Raele.Supercon2D;

public static class GeneralUtil
{
	public static Vector2 MoveToward(Vector2 from, Vector2 to, Vector2 delta)
	{
		return new Vector2(
			Mathf.MoveToward(from.X, to.X, delta.X),
			Mathf.MoveToward(from.Y, to.Y, delta.Y)
		);
	}

	public static Vector2 MoveToward(float fromX, float fromY, float toX, float toY, float deltaX, float deltaY)
	{
		return new Vector2(
			Mathf.MoveToward(fromX, toX, deltaX),
			Mathf.MoveToward(fromY, toY, deltaY)
		);
	}
}
