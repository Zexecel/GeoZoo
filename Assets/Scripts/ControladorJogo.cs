using UnityEngine;
using TMPro; // importante — muda de UnityEngine.UI para TMPro

/// <summary>
/// Controla o tempo de jogo e o texto do HUD.
/// Coloca um (apenas um) na cena e liga os TextMeshProUGUI no Inspector.
/// </summary>
public class ControladorJogo : MonoBehaviour
{
    public static ControladorJogo Instancia { get; private set; }

    [Header("HUD (TextMeshPro)")]
    public TextMeshProUGUI TXT_Tempo;
    public TextMeshProUGUI TXT_Zoo;

    // Aliases para compatibilidade com GeoZooSceneBuilder
    public TextMeshProUGUI TxtTempo
    {
        get => TXT_Tempo;
        set => TXT_Tempo = value;
    }

    public TextMeshProUGUI TxtZoo
    {
        get => TXT_Zoo;
        set => TXT_Zoo = value;
    }

    [Header("Tempo")]
    public int SegundosInicio = 150; // 2:30
    float _tempoRestante;
    bool _timerAtivo;

    int _pontosZoo;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        _tempoRestante = SegundosInicio;
        AtualizarTextoTempo();
        AtualizarTextoZoo();
    }

    void Update()
    {
        if (_timerAtivo)
        {
            _tempoRestante -= Time.deltaTime;
            if (_tempoRestante < 0f)
            {
                _tempoRestante = 0f;
                _timerAtivo = false;
                // TODO: fim de jogo
            }
            AtualizarTextoTempo();
        }
    }

    public void IniciarTimer()
    {
        _timerAtivo = true;
        Debug.Log("TIMER: iniciou");
    }

    public void PararTimer() => _timerAtivo = false;

    public void AddTempo(float segundos)
    {
        _tempoRestante += segundos;
        AtualizarTextoTempo();
    }

    public void AddZoo(int pontos)
    {
        _pontosZoo += pontos;
        AtualizarTextoZoo();
    }

    void AtualizarTextoTempo()
    {
        if (TXT_Tempo == null) return;
        int t = Mathf.Max(0, Mathf.FloorToInt(_tempoRestante));
        int m = t / 60;
        int s = t % 60;
        TXT_Tempo.text = $"TIME: {m}:{s:00}";
    }

    void AtualizarTextoZoo()
    {
        if (TXT_Zoo == null) return;
        TXT_Zoo.text = $"ZOO: {_pontosZoo}";
    }
}
