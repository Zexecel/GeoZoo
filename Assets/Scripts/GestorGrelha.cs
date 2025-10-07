using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Constrói a grelha (4x3 por default), cria células e gera peças iniciais na barra inferior.
/// Também faz raycast UI para descobrir a célula por baixo do cursor/toque.
/// Coloca este script num GameObject (ex.: GO_Sistemas).
/// </summary>
public class GestorGrelha : MonoBehaviour
{
    [Header("Grelha")]
    public Transform PaiTabuleiro;   // GRD_Tabuleiro (tem GridLayoutGroup)
    public GameObject PrefabCelula;  // PF_Celula
    [Range(1, 10)] public int Colunas = 4;
    [Range(1, 10)] public int Linhas = 3;

    [Header("Peças")]
    public Transform PaiBarraPecas;  // UI_BarraPecas
    public GameObject PrefabPeca;    // PF_Peca
    public int PecasIniciais = 4;

    private CelulaTabuleiro[,] _celulas;
    private Canvas _canvas;
    private GraphicRaycaster _raycaster;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _raycaster = _canvas.GetComponent<GraphicRaycaster>();
        ConstruirGrelha();
    }

    void Start()
    {
        CriarPecasIniciais();
    }

    /// <summary>
    /// Cria a matriz de células com base em Colunas e Linhas.
    /// </summary>
    public void ConstruirGrelha()
    {
        // Limpa filhos anteriores (se estivermos a reconstruir)
        for (int i = PaiTabuleiro.childCount - 1; i >= 0; i--)
            Object.Destroy(PaiTabuleiro.GetChild(i).gameObject);

        _celulas = new CelulaTabuleiro[Colunas, Linhas];

        for (int y = 0; y < Linhas; y++)
        for (int x = 0; x < Colunas; x++)
        {
            var go = Object.Instantiate(PrefabCelula, PaiTabuleiro);
            var cel = go.GetComponent<CelulaTabuleiro>();
            cel.Coordenada = new Vector2Int(x, y);
            _celulas[x, y] = cel;
        }
    }

    /// <summary>
    /// Cria um conjunto simples de peças no tabuleiro inferior.
    /// </summary>
    void CriarPecasIniciais()
    {
        for (int i = 0; i < PecasIniciais; i++)
            Object.Instantiate(PrefabPeca, PaiBarraPecas);
    }

    /// <summary>
    /// Faz um raycast UI e devolve a célula do tabuleiro por baixo do ponto de ecrã.
    /// Devolve null se não houver célula válida.
    /// </summary>
   public CelulaTabuleiro ObterCelulaEmEcrã(Vector2 posEcrã)
{
    var ped = new PointerEventData(EventSystem.current) { position = posEcrã };
    var resultados = new List<RaycastResult>();
    _raycaster.Raycast(ped, resultados);

    foreach (var r in resultados)
    {
        var cel = r.gameObject.GetComponentInParent<CelulaTabuleiro>();
        if (cel != null) return cel;
    }
    return null;
}


}
