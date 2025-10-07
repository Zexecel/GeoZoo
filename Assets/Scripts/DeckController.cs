using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform DeckBack;        // IMG_DeckBack
    public RectTransform GridRoot;        // (opcional)
    public RectTransform PosPreviewLeft;  // POS_PreviewLeft
    public RectTransform DragLayer;       // UI_DragLayer

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

    void Awake()
    {
        ValidarLigações("Awake");
        GarantirOnClick();
    }

    void OnValidate()
    {
        // corre no editor quando mexes no Inspector
        ValidarLigações("OnValidate");
    }

    void GarantirOnClick()
    {
        if (DeckBack == null) return;
        var btn = DeckBack.GetComponent<Button>();
        if (!btn) btn = DeckBack.gameObject.AddComponent<Button>();

        // Evita listeners duplicados
        btn.onClick.RemoveListener(ComprarCarta);
        btn.onClick.AddListener(ComprarCarta);
    }

    void ValidarLigações(string origem)
    {
        string msg =
            $"[DeckController::{origem}] " +
            $"DeckBack={(DeckBack ? DeckBack.name : "null")}, " +
            $"PosPreviewLeft={(PosPreviewLeft ? PosPreviewLeft.name : "null")}, " +
            $"DragLayer={(DragLayer ? DragLayer.name : "null")}, " +
            $"CartasCount={(Cartas != null ? Cartas.Count : -1)}";

        Debug.Log(msg);

        if (!DeckBack) Debug.LogWarning("[DeckController] Falta ligar DeckBack (ex.: IMG_DeckBack).");
        if (!PosPreviewLeft) Debug.LogWarning("[DeckController] Falta ligar PosPreviewLeft (um RectTransform à esquerda).");
        if (!DragLayer) Debug.LogWarning("[DeckController] Falta ligar DragLayer (UI_DragLayer com Canvas override sorting).");
        if (Cartas == null || Cartas.Count == 0)
            Debug.LogWarning("[DeckController] A lista Cartas está vazia — adiciona sprites importadas como Sprite (2D and UI).");
    }

    public void ComprarCarta()
    {
        if (DeckBack == null || PosPreviewLeft == null || DragLayer == null || Cartas == null || Cartas.Count == 0)
        {
            Debug.LogWarning("[DeckController] Faltam referências ou sprites. Verifica os avisos acima.");
            return;
        }

        var sprite = Cartas[Random.Range(0, Cartas.Count)];

        var go = new GameObject("CartaTemp", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _cartaRT = go.GetComponent<RectTransform>();
        _cartaRT.SetParent(DragLayer, false);
        _cartaRT.pivot = new Vector2(0.5f, 0.5f);
        _cartaRT.anchorMin = _cartaRT.anchorMax = new Vector2(0.5f, 0.5f);
        _cartaRT.sizeDelta = new Vector2(256, 256);

        _cartaAtualImg = go.GetComponent<Image>();
        _cartaAtualImg.raycastTarget = false;
        _cartaAtualImg.sprite = sprite;
        _cartaRT.anchoredPosition = WorldToCanvasPos(DeckBack);

        StartCoroutine(AnimarCartaAtePreview());
    }

    IEnumerator AnimarCartaAtePreview()
    {
        Vector2 startPos = WorldToCanvasPos(DeckBack);
        Vector2 centro = Vector2.zero;
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

        _cartaRT.anchoredPosition = alvo;
        _cartaRT.localScale = Vector3.one;

        Debug.Log("CARTA pronta no preview esquerdo");

        PermissoesJogo.HabilitarInteracao();
        if (ControladorJogo.Instancia != null)
            ControladorJogo.Instancia.IniciarTimer();
        else
            Debug.LogWarning("[DeckController] ControladorJogo.Instancia é null — não consegui iniciar o timer.");
    }

    Vector2 WorldToCanvasPos(RectTransform rt)
    {
        Canvas canvas = DragLayer ? DragLayer.GetComponentInParent<Canvas>() : null;
        if (canvas == null) return Vector2.zero;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            rt.position);

        RectTransform canvasRT = (RectTransform)canvas.transform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint);

        return localPoint;
    }
}
