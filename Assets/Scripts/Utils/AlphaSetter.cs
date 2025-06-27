using System.Collections;
using System.Collections.Generic;
// using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class AlphaSetter : MonoBehaviour
{

    public bool transist = false;
    public float TargetAlpha = 1f;
    public float CurrentAlpha = 1f;
    public float updateFactor = 0.1f;

    void Update()
    {
        if(transist){
            if (TargetAlpha > 1f) TargetAlpha = 1f;
            if (TargetAlpha < 0f) TargetAlpha = 0f;
            CurrentAlpha = Mathf.Lerp(CurrentAlpha, TargetAlpha, updateFactor);
            SetAlpha(CurrentAlpha);
        }
    }

    public void SetTargetAlpha(float alpha)
    {
        TargetAlpha = alpha;
        transist = true;
    }

    public void SetAlpha(float alpha)
    {
        TryGetComponent<CanvasGroup>(out var canvasGroup);
        if (canvasGroup != null) canvasGroup.alpha = alpha;

        TryGetComponent<SpriteRenderer>(out var spriteRenderer);
        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        TryGetComponent<UnityEngine.UI.Image>(out var image);
        if (image != null)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        TryGetComponent<LineRenderer>(out var lineRenderer);
        if (lineRenderer != null)
        {
            var color = lineRenderer.startColor;
            color.a = alpha;
            lineRenderer.startColor = color;
            color = lineRenderer.endColor;
            color.a = alpha;
            lineRenderer.endColor = color;
        }

        TryGetComponent<ParticleSystem>(out var particleSystem);
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha));
        }

        TryGetComponent<TMP_Text>(out var textMeshPro);
        if (textMeshPro != null)
        {
            var color = textMeshPro.color;
            color.a = alpha;
            textMeshPro.color = color;
        }
    }
}
