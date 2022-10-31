using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour
{

    [SerializeField] private Slider _slider;

    private float _defaultFixedDeltaTime;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        _slider.onValueChanged.AddListener(UpdateTimeScale);
    }

    private void UpdateTimeScale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
    }

}