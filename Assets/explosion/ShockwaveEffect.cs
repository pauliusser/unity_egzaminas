using UnityEngine;
using System.Collections;

public class ShockwaveEffect : MonoBehaviour
{
    public float duration = 0.5f;
    public float blastRadius = 5f; // Will be set by the explosion
    public float lightMaxIntensity = 3f; // Peak intensity of the light

    private Material mat;
    private Light pointLight;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        pointLight = GetComponentInChildren<Light>(); // Find light on same object or child

        StartCoroutine(Animate());
    }

    public void Initialize(float radius)
    {
        blastRadius = radius;
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Scale sphere from 0 to full diameter
            float scale = Mathf.Lerp(0f, blastRadius * 2f, t);
            transform.localScale = Vector3.one * scale;
            
            // Fade alpha
            float alpha = Mathf.Lerp(1f, 0f, t);
            mat.SetFloat("_Alpha", alpha);
            
            // Fade light intensity
            if (pointLight != null)
            {
                pointLight.intensity = Mathf.Lerp(lightMaxIntensity, 0f, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}