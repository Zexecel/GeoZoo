// Assets/Scripts/ControladorJogo.cs
using UnityEngine;
using TMPro;

public class ControladorJogo : MonoBehaviour
{
    public static ControladorJogo Instancia;

    [Header("UI")]
    public TMP_Text TxtTempo;
    public TMP_Text TxtZoo;

    [Header("Configuração")]
    public int TempoInicialSeg = 150;
    public int ContadorZoo = 0;

    float _tempoRestante;
    bool _timerAtivo = false;

    void Awake()
    {
        Instancia = this;
        _tempoRestante = TempoInicialSeg;
        AtualizarTempoUI();
        AtualizarZooUI();
        PermissoesJogo.BloquearInteracao();
    }

    void Update()
    {
        if (!_timerAtivo) return;

        _tempoRestante -= Time.deltaTime;
        if (_tempoRestante < 0f) _tempoRestante = 0f;
        AtualizarTempoUI();

        if (_tempoRestante <= 0f)
        {
            _timerAtivo = false;
            // TODO: fim de jogo / ecrã de resultados
        }
    }

    public void IniciarTimer()
    {
        if (_timerAtivo) return;
        _timerAtivo = true;
    }

    public void PausarTimer() => _timerAtivo = false;

    public void AtualizarZoo(int novoValor)
    {
        ContadorZoo = novoValor;
        AtualizarZooUI();
    }

    public void AdicionarTempo(int segundos)
    {
        _tempoRestante += Mathf.Max(0, segundos);
        AtualizarTempoUI();
    }

    void AtualizarTempoUI()
    {
        if (TxtTempo == null) return;
        int minutos = Mathf.FloorToInt(_tempoRestante / 60f);
        int segundos = Mathf.FloorToInt(_tempoRestante % 60f);
        TxtTempo.text = $"TIME: {minutos}:{segundos:00}";
    }

    void AtualizarZooUI()
    {
        if (TxtZoo == null) return;
        TxtZoo.text = $"ZOO: {ContadorZoo}";
    }
}
