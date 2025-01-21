using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSpline : MonoBehaviour
{
    public enum SplineType
    {
        Uniform,
        Centripetal,
        Chordal
    }

    public SplineType splineType = SplineType.Centripetal;
    public bool doesLoop = false;

    public int PointCount => transform.childCount;


    private void OnDrawGizmos()
    {
        int start = doesLoop == true ? 0 : 1;
        for (int i = start; i < PointCount; i++)
        {
            if (doesLoop == false && i >= PointCount - 2)
                break;

            DrawCurveSegment(GetSegment(i));
        }
    }

    public CatmullRomSegment GetSegment(int i)
    {
        int p0, p1, p2, p3;

        if (doesLoop == true)
        {
            p1 = i;
            p2 = (p1 + 1) % PointCount;
            p3 = (p2 + 1) % PointCount;
            p0 = p1 >= 1 ? p1 - 1 : PointCount - 1;
        }
        else
        {
            p1 = i;
            p2 = p1 + 1;
            p3 = p2 + 1;
            p0 = p1 - 1;
        }

        return new CatmullRomSegment(GetPoint(p0), GetPoint(p1), GetPoint(p2), GetPoint(p3), Alpha);
    }

    private void DrawCurveSegment(CatmullRomSegment curve)
    {
        const int detail = 32;
        Vector3 prev = curve.P1;

        for (int i = 1; i < detail; i++)
        {
            float t = i / (detail - 1f);
            Vector3 pt = curve.GetPoint(t);
            Gizmos.DrawLine(prev, pt);
            prev = pt;
        }
    }

    private Vector3 GetPoint(int i)
    {
        return transform.GetChild(i).position;
    }

    private float Alpha
    {
        get
        {
            if (splineType == SplineType.Uniform)
            {
                return 0.0f;
            }
            else if (splineType == SplineType.Centripetal)
            {
                return 0.5f;
            }

            return 1.0f; // Chordal
        }
    }
}
