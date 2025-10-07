// Assets/Scripts/GridValidator.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridValidator : MonoBehaviour
{
    public static GridValidator Instancia;

    [Header("Referências")]
    public RectTransform GridRoot;         // GRD_Tabuleiro
    public GridLayoutGroup GridLayout;     // opcional
    public int Colunas = 6;
    public int Linhas  = 6;

    [Header("Feedback")]
    public bool LogNoConsole = true;

    AnimalPattern _ativo;

    void Awake()
    {
        Instancia = this;
        if (!GridLayout && GridRoot) GridLayout = GridRoot.GetComponent<GridLayoutGroup>();
    }

    public void DefinirPadraoAtivo(AnimalPattern padrao)
    {
        _ativo = padrao;
        if (LogNoConsole)
            Debug.Log(_ativo ? $"[GridValidator] Padrão ativo: {_ativo.Id}" : "[GridValidator] Padrão ativo: (nenhum)");
    }

    public void Validar()
    {
        if (_ativo == null || _ativo.Celulas == null || _ativo.Celulas.Count == 0)
        {
            if (LogNoConsole) Debug.Log("[GridValidator] Sem padrão ativo.");
            return;
        }

        var mapa = ConstruirMapa();

        for (int y0 = 0; y0 <= Linhas - _ativo.Altura; y0++)
        {
            for (int x0 = 0; x0 <= Colunas - _ativo.Largura; x0++)
            {
                if (MatchEm(mapa, x0, y0))
                {
                    if (LogNoConsole) Debug.Log($"[GridValidator] Match! Anchor=({x0},{y0}) -> +20s e +1 ZOO");
                    ControladorJogo.Instancia?.AdicionarTempo(20);
                    if (ControladorJogo.Instancia != null)
                        ControladorJogo.Instancia.AtualizarZoo(ControladorJogo.Instancia.ContadorZoo + 1);
                    return;
                }
            }
        }

        if (LogNoConsole) Debug.Log("[GridValidator] Ainda não corresponde ao padrão.");
    }

    struct CelInfo { public TileID tile; }

    Dictionary<(int x, int y), CelInfo> ConstruirMapa()
    {
        var map = new Dictionary<(int x, int y), CelInfo>(Colunas * Linhas);

        bool usouCelulaComp = false;
        for (int i = 0; i < GridRoot.childCount; i++)
        {
            var cel = GridRoot.GetChild(i) as RectTransform;
            if (cel == null) continue;

            var ct = cel.GetComponent<CelulaTabuleiro>();
            if (ct != null)
            {
                usouCelulaComp = true;
                TileID tile = null;
                if (cel.childCount > 0) tile = cel.GetChild(0).GetComponent<TileID>();
                map[(ct.X, ct.Y)] = new CelInfo { tile = tile };
            }
        }

        if (!usouCelulaComp)
        {
            int idx = 0;
            for (int y = 0; y < Linhas; y++)
            {
                for (int x = 0; x < Colunas; x++)
                {
                    if (idx >= GridRoot.childCount) break;
                    var cel = GridRoot.GetChild(idx++) as RectTransform;
                    TileID tile = null;
                    if (cel != null && cel.childCount > 0)
                        tile = cel.GetChild(0).GetComponent<TileID>();
                    map[(x, y)] = new CelInfo { tile = tile };
                }
            }
        }

        return map;
    }

    bool MatchEm(Dictionary<(int x, int y), CelInfo> map, int x0, int y0)
    {
        for (int i = 0; i < _ativo.Celulas.Count; i++)
        {
            var req = _ativo.Celulas[i];
            int gx = x0 + req.X;
            int gy = y0 + req.Y;
            if (!map.TryGetValue((gx, gy), out var cel)) return false;
            if (cel.tile == null) return false;

            if (cel.tile.Tipo != req.Tipo) return false;
            int rotReq = _ativo.RotacaoGraus(req.RotacaoSteps);
            if (cel.tile.Rotacao90 != rotReq) return false;
        }
        return true;
    }
}
