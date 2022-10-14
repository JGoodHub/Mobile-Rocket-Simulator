using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{

    public const double GRAVITATIONAL_CONSTANT = 6.67e-11;

    public Rigidbody _rigidbody;
    public Vector3 _initialVelocity;

    public CelestialBody _parent;
    public double mass;

    public bool logOrbit;

    private void Start()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.AddForce(_initialVelocity, ForceMode.VelocityChange);

        KeplerOrbitElements keplerOrbitElements = KeplerOrbitElements.FromCartesianStateVector(transform.position, _initialVelocity, (float) _parent.mass, _rigidbody.mass);
        Debug.Log(keplerOrbitElements);
    }

    private void FixedUpdate()
    {
        if (_parent == null)
            return;

        float distance = Vector3.Distance(transform.position, _parent.transform.position);
        double force = GRAVITATIONAL_CONSTANT * ((_parent.mass * _rigidbody.mass) / Mathf.Pow(distance, 2f));

        _rigidbody.AddForce((_parent.transform.position - transform.position).normalized * (float) force);

        if (logOrbit)
        {
            // logOrbit = false;
            KeplerOrbitElements keplerOrbitElements = KeplerOrbitElements.FromCartesianStateVector(_rigidbody.position, _rigidbody.velocity, (float) _parent.mass, _rigidbody.mass);
            Debug.Log(keplerOrbitElements);
        }
    }

}

public static class OrbitalConstants
{

    public const double GRAVITATIONAL_CONSTANT = 6.67e-11;

}

public struct KeplerOrbitElements
{

    public Vector3 Position;
    public Vector3 Velocity;

    public float SemiMajorAxis;
    public float Eccentricity;
    public Vector3 EccentricityVector;
    public float ArgumentOfPeriapsis;
    public float LongitudeOfAscendingNode;
    public float Inclination;

    public float MeanAnomaly;
    public float EccentricAnomaly;
    public float TrueAnomaly;

    public float ApoapsisRadius;
    public float PeriapsisRadius;
    public float OrbitalPeriod;
    public float MeanMotion;

    public static KeplerOrbitElements FromCartesianStateVector(Vector3 p, Vector3 r, float m1, float m2 = 0)
    {
        KeplerOrbitElements elements = new KeplerOrbitElements();

        elements.Position = p;
        elements.Velocity = r;

        float u = (float) (OrbitalConstants.GRAVITATIONAL_CONSTANT * (m1 + m2)); // Gravitational parameter

        elements.SemiMajorAxis = 1f / ((2f / p.magnitude) - (Mathf.Pow(r.magnitude, 2) / u)); // Semi Major Axis

        Vector3 h = InvCross(p, r); // Orbital momentum

        //Debug.DrawRay(Vector3.zero, h, Color.yellow, 999f);

        elements.Inclination = Mathf.Acos(h.y / h.magnitude); // Inclination

        Vector3 ev = (InvCross(r, h) / u) - (p / p.magnitude); // Eccentricity vector
        elements.EccentricityVector = ev;
        elements.Eccentricity = ev.magnitude;

        Vector3 n = InvCross(Vector3.up, h); // Ascending node vector
        if (Mathf.Abs(elements.Inclination) < 0.01f * Mathf.Deg2Rad || Mathf.Abs(elements.Inclination) > 179.99f * Mathf.Deg2Rad)
            n = Vector3.forward;

        //Debug.DrawRay(Vector3.zero, n, Color.magenta, 999f);

        elements.ArgumentOfPeriapsis = Mathf.Acos(Vector3.Dot(n, ev) / (n.magnitude * ev.magnitude)); // Argument of periapsis
        if (ev.y <= 0)
            elements.ArgumentOfPeriapsis = (2f * Mathf.PI) - elements.ArgumentOfPeriapsis;

        elements.LongitudeOfAscendingNode = Mathf.Acos(n.z / n.magnitude); // Longitude of the ascending node
        if (n.x < 0)
            elements.LongitudeOfAscendingNode = (2f * Mathf.PI) - elements.LongitudeOfAscendingNode;

        elements.TrueAnomaly = Mathf.Acos(Vector3.Dot(ev, p) / (ev.magnitude * p.magnitude)); // True anomaly
        if (Vector3.Dot(p, r) < 0)
            elements.TrueAnomaly = (2f * Mathf.PI) - elements.TrueAnomaly;

        elements.EccentricAnomaly = Mathf.Atan2(Mathf.Sin(elements.TrueAnomaly) * Mathf.Sqrt(1 - (elements.Eccentricity * elements.Eccentricity)), elements.Eccentricity + Mathf.Cos(elements.TrueAnomaly)); // Eccentricity anomaly
        if (elements.EccentricAnomaly < 0f)
            elements.EccentricAnomaly = (2f * Mathf.PI) + elements.EccentricAnomaly;

        elements.MeanAnomaly = elements.EccentricAnomaly - (elements.Eccentricity * Mathf.Sin(elements.EccentricAnomaly)); // Mean anomaly
        elements.MeanMotion = Mathf.Sqrt(u / Mathf.Pow(elements.SemiMajorAxis, 3)); // Mean motion

        elements.ApoapsisRadius = (1 + elements.Eccentricity) * elements.SemiMajorAxis;
        elements.PeriapsisRadius = (1 - elements.Eccentricity) * elements.SemiMajorAxis;

        elements.OrbitalPeriod = 2f * Mathf.PI * Mathf.Sqrt(Mathf.Pow(elements.SemiMajorAxis, 3) / u);

        Debug.DrawRay(Vector3.zero, ev.normalized * elements.PeriapsisRadius, Color.blue, 999f);
        Debug.DrawRay(Vector3.zero, -ev.normalized * elements.ApoapsisRadius, Color.blue, 999f);

        return elements;
    }

