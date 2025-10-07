// Assets/Scripts/Peca.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Peca : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    // -------- Foco exclusivo (apenas uma peça focada de cada vez) --------
    static Peca _focada;
    static void DefinirFoco(Peca nova) => _focada = nova;
    // ---------------------------------------------------------------------

    [Header("Referências")]
    public RectTransform DragLayer;         // UI_DragLayer (Canvas com Override Sorting ON)
    public RectTransform GridRoot;          // GRD_Tabuleiro
    public GridLayoutGroup GridLayout;      // GridLayout do tabuleiro
    public RectTransform MaoRoot;           // UI_BarraPecas
    public GraphicRaycaster Raycaster;      // GraphicRaycaster do Canvas principal

    [Header("Comportamento")]
    public bool ajustarParaCellNaGrelha = true;
    [Range(0.2f, 1f)] public float alphaDuranteDrag = 0.85f;
    public bool bloquearCelulaOcupada = true;

    [Header("Rotação")]
    public bool permitirRotacao = true;
    public KeyCode teclaRotCCW = KeyCode.Q;   // -90°
    public KeyCode teclaRotCW  = KeyCode.E;   // +90°
    [Range(0.05f, 0.3f)] public float tempoRotacao = 0.12f;

    // internos
    RectTransform _rt;
    CanvasGroup _cg;
    Canvas _canvas;
    Camera _uiCam; // null em Overlay
    EventSystem _eventSystem;
    PecaFlip _flip;

    Vector2 _sizeOriginalNaMao;
    Transform _parentOriginal;
    int _siblingIndexOriginal;
    bool _estavaNaGrelha;

    bool _aRodar;
    bool _aArrastar;

    readonly List<RaycastResult> _hits = new List<RaycastResult>(16);

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying) TryAutoAssignReferences();
    }
