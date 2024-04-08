using UnityEngine;

public class SlopeCalculator : MonoBehaviour
{
    public static float CalculateSlope(Vector2 pointA, Vector2 pointB)
    {
        if (Mathf.Approximately(pointB.x, pointA.x)) return 0;
        return (pointB.y - pointA.y) / (pointB.x - pointA.x);
    }

    public static Color GetColorFromSlope(float slope)
    {
        float normalizedSlope = Mathf.InverseLerp(-1f, 1f, slope);
        return Color.Lerp(Color.blue, Color.red, normalizedSlope);
    }

    public static float GetPitchFromSlope(float slope)
    {
        return Mathf.Clamp(1.0f + slope * 0.5f, 0.5f, 1.5f);
    }
}
