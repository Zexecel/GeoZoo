// Assets/Scripts/TileID.cs
using UnityEngine;

public enum TileTipo { A, B, C, D }

[DisallowMultipleComponent]
public class TileID : MonoBehaviour
{
    public TileTipo Tipo;

    public int Rotacao90
    {
        get
        {
            var z = transform.localEulerAngles.z;
            int r = Mathf.RoundToInt(z / 90f) * 90;
            r = (r % 360 + 360) % 360;
            return r;
        }
    }
}
