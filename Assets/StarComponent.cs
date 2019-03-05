using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarComponent : MonoBehaviour
{
    [Header("闪烁频率")]
    [Range(0f, 100f)]
    public float twinkleFrequency;
}
