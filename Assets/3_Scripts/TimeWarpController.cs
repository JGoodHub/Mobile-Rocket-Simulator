using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeWarpController : MonoBehaviour
{

    [SerializeField] private Slider _slider;
    [SerializeField] private Text _warpFactorText;

    private float _defaultFixedDeltaTime;

    public int[] TimeWarpScales = {1, 2, 4, 8, 16, 32, 64, 128};

    private void OnValidate()
    {
        _slider.minValue = 0;
        _slider.maxValue = TimeWarpScales.Length - 1;
        _slider.wholeNumbers = true;
    }

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        _slider.onValueChanged.AddListener(UpdateTimeScale);

        _slider.value = 0;
        _warpFactorText.text = "x1";
    }

    private void UpdateTimeScale(float value)
    {
        Time.timeScale = TimeWarpScales[Mathf.RoundToInt(value)];
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;

        _warpFactorText.text = $"x{TimeWarpScales[Mathf.RoundToInt(value)]}";
    }

}