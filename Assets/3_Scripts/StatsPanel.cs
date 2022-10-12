using System;
using System.Collections;
using System.Collections.Generic;
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

        Vector3 rocketRealPosition = FloatingOrigin.Instance.transform.position + rocketController.transform.position;
        Planet.Instance.CalculateApoAndPeriAltitudes(rocketRealPosition, rocketController.Rigidbody.velocity, out float apoapsis, out float periapsis);
        _apoapsisText.text = $"Apoapsis: {apoapsis}m";
        _periapsisText.text = $"Periapsis: {periapsis}m";
    }

}