// Assets/Scripts/DeckController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckController : MonoBehaviour
{
    [Header("Referências de UI")]
    public RectTransform DeckBack;         // IMG_DeckBack (botão)
    public RectTransform GridRoot;         // GRD_Tabuleiro
    public RectTransform DragLayer;        // UI_DragLayer (Canvas Override Sorting ON)
    public RectTransform PosPreviewLeft;   // alvo de preview à esquerda

    [Header("Prefab da Carta (UI)")]
    public GameObject PecaPrefab;          // PF_Peca

    [Header("Sprites (opcional)")]
    public List<Sprite> CartasFrente = new List<Sprite>(); // baralho (faces)
    public Sprite CartaVerso;                                   // verso único

    [Header("Dimensão da Carta")]
    public bool usarCellSizeDaGrelha = true;
    public Vector2 tamanhoCarta = new Vector2(160, 160);
    public bool usarTamanhoDoDeckAoSair = true;

    [Header("Animação")]
    [Range(0.05f, 2f)] public float tDeckParaCentro = 0.8f;
    [Range(0.05f, 2f)] public float tCentroParaPreview = 0.8f;
    [Range(0.05f, 1f)] public float tPulseUp = 0.2f;
    [Range(0.05f, 1f)] public float tPulseDown = 0.18f;
    [Range(0f, 5f)]    public float pausaNoCentro = 0.6f;
    [Range(1.0f, 1.6f)] public float escalaCentro = 1.15f;
    public AnimationCurve curvaPos = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve curvaScale = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Queue<Sprite> _deck;
    bool _jaIniciouTudo = false;

    void Awake()
    {
        // baralhar
        var pool = new List<Sprite>(CartasFrente);
        for (int i = 0; i < pool.Count; i++) { int j = Random.Range(i, pool.Count); (pool[i], pool[j]) = (pool[j], pool[i]); }
        _deck = new Queue<Sprite>(pool);

        if (!DragLayer)
        {
            var canvas = GetComponentInParent<Canvas>();
            DragLayer = canvas ? canvas.transform as RectTransform : null;
        }
    }

    // Ligar no OnClick do IMG_DeckBack
    public void ComprarCarta()
    {
        if (!PecaPrefab || !DeckBack || !DragLayer)
        {
            Debug.LogWarning("DeckController: faltam referências (PecaPrefab/DeckBack/DragLayer).");
            return;
        }

        // Garantir layer por cima
        var dlCanvas = DragLayer.GetComponent<Canvas>() ?? DragLayer.gameObject.AddComponent<Canvas>();
        dlCanvas.overrideSorting = true;
        if (dlCanvas.sortingOrder < 1000) dlCanvas.sortingOrder = 1000;
        if (!DragLayer.GetComponent<GraphicRaycaster>()) DragLayer.gameObject.AddComponent<GraphicRaycaster>();
        DragLayer.SetAsLastSibling();

        // carta da frente (baralho)
        var spriteFrente = (_deck != null && _deck.Count > 0) ? _deck.Dequeue() : null;

        // instanciar
        var go = Instantiate(PecaPrefab, DragLayer, false);
        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();
        var cg  = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        var flip = go.GetComponent<PecaFlip>() ?? go.AddComponent<PecaFlip>();

        // rect
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        // tamanho
        if (usarTamanhoDoDeckAoSair)       rt.sizeDelta = DeckBack.rect.size;
        else if (usarCellSizeDaGrelha && GridRoot)
        {
            var glg = GridRoot.GetComponent<GridLayoutGroup>();
            rt.sizeDelta = glg ? glg.cellSize : tamanhoCarta;
        }
        else                               rt.sizeDelta = tamanhoCarta;

        // preparar flip: começa em VERSO
        if (img) { img.enabled = true; img.raycastTarget = false; img.preserveAspect = true; }
        cg.alpha = 1f; cg.blocksRaycasts = false;
        flip.Configurar(spriteFrente, CartaVerso);   // frente = animal, verso = capa do tile
        flip.MostrarVerso();                         // sair do deck em verso

        // desativar lógica da peça durante anim
        var peca = go.GetComponent<Peca>(); if (peca) peca.enabled = false;

        Canvas.ForceUpdateCanvases();

        // posição inicial (WORLD-SPACE) sobre o deck
        rt.position = WorldCenter(DeckBack);
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();

        StartCoroutine(AnimacaoCartaWorld(go, rt, peca, flip));
    }

    IEnumerator AnimacaoCartaWorld(GameObject go, RectTransform rt, Peca pecaParaReativar, PecaFlip flip)
    {
        Vector3 pDeck   = WorldCenter(DeckBack);
        Vector3 pPrev   = PosPreviewLeft ? WorldCenter(PosPreviewLeft) : (pDeck + Vector3.left * 600f);
        Vector3 pCentro = GridRoot ? WorldCenter(GridRoot) : (pDeck + new Vector3(0f, 120f, 0f));

        // 1) DECK -> CENTRO
        yield return MoverWorld(rt, pDeck, pCentro, tDeckParaCentro, curvaPos, true);

        // 2) Pulse Up
        yield return Escalar(rt, 1f, escalaCentro, tPulseUp, curvaScale);

        // >>> FLIP AQUI (no topo do pulse) <<<
        if (flip != null) yield return flip.FlipParaFrente();

        // 3) Pausa para ler a carta
        if (pausaNoCentro > 0f) yield return new WaitForSecondsRealtime(pausaNoCentro);

        // 4) Pulse Down
        yield return Escalar(rt, escalaCentro, 1f, tPulseDown, curvaScale);

        // 5) CENTRO -> PREVIEW ESQUERDA
        yield return MoverWorld(rt, pCentro, pPrev, tCentroParaPreview, curvaPos, true);

        // Reativar interação da peça
        var img = go.GetComponent<Image>();
        var cg  = go.GetComponent<CanvasGroup>();
        if (img) img.raycastTarget = true;
        if (cg)  cg.blocksRaycasts = true;
        if (pecaParaReativar) pecaParaReativar.enabled = true;

        // Liberta interação / arranca timer só na 1ª carta
        if (!_jaIniciouTudo && !PermissoesJogo.JaLibertou)
        {
            _jaIniciouTudo = true;
            PermissoesJogo.HabilitarInteracao();
            ControladorJogo.Instancia?.IniciarTimer();
        }
    }

    IEnumerator MoverWorld(RectTransform rt, Vector3 de, Vector3 ate, float dur, AnimationCurve curva, bool manterEscala = false)
    {
        float t = 0f;
        float escala = rt.localScale.x;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float e = curva.Evaluate(Mathf.Clamp01(t / dur));
            rt.position = Vector3.LerpUnclamped(de, ate, e);
            if (manterEscala) rt.localScale = new Vector3(escala, escala, 1f);
            yield return null;
        }
        rt.position = ate;
    }

    IEnumerator Escalar(RectTransform rt, float de, float ate, float dur, AnimationCurve curva)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float e = curva.Evaluate(Mathf.Clamp01(t / dur));
            float s = Mathf.LerpUnclamped(de, ate, e);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        rt.localScale = new Vector3(ate, ate, 1f);
    }

    Vector3 WorldCenter(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }
}