    public Vector3 ApoapsisPosition()
    {
        return EccentricityVector.normalized * ApoapsisRadius;
    }

    public Vector3 PeriapsisPosition()
    {
        return EccentricityVector.normalized * PeriapsisRadius;
    }

    public List<Vector3> OrbitLinePositions()
    {
        float timeStep = OrbitalPeriod / 100f;
        float time = 0f;
        List<Vector3> orbitLinePositions = new List<Vector3>();

        while (time < OrbitalPeriod)
        {
            float iterationMeanAnomaly = MeanAnomaly + (MeanMotion * time);
            
            // Convert mean to eccentric
            
            
            // Convert eccentric to true
            
            
            // Find position based on true


            time += timeStep;
        }
        

        return orbitLinePositions;
    }

    public static Vector3 InvCross(Vector3 a, Vector3 b) => Vector3.Cross(a, b) * -1f;

    public override string ToString()
    {
        return
            $"Position: {Position}m\n" +
            $"Velocity: {Velocity}m/s\n" +
            $"Semi Major Axis: {SemiMajorAxis}m\n" +
            $"Eccentricity: {Eccentricity}\n" +
            $"Argument Of Periapsis: {ArgumentOfPeriapsis}r {ArgumentOfPeriapsis * Mathf.Rad2Deg}°\n" +
            $"Longitude Of Ascending Node: {LongitudeOfAscendingNode}r {LongitudeOfAscendingNode * Mathf.Rad2Deg}°\n" +
            $"Inclination: {Inclination}r {Inclination * Mathf.Rad2Deg}°\n" +
            $"Mean Anomaly:{MeanAnomaly}r {MeanAnomaly * Mathf.Rad2Deg}°\n" +
            $"True Anomaly:{TrueAnomaly}r {TrueAnomaly * Mathf.Rad2Deg}°\n" +
            $"Eccentric  Anomaly:{EccentricAnomaly}r {EccentricAnomaly * Mathf.Rad2Deg}°\n" +
            $"Apoapsis Radius: {ApoapsisRadius}m\n" +
            $"Periapsis Radius: {PeriapsisRadius}m\n" +
            $"Orbital Period: {OrbitalPeriod}s";
    }

}