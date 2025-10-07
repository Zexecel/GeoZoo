using UnityEngine;
using UnityEngine.UI;

public class GridValidator : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform GridRoot;     // GRD_Tabuleiro
    public GridLayoutGroup GridLayout; // GridLayoutGroup do GRD_Tabuleiro

    [Header("Snap")]
    public float DistanciaSnapMax = 80f;
    public bool SnappingAtivo = true;

    public bool TentarEncaixar(Peca peca, RectTransform pecaRT)
    {
        if (!SnappingAtivo || !GridRoot || !GridLayout || !pecaRT) return false;

        // posição da peça no espaço da grelha
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GridRoot,
            RectTransformUtility.WorldToScreenPoint(null, pecaRT.position),
            null,
            out localPos
        );

        var pad  = GridLayout.padding;
        var cell = GridLayout.cellSize;
        var gap  = GridLayout.spacing;

        float left = -GridRoot.rect.width * 0.5f + pad.left + cell.x * 0.5f;
        float top  =  GridRoot.rect.height * 0.5f - pad.top  - cell.y * 0.5f;

        float stepX = cell.x + gap.x;
        float stepY = cell.y + gap.y;

        int col = Mathf.RoundToInt((localPos.x - left) / stepX);
        int row = Mathf.RoundToInt((top - localPos.y) / stepY);

        Vector2 posSnap = new Vector2(left + col * stepX, top - row * stepY);

        if (Vector2.Distance(localPos, posSnap) > DistanciaSnapMax) return false;

        // passa a filha da grelha e garante topo
        pecaRT.SetParent(GridRoot, false);
        pecaRT.SetAsLastSibling();

        // normaliza
        pecaRT.anchoredPosition = new Vector2(Mathf.Round(posSnap.x), Mathf.Round(posSnap.y));
        pecaRT.localRotation = Quaternion.identity;
        pecaRT.localScale    = Vector3.one;

        return true;
    }
}
