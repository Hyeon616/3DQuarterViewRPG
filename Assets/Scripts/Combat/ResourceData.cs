using UnityEngine;

[System.Serializable]
public class ResourceData
{
    [SerializeField] private string resourceName;
    [SerializeField] private float maxValue;
    [SerializeField] private float initialValue;

    public string ResourceName => resourceName;
    public float MaxValue => maxValue;
    public float InitialValue => initialValue;
}