using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RoyalSmash.UI
{
    /// <summary>
    /// Coordinates floating text popups for rewards, impacts, skills, and status feedback.
    /// Uses object pooling to eliminate garbage collection allocations.
    /// Follows Master Build Prompt Section 24 & Full Documentation Section 311.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        private static FloatingTextManager _instance;
        public static FloatingTextManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private int _poolSize = 10;
        [SerializeField] private float _defaultDuration = 1.2f;
        [SerializeField] private float _riseDistance = 1.5f;

        private readonly Queue<GameObject> _pool = new Queue<GameObject>();
        private readonly List<GameObject> _activeTexts = new List<GameObject>();

        public int PoolCount => _pool.Count;
        public int ActiveCount => _activeTexts.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            InitializePool();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var textObj = CreateTextObject();
                textObj.SetActive(false);
                _pool.Enqueue(textObj);
            }
        }

        private GameObject CreateTextObject()
        {
            var go = new GameObject("FloatingText_Pooled");
            go.transform.SetParent(transform);

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 6f;
            tmp.fontStyle = FontStyles.Bold;

            return go;
        }

        /// <summary>
        /// Spawns a floating 3D text banner that rises and fades out.
        /// </summary>
        public void SpawnFloatingText(string message, Vector3 worldPosition, Color color, float duration = -1f)
        {
            GameObject textObj = GetFromPool();
            textObj.transform.position = worldPosition;
            textObj.transform.localScale = Vector3.one;

            var tmp = textObj.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = message;
                tmp.color = color;
            }

            textObj.SetActive(true);
            _activeTexts.Add(textObj);

            float activeDuration = duration > 0f ? duration : _defaultDuration;
            StartCoroutine(AnimateAndRecycle(textObj, activeDuration));
        }

        private GameObject GetFromPool()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }
            return CreateTextObject();
        }

        private IEnumerator AnimateAndRecycle(GameObject textObj, float duration)
        {
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * _riseDistance;
            var tmp = textObj.GetComponent<TextMeshPro>();
            Color baseColor = tmp != null ? tmp.color : Color.white;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Upward drift
                textObj.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Scale punch on spawn
                float scale = (t < 0.2f) ? Mathf.Lerp(0.5f, 1.2f, t / 0.2f) : Mathf.Lerp(1.2f, 1.0f, (t - 0.2f) / 0.8f);
                textObj.transform.localScale = Vector3.one * scale;

                // Alpha fade out near end
                if (tmp != null && t > 0.5f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
                    tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                }

                yield return null;
            }

            // Recycle
            textObj.SetActive(false);
            _activeTexts.Remove(textObj);
            _pool.Enqueue(textObj);
        }
    }
}
