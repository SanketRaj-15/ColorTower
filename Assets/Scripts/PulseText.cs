using UnityEngine;

public class PulseText : MonoBehaviour
{
    public float pulseSpeed = 3f;
    public float pulseSize = 0.12f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;

        Debug.Log(gameObject.name + " pulse animation started");
    }

    void Update()
    {
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseSize;

        transform.localScale = startScale + new Vector3(pulse, pulse, pulse);
    }
}
