using UnityEngine;
using UnityEngine.UI;

public class CelulaTabuleiro : MonoBehaviour
{
    // Usado pelo GestorGrelha para marcar posição na grelha
    public Vector2Int Coordenada { get; set; }

    // Ocupante atual desta célula
    private Peca ocupante;
    private Image imagem;

    // Compatibilidade com código antigo
    public bool EstaVazia => ocupante == null;

    void Awake()
    {
        imagem = GetComponent<Image>();
    }

    /// Retorna true se esta célula já tiver uma peça.
    public bool TemOcupante()
    {
        return ocupante != null;
    }

    /// Define a peça que ocupa esta célula (ou null para libertar).
    public void DefinirOcupante(Peca novaPeca)
    {
        ocupante = novaPeca;
    }

    /// Liberta esta célula.
    public void LimparOcupante()
    {
        ocupante = null;
    }

    /// Visual opcional: realçar a célula.
    public void Realcar(bool ativo)
    {
        if (imagem != null)
            imagem.color = ativo ? new Color(1f, 1f, 0.9f, 1f) : Color.white;
    }
}
