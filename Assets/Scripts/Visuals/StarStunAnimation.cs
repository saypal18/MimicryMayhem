using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StarStunAnimation : MonoBehaviour, IAnimation
{
    private class StarInstance
    {
        public GameObject gameObject;
        public SpriteRenderer spriteRenderer;
        public float radiusX;
        public float radiusY;
        public float phase;

        public StarInstance(GameObject go, SpriteRenderer sr, float rx, float ry, float p)
        {
            gameObject = go;
            spriteRenderer = sr;
            radiusX = rx;
            radiusY = ry;
            phase = p;
        }
    }

    [Header("Settings")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private int starCount = 3;
    [SerializeField] private float minRadiusX = 0.7f;
    [SerializeField] private float maxRadiusX = 0.8f;
    [SerializeField] private float minRadiusY = 0.7f;
    [SerializeField] private float maxRadiusY = 0.8f;
    [SerializeField] private float rotationSpeed = 1.0f; // Radians per second
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Transform starsParent;

    private List<StarInstance> stars = new List<StarInstance>();
    private bool isPlaying = false;
    private bool isFadingIn = false;
    private bool stopPending = false;
    private Sequence fadeSequence;

    private void Awake()
    {
        // Find the PoolingEntity on this object or a parent to handle cleanup when the entity is despawned
        PoolingEntity pe = GetComponentInParent<PoolingEntity>();
        if (pe != null)
        {
            pe.OnDespawning += HandleDespawning;
        }
    }

    private void HandleDespawning()
    {
        DespawnStars();
        fadeSequence?.Kill();
        isPlaying = false;
        stopPending = false;
    }

    public void Play()
    {
        stopPending = false;

        if (isPlaying)
        {
            // Already playing, just ensure we fade to 1
            FadeStars(1f);
            return;
        }

        isPlaying = true;
        SpawnStars();
        FadeStars(1f);
    }

    public void Stop()
    {
        if (!isPlaying) return;

        if (isFadingIn)
        {
            stopPending = true;
            return;
        }

        FadeStars(0f, () => {
            DespawnStars();
            isPlaying = false;
        });
    }

    private void SpawnStars()
    {
        Transform parent = starsParent != null ? starsParent : transform;
        
        // Random overall offset for the group
        float initialOffset = Random.Range(0f, Mathf.PI * 2f);
        float phaseSpacing = (Mathf.PI * 2f) / starCount;

        for (int i = 0; i < starCount; i++)
        {
            GameObject starGo = PoolingEntity.Spawn(starPrefab, parent);
            SpriteRenderer sr = starGo.GetComponent<SpriteRenderer>();
            
            if (sr == null)
            {
                Debug.LogWarning("StarStunAnimation: Star prefab is missing a SpriteRenderer!");
                continue;
            }

            // Set initial alpha to 0 for fade in
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            // Equidistant phases: (2π / count) * index + random group offset
            float p = initialOffset + (i * phaseSpacing);

            float rx = Random.Range(minRadiusX, maxRadiusX);
            float ry = Random.Range(minRadiusY, maxRadiusY);

            stars.Add(new StarInstance(starGo, sr, rx, ry, p));
            UpdateStarPosition(stars[stars.Count - 1]);
        }
    }

    private void DespawnStars()
    {
        foreach (var star in stars)
        {
            PoolingEntity.Despawn(star.gameObject);
        }
        stars.Clear();
    }

    private void FadeStars(float targetAlpha, System.Action onComplete = null)
    {
        fadeSequence?.Kill();
        fadeSequence = DOTween.Sequence();

        if (targetAlpha > 0.5f) isFadingIn = true;

        foreach (var star in stars)
        {
            fadeSequence.Join(star.spriteRenderer.DOFade(targetAlpha, fadeDuration));
        }

        fadeSequence.OnComplete(() => {
            isFadingIn = false;
            if (stopPending && targetAlpha > 0.5f)
            {
                Stop();
            }
            onComplete?.Invoke();
        });
    }

    private void Update()
    {
        if (!isPlaying) return;

        foreach (var star in stars)
        {
            star.phase += rotationSpeed * Time.deltaTime;
            UpdateStarPosition(star);
        }
    }

    private void UpdateStarPosition(StarInstance star)
    {
        float x = star.radiusX * Mathf.Sin(star.phase);
        float y = star.radiusY * Mathf.Cos(star.phase);
        star.gameObject.transform.localPosition = new Vector3(x, y, 0);
    }

    private void OnDestroy()
    {
        fadeSequence?.Kill();
    }
}
