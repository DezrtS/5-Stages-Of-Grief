using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Effects/Flash Effect/Flash Effect Data")]
public class FlashEffectData : ScriptableObject
{
    [SerializeField] private FlashColor[] flashColors;

    public FlashColor[] FlashColors { get { return flashColors; } }
}

[Serializable]
public class FlashColor
{
    [SerializeField] private Color color;
    [SerializeField] private float colorDuration;
    [Range(0, 1)]
    [SerializeField] private float flashAmount;
    [Range(0, 1)]
    [SerializeField] private float flashEmission;

    public Color Color { get { return color; } }
    public float ColorDuration { get { return colorDuration; } }
    public float FlashAmount { get {  return flashAmount; } }
    public float FlashEmission { get {  return flashEmission; } }
}