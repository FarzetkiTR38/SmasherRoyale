using UnityEngine;
using UnityEngine.UI;

namespace RoyalSmash.UI
{
    /// <summary>Resolution-independent round HUD shapes, preserving Image and Button bindings.</summary>
    [DisallowMultipleComponent]
    public sealed class SkyCitadelCircleGraphic : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive()) return;
            Rect rect = graphic.rectTransform.rect;
            const int segments = 96;
            vertices.Clear();
            vertices.AddVert(rect.center, graphic.color, new Vector2(.5f,.5f));
            float radius = Mathf.Min(rect.width,rect.height)*.5f;
            for (int i=0;i<=segments;i++)
            {
                float a=i*2*Mathf.PI/segments;
                Vector2 point=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                // Sample the opaque center of the existing UI sprite, avoiding its low-resolution edge.
                vertices.AddVert(rect.center+point*radius,graphic.color,new Vector2(.5f,.5f));
                if(i>0)vertices.AddTriangle(0,i,i+1);
            }
        }
    }
}
