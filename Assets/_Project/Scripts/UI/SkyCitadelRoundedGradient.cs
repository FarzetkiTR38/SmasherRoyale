using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>Rounded, vertically shaded UI geometry that keeps the original Image/Button binding.</summary>
    [DisallowMultipleComponent]
    public sealed class SkyCitadelRoundedGradient : BaseMeshEffect
    {
        [SerializeField] private Color _top = Color.white;
        [SerializeField] private Color _middle = Color.white;
        [SerializeField] private Color _bottom = Color.white;
        [SerializeField, Min(0)] private float _radius = 24f;
        [SerializeField, Range(0, 1)] private float _gloss;

        public void Configure(Color top, Color middle, Color bottom, float radius, float gloss = 0f)
        {
            _top = top;
            _middle = middle;
            _bottom = bottom;
            _radius = radius;
            _gloss = gloss;
            if (graphic != null) graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive()) return;
            Rect rect = graphic.rectTransform.rect;
            mesh.Clear();
            if (rect.width <= 0f || rect.height <= 0f) return;
            float radius = Mathf.Clamp(_radius, 0f, Mathf.Min(rect.width, rect.height) * .5f);
            const int strips = 64;
            // Horizontal strips keep every gradient stop horizontal, including around the rounded corners.
            for (int i = 0; i <= strips; i++)
            {
                // Extra samples near the upper/lower corners keep circular rims smooth at phone resolution.
                float t = (1f - Mathf.Cos(i / (float)strips * Mathf.PI)) * .5f;
                float y = rect.yMin + t * rect.height;
                float fromEdge = Mathf.Min(y - rect.yMin, rect.yMax - y);
                float inset = fromEdge >= radius ? 0f : radius -
                    Mathf.Sqrt(Mathf.Max(0f, radius * radius - (radius - fromEdge) * (radius - fromEdge)));
                Color tint = t < .5f ? Color.Lerp(_bottom, _middle, t * 2f) :
                    Color.Lerp(_middle, _top, (t - .5f) * 2f);
                float specular = Mathf.Exp(-Mathf.Pow((t - .83f) * 13f, 2f)) * _gloss;
                tint = Color.Lerp(tint, new Color(1, 1, 1, tint.a), specular) * graphic.color;
                mesh.AddVert(new Vector3(rect.xMin + inset, y), tint, new Vector2(.5f, .5f));
                mesh.AddVert(new Vector3(rect.xMax - inset, y), tint, new Vector2(.5f, .5f));
                if (i == 0) continue;
                int v = i * 2;
                mesh.AddTriangle(v - 2, v, v + 1);
                mesh.AddTriangle(v - 2, v + 1, v - 1);
            }
        }
    }
}
