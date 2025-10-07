// Assets/Scripts/GestorGrelha.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GestorGrelha : MonoBehaviour
{
    // ===== LEGADO (usado pelo GeoZooSceneBuilder) =====
    [Header("⏮️ Compat (legado) — NÃO usar em código novo")]
    public RectTransform PaiTabuleiro;   // alias -> GridRoot
    public GameObject    PrefabCelula;   // não usado (grid é gerada via código)
    public RectTransform PaiBarraPecas;  // alias -> MaoRoot
    public GameObject    PrefabPeca;     // alias -> primeiro de PrefabsPeca

    // ===== ATUAL =====
    [Header("Tabuleiro (Grid)")]
    public RectTransform GridRoot;           // GRD_Tabuleiro
    public GridLayoutGroup GridLayout;       // (se não ligar, tenta obter do GridRoot)
    [Min(1)] public int Colunas = 6;
    [Min(1)] public int Linhas  = 6;
    public bool RecriarAoIniciar = true;

    [Header("Célula (opcional, só visual)")]
    public bool CriarImagemNasCelulas = true;
    public Sprite SpriteCelula;
    [Range(0f, 1f)] public float AlphaCelula = 0.08f;

    [Header("Mão do Jogador")]
    public RectTransform MaoRoot;            // UI_BarraPecas
    public List<GameObject> PrefabsPeca = new List<GameObject>();
    public List<int> PecasIniciaisNaMao = new List<int>() { 0, 1, 2, 3 };

    [Header("Spawn na Mão — Controlo")]
    public bool SpawnPecasNaMaoAoIniciar = true;
    [Tooltip("Se já existirem peças na mão (na cena), não cria novas ao iniciar.")]
    public bool IgnorarSpawnSeJaExistiremPecasNaMao = true;

#if UNITY_EDITOR
    void OnValidate()
    {
        // sincroniza aliases legado <-> campos atuais
        if (PaiTabuleiro != null && GridRoot == null) GridRoot = PaiTabuleiro;
        if (GridRoot != null && PaiTabuleiro == null) PaiTabuleiro = GridRoot;

        if (PaiBarraPecas != null && MaoRoot == null) MaoRoot = PaiBarraPecas;
        if (MaoRoot != null && PaiBarraPecas == null) PaiBarraPecas = MaoRoot;

        if (PrefabPeca != null)
        {
            if (PrefabsPeca == null) PrefabsPeca = new List<GameObject>();
            if (PrefabsPeca.Count == 0 || PrefabsPeca[0] == null)
            {
                if (PrefabsPeca.Count == 0) PrefabsPeca.Add(PrefabPeca);
                else PrefabsPeca[0] = PrefabPeca;
            }
        }

        if (!Application.isPlaying && !GridLayout && GridRoot)
            GridLayout = GridRoot.GetComponent<GridLayoutGroup>();
    }
#endif

    void Awake()
    {
        // sincroniza ao arrancar
        if (!GridRoot && PaiTabuleiro) GridRoot = PaiTabuleiro;
        if (!MaoRoot && PaiBarraPecas) MaoRoot = PaiBarraPecas;
        if ((PrefabsPeca == null || PrefabsPeca.Count == 0) && PrefabPeca)
            PrefabsPeca = new List<GameObject> { PrefabPeca };

        if (!GridLayout && GridRoot)
            GridLayout = GridRoot.GetComponent<GridLayoutGroup>();

        if (RecriarAoIniciar && GridRoot)
            CriarOuRecriarGrelha();

        // ---- controlo do auto-spawn na mão ----
        if (SpawnPecasNaMaoAoIniciar &&
            MaoRoot && PrefabsPeca != null && PrefabsPeca.Count > 0 &&
            PecasIniciaisNaMao != null && PecasIniciaisNaMao.Count > 0)
        {
            if (IgnorarSpawnSeJaExistiremPecasNaMao && ContarPecasNaMao() > 0)
            {
                Debug.Log("[GestorGrelha] Já existem peças na mão — não vou auto-spawnar para evitar duplicados.");
            }
            else
            {
                CriarPecasIniciaisNaMao();
            }
        }
    }

    int ContarPecasNaMao()
    {
        if (!MaoRoot) return 0;
        int n = 0;
        for (int i = 0; i < MaoRoot.childCount; i++)
        {
            var child = MaoRoot.GetChild(i);
            if (child.GetComponent<Peca>() != null) n++;
        }
        return n;
    }

    [ContextMenu("Recriar Grelha Agora")]
    public void CriarOuRecriarGrelha()
    {
        if (!GridRoot)
        {
            Debug.LogWarning("[GestorGrelha] GridRoot não está ligado.");
            return;
        }

        var toDestroy = new List<GameObject>();
        for (int i = 0; i < GridRoot.childCount; i++)
            toDestroy.Add(GridRoot.GetChild(i).gameObject);
        for (int i = 0; i < toDestroy.Count; i++)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(toDestroy[i]);
            else Destroy(toDestroy[i]);
#else
            Destroy(toDestroy[i]);
#endif
        }

        if (GridLayout)
        {
            if (GridLayout.cellSize == Vector2.zero)
            {
                var rt = (RectTransform)GridLayout.transform;
                var w = rt.rect.width  - GridLayout.padding.left - GridLayout.padding.right - GridLayout.spacing.x * (Colunas - 1);
                var h = rt.rect.height - GridLayout.padding.top  - GridLayout.padding.bottom - GridLayout.spacing.y * (Linhas  - 1);
                var cellW = Mathf.Floor(w / Colunas);
                var cellH = Mathf.Floor(h / Linhas);
                GridLayout.cellSize = new Vector2(cellW, cellH);
            }
            GridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            GridLayout.constraintCount = Colunas;
        }

        // cria células
        for (int y = 0; y < Linhas; y++)
        {
            for (int x = 0; x < Colunas; x++)
            {
                var go = new GameObject($"CEL_{x}_{y}", typeof(RectTransform));
                go.transform.SetParent(GridRoot, false);
                var rt = go.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;

                var cel = go.AddComponent<CelulaTabuleiro>();
                cel.X = x; cel.Y = y;

                if (CriarImagemNasCelulas)
                {
                    var img = go.AddComponent<Image>();
                    img.raycastTarget = true; // importante para o raycast apanhar a célula
                    if (SpriteCelula)
                    {
                        img.sprite = SpriteCelula;
                        img.type = Image.Type.Sliced;
                        img.color = new Color(1f, 1f, 1f, AlphaCelula);
                    }
                    else
                    {
                        img.color = new Color(1f, 1f, 1f, AlphaCelula);
                    }
                }
            }
        }

        Debug.Log($"[GestorGrelha] Grelha criada: {Colunas}x{Linhas} = {Colunas * Linhas} células.");
    }

    [ContextMenu("Criar Peças Iniciais na Mão")]
    public void CriarPecasIniciaisNaMao()
    {
        if (!MaoRoot) { Debug.LogWarning("[GestorGrelha] MaoRoot não está ligado."); return; }
        if (PrefabsPeca == null || PrefabsPeca.Count == 0) { Debug.LogWarning("[GestorGrelha] Lista de PrefabsPeca vazia."); return; }

        for (int i = 0; i < PecasIniciaisNaMao.Count; i++)
        {
            int idx = PecasIniciaisNaMao[i];
            CriarPecaNaMao(idx);
        }
        Debug.Log($"[GestorGrelha] Criadas {PecasIniciaisNaMao.Count} peças na mão.");
    }

    public GameObject CriarPecaNaMao(int prefabIndex)
    {
        if (!MaoRoot) { Debug.LogWarning("[GestorGrelha] MaoRoot não ligado."); return null; }
        if (prefabIndex < 0 || prefabIndex >= PrefabsPeca.Count) { Debug.LogWarning("[GestorGrelha] Índice de prefab inválido."); return null; }
        var prefab = PrefabsPeca[prefabIndex];
        if (!prefab) { Debug.LogWarning("[GestorGrelha] Prefab nulo."); return null; }

        var go = Instantiate(prefab, MaoRoot, false);
        var rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.SetAsLastSibling();

        var img = go.GetComponent<Image>();
        if (img) { img.enabled = true; img.raycastTarget = true; }
        var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
        cg.alpha = 1f; cg.blocksRaycasts = true;

        var flip = go.GetComponent<PecaFlip>();
        if (flip && flip.SpriteFrente) flip.MostrarFrente();

        return go;
    }

    public GameObject CriarPecaNaCelula(int prefabIndex, int x, int y)
    {
        if (!GridRoot) { Debug.LogWarning("[GestorGrelha] GridRoot não ligado."); return null; }
        if (prefabIndex < 0 || prefabIndex >= PrefabsPeca.Count) { Debug.LogWarning("[GestorGrelha] Índice de prefab inválido."); return null; }

        var cel = ObterCelula(x, y);
        if (!cel)
        {
            Debug.LogWarning($"[GestorGrelha] Célula ({x},{y}) não encontrada.");
            return null;
        }
        if (cel.childCount > 0)
        {
            Debug.LogWarning($"[GestorGrelha] Célula ({x},{y}) já ocupada.");
            return null;
        }

        var go = Instantiate(PrefabsPeca[prefabIndex], cel, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        if (GridLayout) rt.sizeDelta = GridLayout.cellSize;

        var flip = go.GetComponent<PecaFlip>();
        if (flip && flip.SpriteFrente) flip.MostrarFrente();

        return go;
    }

    public RectTransform ObterCelula(int x, int y)
    {
        for (int i = 0; i < GridRoot.childCount; i++)
        {
            var child = GridRoot.GetChild(i);
            var cel = child.GetComponent<CelulaTabuleiro>();
            if (cel != null && cel.X == x && cel.Y == y)
                return child as RectTransform;
        }
        return null;
    }
}
