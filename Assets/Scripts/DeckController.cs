using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla compra/animação da carta do deck:
/// - Sai do deck (direita), vai ao centro com "zoom", vira para a frente (se tiveres flip),
/// - segue para o preview esquerdo (POS_PreviewLeft),
/// - no fim: PermissoesJogo.HabilitarInteracao() e ControladorJogo.Instancia.IniciarTimer().
/// </summary>
public class DeckController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform DeckBack;        // IMG_DeckBack (ponto de partida da carta)
    public RectTransform GridRoot;        // GRD_Tabuleiro (se precisares como referência)
    public RectTransform PosPreviewLeft;  // alvo à esquerda onde a carta fica parada
    public RectTransform DragLayer;       // UI_DragLayer (para spawn da carta por cima)

    [Header("Cartas (sprites frente)")]
    public List<Sprite> Cartas = new List<Sprite>();

    [Header("Animação")]
    public float DurAteCentro = 0.35f;
    public float DurCentroAteEsquerda = 0.35f;
    public float EscalaNoCentro = 1.2f;
    public AnimationCurve EasePos = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve EaseScale = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Image _cartaAtualImg;
    RectTransform _cartaRT;

    public void ComprarCarta()
    {
        if (Cartas == null || Cartas.Count == 0 || DeckBack == null || PosPreviewLeft == null || DragLayer == null)
        {
            Debug.LogWarning("[DeckController] Faltam referências ou sprites.");
            return;
        }

        // Escolher sprite (podes trocar por lógica com seed/stack)
        var sprite = Cartas[Random.Range(0, Cartas.Count)];

        // Instanciar objeto de carta temporário no DragLayer (sempre por cima)
        var go = new GameObject("CartaTemp", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _cartaRT = go.GetComponent<RectTransform>();
        _cartaRT.SetParent(DragLayer, false);
        _cartaRT.pivot = new Vector2(0.5f, 0.5f);
        _cartaRT.anchorMin = _cartaRT.anchorMax = new Vector2(0.5f, 0.5f);
        _cartaRT.sizeDelta = new Vector2(256, 256);

        _cartaAtualImg = go.GetComponent<Image>();
        _cartaAtualImg.raycastTarget = false; // carta animada não precisa de raycasts
        _cartaAtualImg.sprite = sprite;
        _cartaRT.anchoredPosition = WorldToCanvasPos(DeckBack);

        // Animação
        StartCoroutine(AnimarCartaAtePreview());
    }

    IEnumerator AnimarCartaAtePreview()
    {
        // 1) deck → centro (escala up)
        Vector2 startPos = WorldToCanvasPos(DeckBack);
        Vector2 centro = Vector2.zero; // centro do ecrã no Canvas do DragLayer
        Vector3 escStart = Vector3.one;
        Vector3 escCentro = Vector3.one * EscalaNoCentro;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, DurAteCentro);
            float e = EasePos.Evaluate(Mathf.Clamp01(t));
            _cartaRT.anchoredPosition = Vector2.LerpUnclamped(startPos, centro, e);

            float es = EaseScale.Evaluate(Mathf.Clamp01(t));
            _cartaRT.localScale = Vector3.LerpUnclamped(escStart, escCentro, es);
            yield return null;
        }

        // (Opcional) aqui poderias fazer flip de verso→frente.

        // 2) centro → preview esquerdo (escala volta a 1)
        Vector2 alvo = WorldToCanvasPos(PosPreviewLeft);
        Vector3 escEnd = Vector3.one;

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, DurCentroAteEsquerda);
            float e = EasePos.Evaluate(Mathf.Clamp01(t));
            _cartaRT.anchoredPosition = Vector2.LerpUnclamped(centro, alvo, e);

            float es = EaseScale.Evaluate(Mathf.Clamp01(t));
            _cartaRT.localScale = Vector3.LerpUnclamped(escCentro, escEnd, es);
            yield return null;
        }

        // Fix final
        _cartaRT.anchoredPosition = alvo;
        _cartaRT.localScale = Vector3.one;

        Debug.Log("CARTA pronta no preview esquerdo");

        // Liberta interação e inicia o timer
        PermissoesJogo.HabilitarInteracao();
        if (ControladorJogo.Instancia != null)
        {
            ControladorJogo.Instancia.IniciarTimer();
        }
        else
        {
            Debug.LogWarning("[DeckController] ControladorJogo.Instancia é null — não consegui iniciar o timer.");
        }
    }

    Vector2 WorldToCanvasPos(RectTransform rt)
    {
        // Converte a posição world de um RT para coords locais do Canvas do DragLayer
        Canvas canvas = DragLayer.GetComponentInParent<Canvas>();
        if (canvas == null) return Vector2.zero;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            rt.position);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform, screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint);

        return localPoint;
    }
}
