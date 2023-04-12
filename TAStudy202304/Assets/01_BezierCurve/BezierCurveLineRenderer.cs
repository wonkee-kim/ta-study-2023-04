using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BezierCurveLineRenderer : MonoBehaviour
{
    public enum CurveType
    {
        Quadratic,
        Cubic,
    }

    [SerializeField] private CurveType _curveType;

    [SerializeField] private Transform _p0;
    [SerializeField] private Transform _p1;
    [SerializeField] private Transform _p2;
    [SerializeField] private Transform _p3;

    [SerializeField] private int _resolution = 10;
    [SerializeField] private LineRenderer _lineRenderer;

    private void Start()
    {
        _lineRenderer.positionCount = _resolution + 1;
    }

    private void Update()
    {
        Vector3[] points = new Vector3[_resolution + 1];
        for (int i = 0; i <= _resolution; i++)
        {
            float t = i / (float)_resolution;
            points[i] = _curveType == CurveType.Quadratic
                ? BezierCurve.CalculateQuadraticBezierPoint(t, _p0.position, _p1.position, _p2.position)
                : BezierCurve.CalculateCubicBezierPoint(t, _p0.position, _p1.position, _p2.position, _p3.position);
        }
        _lineRenderer.SetPositions(points);
    }
}
