using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSegment
{
    private Vector3 p0, p1, p2, p3;
    private const float t0 = 0;
    private float t1, t2, t3;
    private float alpha;

    public Vector3 P0 { get { return p0; } }
    public Vector3 P1 { get { return p1; } }
    public Vector3 P2 { get { return p2; } }
    public Vector3 P3 { get { return p3; } }

    public float T1 { get { return t1; } }
    public float T2 { get { return t2; } }


    public CatmullRomSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float alpha)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.alpha = alpha;

        CalculateT();
    }

    public Vector3 GetPoint(float t, bool lerpUnclamped = true)
    {
        float u = lerpUnclamped ? Mathf.LerpUnclamped(t1, t2, t) : t;

        Vector3 A1 = (t1 - u) / (t1 - t0) * p0 + (u - t0) / (t1 - t0) * p1;
        Vector3 A2 = (t2 - u) / (t2 - t1) * p1 + (u - t1) / (t2 - t1) * p2;
        Vector3 A3 = (t3 - u) / (t3 - t2) * p2 + (u - t2) / (t3 - t2) * p3;

        Vector3 B1 = (t2 - u) / (t2 - t0) * A1 + (u - t0) / (t2 - t0) * A2;
        Vector3 B2 = (t3 - u) / (t3 - t1) * A2 + (u - t1) / (t3 - t1) * A3;

        Vector3 C = (t2 - u) / (t2 - t1) * B1 + (u - t1) / (t2 - t1) * B2;
        return C;
    }

    private void CalculateT()
    {
        t1 = GetT(p0, p1);
        t2 = GetT(p1, p2) + t1;
        t3 = GetT(p2, p3) + t2;
    }

    private float GetT(Vector3 a, Vector3 b)
    {
        return Mathf.Pow(Vector3.SqrMagnitude(a - b), 0.5f * alpha);
    }
}
