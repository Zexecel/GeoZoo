// Assets/Scripts/CelulaTabuleiro.cs
using UnityEngine;

[DisallowMultipleComponent]
public class CelulaTabuleiro : MonoBehaviour
{
    public int X;
    public int Y;

    // Compat com código antigo, se precisares:
    public Vector2Int Coordenada => new Vector2Int(X, Y);
}
