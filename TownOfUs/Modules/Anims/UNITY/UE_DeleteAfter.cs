using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace AuAvengers.Animations;

[RegisterInIl2Cpp]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Unity")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter",
    Justification = "Unity")]
public sealed class UE_DeleteAfter : MonoBehaviour
{
    public float endTime = -1f;
    public float currentPosition;
    public int inst;
    public Action<UE_DeleteAfter, int> after;

    public void Update()
    {
        currentPosition += Time.deltaTime;
        if (currentPosition > endTime)
        {
            if (after != null)
            {
                after(this, inst);
            }

            gameObject.DeepDestroy();
        }
    }
}