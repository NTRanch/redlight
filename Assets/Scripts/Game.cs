// redlight — Redlight (Unity, C#).
// Tiger Style (tiger-style-csharp): explicit contracts, bounded async work,
// nullable discipline. This is the entry MonoBehaviour; game systems live in
// their own assemblies under Assets/Scripts/.
using UnityEngine;

namespace NTRanch.Redlight
{
    public sealed class Game : MonoBehaviour
    {
        private void Awake()
        {
            // Preflight: wire the week's systems here, in dependency order.
        }
    }
}