#endif

    void Awake()
    {
        TryAutoAssignReferences();

        _rt     = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        _cg     = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();
        _uiCam  = (_canvas && _canvas.renderMode == RenderMode.ScreenSpaceCamera) ? _canvas.worldCamera : null;
        _eventSystem = EventSystem.current;
        _flip   = GetComponent<PecaFlip>();

        _sizeOriginalNaMao = _rt.sizeDelta;

        if (!GridLayout && GridRoot)
            GridLayout = GridRoot.GetComponent<GridLayoutGroup>();
    }

    void Update()
    {
        if (!PermissoesJogo.PodeInteragir || !permitirRotacao) return;

        // roda se estiver a arrastar OU se for a focada
        bool possoRodar = _aArrastar || ReferenceEquals(_focada, this);
        if (possoRodar && !_aRodar)
        {
            if (Input.GetKeyDown(teclaRotCCW)) StartCoroutine(RodarSuave(-90f));
            else if (Input.GetKeyDown(teclaRotCW)) StartCoroutine(RodarSuave(+90f));
        }
    }

    // -------------------------------------------------------------
    // INTERAÇÃO
    // -------------------------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!PermissoesJogo.PodeInteragir) return;

        // esta peça fica focada
        DefinirFoco(this);

        // botão direito = flip frente/verso só desta peça
        if (eventData.button == PointerEventData.InputButton.Right && _flip != null)
        {
            bool estaNaFrente = TryGetImageSprite() == _flip.SpriteFrente;
            if (estaNaFrente) StartCoroutine(_flip.FlipParaVerso());
            else              StartCoroutine(_flip.FlipParaFrente());
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!PermissoesJogo.PodeInteragir) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (DragLayer == null) return;

        _aArrastar = true;
        DefinirFoco(this);

        _parentOriginal   = _rt.parent;
        _siblingIndexOriginal = _rt.GetSiblingIndex();
        _estavaNaGrelha   = EstaEmGrelha(_rt);

        _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.pivot     = new Vector2(0.5f, 0.5f);

        Vector3 worldAntes = _rt.position;
        _rt.SetParent(DragLayer, worldPositionStays: true);
        _rt.position = worldAntes;
        _rt.SetAsLastSibling();

        _cg.blocksRaycasts = false;
        _cg.alpha = alphaDuranteDrag;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragLayer, eventData.position, _uiCam, out var local);
        _rt.anchoredPosition = local;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!PermissoesJogo.PodeInteragir || DragLayer == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            DragLayer, eventData.position, _uiCam, out var local);
        _rt.anchoredPosition = local;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _aArrastar = false;

        if (!PermissoesJogo.PodeInteragir) { VoltarParaOrigem(); return; }

        _cg.blocksRaycasts = true;
        _cg.alpha = 1f;

        RectTransform celula = EncontrarCelulaDebaixoDoCursor(eventData);
        if (celula != null && (CelulaEstaLivre(celula) || !bloquearCelulaOcupada))
        {
            ColocarNaCelula(celula);
            return;
        }

        VoltarParaMao();
    }

    // -------------------------------------------------------------
    // LÓGICA DE COLOCAÇÃO
    // -------------------------------------------------------------
    void ColocarNaCelula(RectTransform celula)
    {
        _rt.SetParent(celula, worldPositionStays: false);
        _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.pivot     = new Vector2(0.5f, 0.5f);
        _rt.anchoredPosition = Vector2.zero;
        _rt.localScale = Vector3.one;

        if (ajustarParaCellNaGrelha && GridLayout)
            _rt.sizeDelta = GridLayout.cellSize;

        _estavaNaGrelha = true;
    }

    void VoltarParaMao()
    {
        if (MaoRoot == null) { VoltarParaOrigem(); return; }

        _rt.SetParent(MaoRoot, worldPositionStays: false);
        _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.pivot     = new Vector2(0.5f, 0.5f);
        _rt.localRotation = Quaternion.identity;
        _rt.localScale    = Vector3.one;
        _rt.SetAsLastSibling();
        _rt.sizeDelta = _sizeOriginalNaMao;

        _estavaNaGrelha = false;
    }

    void VoltarParaOrigem()
    {
        if (_parentOriginal == null) { VoltarParaMao(); return; }

        _rt.SetParent(_parentOriginal, worldPositionStays: false);
        _rt.SetSiblingIndex(_siblingIndexOriginal);
        _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.pivot     = new Vector2(0.5f, 0.5f);
        _rt.localRotation = Quaternion.identity;
        _rt.localScale    = Vector3.one;

        if (ajustarParaCellNaGrelha && EstaEmGrelha(_rt) && GridLayout)
            _rt.sizeDelta = GridLayout.cellSize;
        else
            _rt.sizeDelta = _sizeOriginalNaMao;
    }

    // -------------------------------------------------------------
    // ROTAÇÃO
    // -------------------------------------------------------------
    System.Collections.IEnumerator RodarSuave(float delta)
    {
        _aRodar = true;
        float t = 0f;
        Quaternion ini = _rt.localRotation;
        Quaternion fim = Quaternion.Euler(0, 0, _rt.localEulerAngles.z + delta);

        while (t < tempoRotacao)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / tempoRotacao);
            _rt.localRotation = Quaternion.Slerp(ini, fim, k);
            yield return null;
        }
        _rt.localRotation = fim;
        _aRodar = false;
    }

    // -------------------------------------------------------------
    // RAYCAST / GRID
    // -------------------------------------------------------------
    RectTransform EncontrarCelulaDebaixoDoCursor(PointerEventData eventData)
    {
        if (Raycaster == null || GridRoot == null) return null;

        _hits.Clear();
        Raycaster.Raycast(eventData, _hits);

        RectTransform melhor = null;
        float melhorDist = float.MaxValue;

        for (int i = 0; i < _hits.Count; i++)
        {
            var rt = _hits[i].gameObject.GetComponent<RectTransform>();
            if (rt == null) continue;
            if (!TransformEhFilhoDe(rt, GridRoot)) continue;

            var candidato = SobeAteFilhoDiretoDe(rt, GridRoot);
            if (candidato == null) continue;

            Vector2 screen = eventData.position;
            Vector2 centerScreen = WorldToScreenCenter(candidato);
            float dist = (screen - centerScreen).sqrMagnitude;

            if (dist < melhorDist) { melhorDist = dist; melhor = candidato; }
        }
        return melhor;
    }

    bool CelulaEstaLivre(RectTransform celula) => celula.childCount == 0;

    // -------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------
    bool EstaEmGrelha(Transform t) => GridRoot && t && t.IsChildOf(GridRoot);

    static bool TransformEhFilhoDe(Transform t, Transform pai)
        => (t && pai) && (t == pai || t.IsChildOf(pai));

    static RectTransform SobeAteFilhoDiretoDe(Transform t, Transform paiDireto)
    {
        if (!t || !paiDireto) return null;
        var atual = t;
        while (atual && atual.parent != paiDireto) atual = atual.parent;
        return (atual && atual.parent == paiDireto) ? atual as RectTransform : null;
    }

    Vector2 WorldToScreenCenter(RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        var c = (corners[0] + corners[2]) * 0.5f;
        return RectTransformUtility.WorldToScreenPoint(null, c); // Overlay => cam null
    }

    Sprite TryGetImageSprite()
    {
        var img = GetComponent<Image>();
        return img ? img.sprite : null;
    }

    void TryAutoAssignReferences()
    {
        if (!Raycaster)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas) Raycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        if (!DragLayer)
        {
            var go = GameObject.Find("UI_DragLayer");
            if (go) DragLayer = go.transform as RectTransform;
            if (!DragLayer)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas) DragLayer = canvas.transform as RectTransform;
            }
        }

        if (!GridRoot)
        {
            var go = GameObject.Find("GRD_Tabuleiro");
            if (go) GridRoot = go.transform as RectTransform;

            if (!GridRoot)
            {
                var glgUp = GetComponentInParent<GridLayoutGroup>();
                if (glgUp) GridRoot = glgUp.transform as RectTransform;
            }
        }
        if (!GridLayout && GridRoot)
        {
            GridLayout = GridRoot.GetComponent<GridLayoutGroup>() ??
                         GridRoot.GetComponentInChildren<GridLayoutGroup>(true);
        }

        if (!MaoRoot)
        {
            var go = GameObject.Find("UI_BarraPecas");
            if (go) MaoRoot = go.transform as RectTransform;

            if (!MaoRoot)
            {
                var h = GetComponentInParent<HorizontalLayoutGroup>();
                var v = GetComponentInParent<VerticalLayoutGroup>();
                if (h) MaoRoot = h.transform as RectTransform;
                else if (v) MaoRoot = v.transform as RectTransform;
            }
        }
    }
}
