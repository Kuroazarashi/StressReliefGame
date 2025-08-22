using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectTransparencer : MonoBehaviour
{
    public Transform player;
    public LayerMask layerToHide;
    public float transparency = 0.3f;
    public float transitionDuration = 0.5f;

    private Dictionary<Renderer, Coroutine> activeCoroutines = new Dictionary<Renderer, Coroutine>();

    private void Update()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, (player.position - transform.position).normalized, Vector3.Distance(transform.position, player.position), layerToHide);

        HashSet<Renderer> currentHits = new HashSet<Renderer>();

        foreach (var hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                currentHits.Add(renderer);
                if (!activeCoroutines.ContainsKey(renderer))
                {
                    Coroutine newCoroutine = StartCoroutine(SetTransparency(renderer, transparency, transitionDuration));
                    activeCoroutines.Add(renderer, newCoroutine);
                }
            }
        }

        List<Renderer> renderersToRestore = new List<Renderer>();
        foreach (var renderer in activeCoroutines.Keys)
        {
            if (!currentHits.Contains(renderer))
            {
                renderersToRestore.Add(renderer);
            }
        }

        foreach (var renderer in renderersToRestore)
        {
            if (activeCoroutines.ContainsKey(renderer))
            {
                StopCoroutine(activeCoroutines[renderer]);
                activeCoroutines.Remove(renderer);
                StartCoroutine(SetTransparency(renderer, 1.0f, transitionDuration));
            }
        }
    }

    private IEnumerator SetTransparency(Renderer renderer, float targetAlpha, float duration)
    {
        if (!renderer.material.HasProperty("_Color"))
        {
            yield break;
        }

        Color startColor = renderer.material.color;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            Color newColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, targetAlpha, t));
            renderer.material.color = newColor;
            yield return null;
        }

        Color finalColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        renderer.material.color = finalColor;
    }
}