using UnityEngine;

public class CuratorLightController : MonoBehaviour
{
    public static CuratorLightController instance;

    [Header("References")]
    public Light curatorLight;

    [Header("Idle pulse")]
    public float idlePulseSpeed  = 0.8f;
    public float idlePulseAmount = 0.15f;
    public float baseIntensity   = 0.8f;

    private float flashIntensity = 0f;

    void Awake() { instance = this; }

    void Update()
    {
        if (curatorLight == null) return;

        // Gentle idle pulse — the room quietly breathes
        // in sync with the curator's presence
        float idle = baseIntensity
            + Mathf.Sin(Time.time * idlePulseSpeed) * idlePulseAmount;

        // Flash intensity decays over time — called by TriggerFlash()
        flashIntensity = Mathf.MoveTowards(
            flashIntensity, 0f, Time.deltaTime * 2f);

        curatorLight.intensity = idle + flashIntensity;
    }

    // Called when the curator finds new connections —
    // the room lights up briefly to signal AI activity
    public void TriggerFlash()
    {
        flashIntensity = 2.5f;
    }
}