using UnityEngine;

public class DesafioRuntime : MonoBehaviour
{
    public static DesafioRuntime Instancia { get; private set; }
    public DesafioCarta DesafioAtual { get; private set; }

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void DefinirDesafio(DesafioCarta desafio)
    {
        DesafioAtual = desafio;
    }
}
