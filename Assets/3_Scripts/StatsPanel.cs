using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{

    [SerializeField] private Text _velocityText;
    [SerializeField] private Text _altitudeText;

    [SerializeField] private Text _yawText;
    [SerializeField] private Text _pitchText;

    [SerializeField] private Text _throttleText;
    [SerializeField] private Text _thrustText;

    [SerializeField] private Text _apoapsisText;
    [SerializeField] private Text _periapsisText;
    [SerializeField] private Text _inclinationText;
    [SerializeField] private Text _orbitalPeriodText;

    private void Update()
    {
        TestRocketController rocketController = TestRocketController.Instance;

        _velocityText.text = $"Velocity: {Mathf.RoundToInt(rocketController.Rigidbody.velocity.magnitude)} m/s";
        _altitudeText.text = $"Altitude: {Mathf.RoundToInt(Planet.Instance.GetAltitude(rocketController.transform))}m";

        rocketController.GetYawAndPitchRelativeToPlanet(out float yaw, out float pitch);
        _yawText.text = $"Yaw: {Mathf.RoundToInt(yaw)}°";
        _pitchText.text = $"Pitch: {Mathf.RoundToInt(pitch)}°";

        _throttleText.text = $"Throttle: {Mathf.RoundToInt(rocketController.GetThrottle() * 100f)}%";
        _thrustText.text = $"Thrust: {Mathf.RoundToInt(rocketController.GetThrust() / 1000f)}Kn";

        KeplerOrbitElements keplerOrbitElements = rocketController.ComputeRocketOrbitalElements();
        float apoapsisAltitude = Mathf.RoundToInt(Mathf.Clamp(keplerOrbitElements.ApoapsisRadius - Planet.Instance.RadiusSeaLevel, 0f, float.MaxValue));
        float periapsisAltitude = Mathf.RoundToInt(Mathf.Clamp(keplerOrbitElements.PeriapsisRadius - Planet.Instance.RadiusSeaLevel, 0f, float.MaxValue));

        _apoapsisText.text = $"{apoapsisAltitude.ToString(CultureInfo.CurrentCulture).PadLeft(7, '0')}m";
        _periapsisText.text = $"{periapsisAltitude.ToString(CultureInfo.CurrentCulture).PadLeft(7, '0')}m";
        _inclinationText.text = $"Inclination: {Mathf.Round(keplerOrbitElements.Inclination * 10f) / 10f}°";
        _orbitalPeriodText.text = $"Orbital Period: {SecondsToDateString(Mathf.RoundToInt(keplerOrbitElements.OrbitalPeriod))}";
    }

    private static string SecondsToDateString(int totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        string days = $"{timeSpan.Days}d ";
        string hours = $"{timeSpan.Hours}h ";
        string minutes = $"{timeSpan.Minutes}m ";
        string seconds = $"{timeSpan.Seconds}s";

        string output = string.Empty;

        if (timeSpan.Days > 0)
            output += days;

        if (timeSpan.Days > 0 || timeSpan.Hours > 0)
            output += hours;

        if (timeSpan.Days > 0 || timeSpan.Hours > 0 || timeSpan.Minutes > 0)
            output += minutes;

        if (timeSpan.Days > 0 || timeSpan.Hours > 0 || timeSpan.Minutes > 0 || timeSpan.Seconds > 0)
            output += seconds;

        return output;
    }

}