using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Valida e posiciona peças (PF_Peca) na grelha GRD_Tabuleiro.
/// - Deteta colisão com células (CelulaTabuleiro)
/// - Encaixa no centro da célula mais próxima
/// - Notifica o ControladorJogo quando o encaixe é válido
/// </summary>
public class GridValidator : MonoBehaviour
{
    [Header("Referências")]
    public RectTransform GridRoot;           // GRD_Tabuleiro
    public GridLayoutGroup GridLayout;       // GridLayoutGroup do GRD_Tabuleiro

    [Header("Regras de Snap")]
    public float DistanciaSnapMax = 60f;     // distância máxima para snap automático
    public bool SnappingAtivo = true;

    /// <summary>
    /// Tenta encaixar a peça na célula mais próxima dentro do grid.
    /// Retorna true se encaixou (sucesso), false se não.
    /// </summary>
    public bool TentarEncaixar(Peca peca, RectTransform pecaRT)
    {
        if (!SnappingAtivo || GridRoot == null || GridLayout == null || pecaRT == null)
            return false;

        // Converter posição da peça para o espaço local da grelha
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GridRoot, RectTransformUtility.WorldToScreenPoint(null, pecaRT.position),
            null, out localPos);

        // Calcular célula mais próxima
        Vector2 cellSize = GridLayout.cellSize + GridLayout.spacing;
        Vector2 gridCenterOffset = new Vector2(
            (GridRoot.rect.width - cellSize.x * GridLayout.constraintCount) * 0.5f,
            -(GridRoot.rect.height - cellSize.y * GridLayout.constraintCount) * 0.5f
        );

        // Encontrar célula mais próxima
        Vector2 posRelativa = localPos - gridCenterOffset;
        int col = Mathf.RoundToInt(posRelativa.x / cellSize.x);
        int row = Mathf.RoundToInt(-posRelativa.y / cellSize.y);

        // Posição alvo
        Vector2 posSnap = new Vector2(col * cellSize.x, -row * cellSize.y) + gridCenterOffset;

        // Distância até ao alvo
        float dist = Vector2.Distance(localPos, posSnap);
        if (dist > DistanciaSnapMax)
            return false; // demasiado longe

        // Encaixar (reposicionar e reparentar para a grelha)
        pecaRT.SetParent(GridRoot, false);
        pecaRT.anchoredPosition = posSnap;
        pecaRT.localRotation = Quaternion.identity;
        pecaRT.localScale = Vector3.one;

        // Opcional: se quiseres “fixar” a peça
        var cg = pecaRT.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = true;

        Debug.Log($"[GridValidator] Peça {peca.name} encaixada na célula ({col},{row})");

        // Notificar ControladorJogo (acerto)
        if (ControladorJogo.Instancia != null)
        {
            ControladorJogo.Instancia.AddZoo(1);
            ControladorJogo.Instancia.AddTempo(20);
        }

        return true;
    }
}
