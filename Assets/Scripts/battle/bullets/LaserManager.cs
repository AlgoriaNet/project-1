using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace battle.bullets
{
    public class LaserManager : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(191 / 255, 36 / 255, 0);
        public float colorIntensity = 4.3f;
        public float beamColorEnhance = 1;

        public GameObject startVFX;
        public GameObject endVFX;

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponentInChildren<LineRenderer>();
            lineRenderer.material.color = color * colorIntensity;
            var particles = GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                Renderer r = particle.GetComponent<Renderer>();
                r.material.SetColor("_EmissionColor", color * (colorIntensity * beamColorEnhance));
            }
        }
    }
}