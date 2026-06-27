using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AuAvengers.Animations;

[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter",
    Justification = "Unity")]
public sealed class UE_SpriteFlipper : MonoBehaviour
{ public Il2CppArrayBase<SpriteRenderer> RenderersArray { get; set; }

    public bool UseNegative { get; set; }
    public bool DoOffset { get; set; }
    public float Offset { get; set; } = 0.8f;
 public CosmeticsLayer reference { get; set; }

    public void Start()
    {
        RenderersArray = GetComponentsInChildren<SpriteRenderer>(true);
    }

    [SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested",
        Justification = "Too much work")]
    public void Update()
    {
        if (RenderersArray.Count != 0)
        {
            foreach (var rend in RenderersArray)
            {
                rend.flipX = reference.FlipX;

                if (DoOffset)
                {
                    transform.parent.localPosition = new Vector3(reference.FlipX ? UseNegative ? -Offset : Offset : 0,
                        transform.parent.localPosition.y, transform.parent.localPosition.z);
                }
            }
        }
    }
}