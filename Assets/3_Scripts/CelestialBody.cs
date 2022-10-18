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
        
        keplerOrbitElements.GetOrbitalPath();
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
    public float SemiMinorAxis;
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

        // Constants
        const float twoPI = Mathf.PI * 2f;

        elements.Position = p;
        elements.Velocity = r;

        float gravParam = (float) (OrbitalConstants.GRAVITATIONAL_CONSTANT * (m1 + m2)); // Gravitational parameter

        Vector3 orbMom = InvCross(p, r); // Orbital momentum
        Debug.DrawRay(Vector3.zero, orbMom, Color.yellow, 999f);

        elements.Inclination = Mathf.Acos(orbMom.y / orbMom.magnitude); // Inclination

        Vector3 ev = (InvCross(r, orbMom) / gravParam) - (p / p.magnitude); // Eccentricity vector
        elements.EccentricityVector = ev;
        elements.Eccentricity = ev.magnitude; // Eccentricity

        elements.SemiMajorAxis = 1f / ((2f / p.magnitude) - (Mathf.Pow(r.magnitude, 2) / gravParam)); // Semi Major Axis
        elements.SemiMinorAxis = elements.SemiMajorAxis * Mathf.Sqrt(1f - (elements.Eccentricity * elements.Eccentricity)); // Semi Minor Axis

        Vector3 n = InvCross(Vector3.up, orbMom); // Ascending node vector
        if (Mathf.Abs(elements.Inclination) < 0.01f * Mathf.Deg2Rad || Mathf.Abs(elements.Inclination) > 179.99f * Mathf.Deg2Rad)
            n = Vector3.right;
        
        Debug.DrawRay(Vector3.zero, n, Color.red, 999f);

        elements.ArgumentOfPeriapsis = Mathf.Acos(Vector3.Dot(n, ev) / (n.magnitude * ev.magnitude)); // Argument of periapsis
        if (ev.y <= 0)
            elements.ArgumentOfPeriapsis = twoPI - elements.ArgumentOfPeriapsis;

        if (Math.Abs(elements.ArgumentOfPeriapsis - twoPI) < Mathf.Epsilon)
            elements.ArgumentOfPeriapsis = 0f;

        elements.LongitudeOfAscendingNode = Mathf.Acos(n.x / n.magnitude); // Longitude of the ascending node
        if (n.z < 0)
            elements.LongitudeOfAscendingNode = twoPI - elements.LongitudeOfAscendingNode;

        elements.TrueAnomaly = Mathf.Acos(Vector3.Dot(ev, p) / (ev.magnitude * p.magnitude)); // True anomaly
        if (Vector3.Dot(p, r) < 0)
            elements.TrueAnomaly = twoPI - elements.TrueAnomaly;

        elements.EccentricAnomaly = Mathf.Atan2(Mathf.Sin(elements.TrueAnomaly) * Mathf.Sqrt(1 - (elements.Eccentricity * elements.Eccentricity)), elements.Eccentricity + Mathf.Cos(elements.TrueAnomaly)); // Eccentricity anomaly
        if (elements.EccentricAnomaly < 0f)
            elements.EccentricAnomaly = twoPI + elements.EccentricAnomaly;

        elements.MeanAnomaly = elements.EccentricAnomaly - (elements.Eccentricity * Mathf.Sin(elements.EccentricAnomaly)); // Mean anomaly
        elements.MeanMotion = Mathf.Sqrt(gravParam / Mathf.Pow(elements.SemiMajorAxis, 3)); // Mean motion

        elements.ApoapsisRadius = (1 + elements.Eccentricity) * elements.SemiMajorAxis;
        elements.PeriapsisRadius = (1 - elements.Eccentricity) * elements.SemiMajorAxis;

        elements.OrbitalPeriod = twoPI * Mathf.Sqrt(Mathf.Pow(elements.SemiMajorAxis, 3) / gravParam);

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

    private float MeanToEccentric(float mean)
    {
        float eNew = mean + Eccentricity;
        if (MeanAnomaly > Mathf.PI)
            eNew = mean - Eccentricity;

        float eOld = eNew + 0.001f;

        while (Mathf.Abs(eNew - eOld) > 0.0001f)
        {
            eOld = eNew;
            eNew = eOld + (mean - eOld + ((Eccentricity * Mathf.Sin(eOld)) / (1f - (Eccentricity * Mathf.Cos(eOld)))));
        }

        return eNew;

        /*
        Enew = M + e;
        if (M > pi);
            Enew = M - e;
        end
        Eold = Enew + 0.001;
        while (abs(Enew - Eold) > 1e-8)
            Eold = Enew;
            Enew = Eold + (M - Eold + e*sin(Eold))/(1 - e*cos(Eold));
        end
        E = Enew;
        */
    }

    private float EccentricToTrue(float eccentric)
    {
        float f = Mathf.Atan2(Mathf.Sin(eccentric) * Mathf.Sqrt(1 - (Eccentricity * Eccentricity)), Mathf.Cos(eccentric) - Eccentricity);
        f %= (2f * Mathf.PI);
        if (f < 0f)
            f += (2f * Mathf.PI);

        return f;

        /*
        f = atan2(sin(E)*sqrt(1-e^2), cos(E)-e);
        f = mod(f, 2*pi);
        if (f < 0)
            f = f + 2*pi;
        end
        */
    }

    private Vector3 GetPositionInOrbit(float E)
    {
        // P and Q form a 2d coordinate system in the plane of the orbit, with +P pointing towards periapsis.
        float P = SemiMajorAxis * (Mathf.Cos(E) - Eccentricity);
        float Q = SemiMajorAxis * Mathf.Sin(E) * Mathf.Sqrt(1 - Mathf.Pow(Eccentricity, 2));

        float x = Mathf.Cos(ArgumentOfPeriapsis) * P - Mathf.Sin(ArgumentOfPeriapsis) * Q;
        float y = Mathf.Sin(ArgumentOfPeriapsis) * P + Mathf.Cos(ArgumentOfPeriapsis) * Q;
        y = Mathf.Cos(Inclination) * y;
        float z = Mathf.Sin(Inclination) * y;

        float tempX = x;
        x = Mathf.Cos(LongitudeOfAscendingNode) * tempX - Mathf.Sin(LongitudeOfAscendingNode) * y;
        y = Mathf.Sin(LongitudeOfAscendingNode) * tempX + Mathf.Cos(LongitudeOfAscendingNode) * y;

        return new Vector3(-x, z, y);
    }

    public List<Vector3> GetOrbitalPath()
    {
        List<Vector3> orbitPathPoints = new List<Vector3>();


        return orbitPathPoints;
    }

    public static Vector3 InvCross(Vector3 a, Vector3 b) => Vector3.Cross(a, b) * -1f;

    public override string ToString()
    {
        return
            $"Position: {Position}m\n" +
            $"Velocity: {Velocity}m/s\n" +
            $"Semi Major Axis: {SemiMajorAxis}m\n" +
            $"Semi Minor Axis: {SemiMinorAxis}m\n" +
            $"Eccentricity: {Eccentricity}\n" +
            $"Longitude Of Ascending Node: {LongitudeOfAscendingNode}r {LongitudeOfAscendingNode * Mathf.Rad2Deg}° CC\n" +
            $"Argument Of Periapsis: {ArgumentOfPeriapsis}r {ArgumentOfPeriapsis * Mathf.Rad2Deg}° CC\n" +
            $"Inclination: {Inclination}r {Inclination * Mathf.Rad2Deg}°\n" +
            $"Mean Anomaly:{MeanAnomaly}r {MeanAnomaly * Mathf.Rad2Deg}°\n" +
            $"True Anomaly:{TrueAnomaly}r {TrueAnomaly * Mathf.Rad2Deg}°\n" +
            $"Eccentric Anomaly:{EccentricAnomaly}r {EccentricAnomaly * Mathf.Rad2Deg}°\n" +
            $"Apoapsis Radius: {ApoapsisRadius}m\n" +
            $"Periapsis Radius: {PeriapsisRadius}m\n" +
            $"Orbital Period: {OrbitalPeriod}s\n" +
            $"Mean Motion: {MeanMotion}r/s {MeanMotion * Mathf.Rad2Deg}°/s";
    }

}