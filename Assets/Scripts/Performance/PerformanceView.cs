using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceView : MonoBehaviour
{
    public PerformanceData Data { get; private set; }

    private float _updateInterval = 0.2f;

    private float _timer;

    private void Awake()
    {
        Data = new PerformanceData();
        Data.Initialize();
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if(_timer< _updateInterval)
            return;

        Data.Check(_timer);
        _timer = 0f;

    }

    private void OnDestroy()
    {
        Data.Dispose();
    }
}
