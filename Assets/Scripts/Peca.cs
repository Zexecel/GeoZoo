using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Peça arrastável:
/// - Não herda escala/anchors do layout ao começar drag (evita offset).
/// - É reparentada para o UI_DragLayer (Canvas com Sorting Order alto).
/// - Durante o drag segue o rato no Canvas do DragLayer (sem drift).
/// - No fim, tenta encaixar; se não encaixar, volta à barra no índice original.
/// 
/// Requisitos na cena:
/// - UI_DragLayer: GameObject irmão (não filho da barra), com Canvas (Override Sorting=ON; Sorting Order alto) e GraphicRaycaster.
/// - Atribuir no Inspector da peça:
///     DragLayer  → RectTransform do UI_DragLayer
///     DragCanvas → Canvas do UI_DragLayer (opcional; se null, é obtido por GetComponentInParent)
/// </summary>
[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image))]
public class Peca : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
{
    [Header("Referências")]
    public RectTransform DragLayer;   // UI_DragLayer (irmão do GRD_Tabuleiro / UI_BarraPecas)
    public Canvas DragCanvas;         // Canvas do UI_DragLayer (se ficar a null é apanhado no BeginDrag)
    public RectTransform GridRoot;    // GRD_Tabuleiro (opcional; para validações tuas)
    public GridValidator GridValidator; // (opcional) se usares validação automática

    [Header("Opções")]
    public bool VoltarParaBarraSeFalhar = true;

    // estado interno
    RectTransform _rt;
    CanvasGroup _cg;

    Transform _parentOriginal;
    int _siblingIndexOriginal = -1;

    bool _aArrastar;
    static Peca _selecionada; // se quiseres usar single-selection
    Vector2 _dragLocalPosCache;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    // ---------- Pointer ----------
    public void OnPointerDown(PointerEventData eventData)
    {
        _selecionada = this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Botão direito → flip frente/verso se tiveres esse sistema noutro script (ex.: PecaFlip)
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var flip = GetComponent<PecaFlip>();
            if (flip != null) flip.Toggle();
        }
    }

    // ---------- Drag ----------
    public void OnBeginDrag(PointerEventData eventData)
    {
        _aArrastar = true;

        // Canvas do DragLayer
        if (DragCanvas == null && DragLayer != null)
            DragCanvas = DragLayer.GetComponentInParent<Canvas>();

        if (DragLayer == null || DragCanvas == null)
        {
            Debug.LogWarning("[Peca] DragLayer/Canvas não definidos.");
            _aArrastar = false;
            return;
        }

        // guardar parent + índice originais (para voltar se falhar)
        _parentOriginal = transform.parent;
        _siblingIndexOriginal = transform.GetSiblingIndex();

        // pôr o DragLayer por cima de tudo (no mesmo Canvas, como irmão)
        DragLayer.SetAsLastSibling();

        // Reparentar para o DragLayer SEM herdar transform (evita offset e escala do layout)
        transform.SetParent(DragLayer, false);

        // leve transparência durante drag (opcional)
        _cg.blocksRaycasts = false;
        _cg.alpha = 0.9f;

        // posicionar imediatamente debaixo do rato
        AtualizarPosicaoPeloRato(eventData);
        _rt.SetAsLastSibling(); // esta peça acima das outras no DragLayer
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_aArrastar) return;
        AtualizarPosicaoPeloRato(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_aArrastar) return;
        _aArrastar = false;

        _cg.blocksRaycasts = true;
        _cg.alpha = 1f;

        // Tenta detetar drop válido via raycast UI
        var alvoValido = TentarColocarNaGrelhaOuOutroAlvo(eventData);

        if (!alvoValido && VoltarParaBarraSeFalhar && _parentOriginal != null)
        {
            // voltar ao parent original e restabelecer ordem
            transform.SetParent(_parentOriginal, false);
            if (_siblingIndexOriginal >= 0)
                transform.SetSiblingIndex(_siblingIndexOriginal);
        }
    }

    // ---------- Helpers ----------
    void AtualizarPosicaoPeloRato(PointerEventData eventData)
    {
        // Para Screen Space Overlay → worldCamera = null
        var cam = DragCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : DragCanvas.worldCamera;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)DragCanvas.transform, eventData.position, cam, out localPoint);

        _rt.anchoredPosition = localPoint; // sem drift/offset
    }

    /// <summary>
    /// Aqui decides a lógica de "encaixe": grelha/célula ou outro alvo.
    /// Se não encaixar, retorna false para voltar à barra (se a flag estiver ativa).
    /// </summary>
    bool TentarColocarNaGrelhaOuOutroAlvo(PointerEventData eventData)
    {
        // Exemplo básico: se tiveres um GridValidator, usa-o.
        if (GridValidator != null)
        {
            if (GridValidator.TentarEncaixar(this, _rt))
            {
                return true; // ficou na grelha
            }
        }

        // Sem validação → considera falha
        return false;
    }
}
