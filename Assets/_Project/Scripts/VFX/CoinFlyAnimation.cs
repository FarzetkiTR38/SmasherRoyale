using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.VFX
{
    /// <summary>
    /// Animates a batch of reward coins traveling along a curved path to the gold widget.
    /// Follows Master Build Prompt Section 22 (UI Reward Animation).
    /// </summary>
    public class CoinFlyAnimation : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.75f;
        [SerializeField] private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public void AnimateFlight(Vector3 startScreenPos, Vector3 targetScreenPos, float delay, Action onArrive = null)
        {
            StartCoroutine(FlightRoutine(startScreenPos, targetScreenPos, delay, onArrive));
        }

        private IEnumerator FlightRoutine(Vector3 start, Vector3 target, float delay, Action onArrive)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            transform.position = start;
            transform.localScale = Vector3.zero;

            // Random control point for curved arc
            Vector3 midPoint = (start + target) * 0.5f;
            float arcOffset = UnityEngine.Random.Range(100f, 250f);
            midPoint.x += UnityEngine.Random.Range(-120f, 120f);
            midPoint.y += arcOffset;

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                float curvedT = _easeCurve != null ? _easeCurve.Evaluate(t) : t;

                // Scale up quickly, then stay normal size
                if (t < 0.2f)
                {
                    transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t / 0.2f);
                }
                else if (t < 0.35f)
                {
                    transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, (t - 0.2f) / 0.15f);
                }
                else
                {
                    transform.localScale = Vector3.one;
                }

                // Quadratic Bezier interpolation
                Vector3 p0 = Vector3.Lerp(start, midPoint, curvedT);
                Vector3 p1 = Vector3.Lerp(midPoint, target, curvedT);
                transform.position = Vector3.Lerp(p0, p1, curvedT);

                yield return null;
            }

            transform.position = target;
            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
