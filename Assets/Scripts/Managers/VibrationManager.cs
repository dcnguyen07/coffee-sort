using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Lofelt.NiceVibrations;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance; 

    private int isShock;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isShock = PlayerPrefs.GetInt("Shock", 1);
    }
    public void Vibrate()
    {
        if (isShock == 1)
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    public void Refresh()
    {
        isShock = PlayerPrefs.GetInt("Shock");
    }
}


public enum HapticType
{
    Light = 0, 
    Medium = 1,
    Heavy = 2 
}