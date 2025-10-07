// Assets/Scripts/DesafioRuntime.cs
using UnityEngine;

/// <summary>
/// Mantém o ID do animal atualmente pedido ao jogador.
/// </summary>
public class DesafioRuntime : MonoBehaviour
{
    [SerializeField] private string animalAlvoId = "LEAO"; // exemplo default

    // Propriedade esperada pelo GridValidator
    public string AnimalAlvoId => animalAlvoId;

    // Podes usar isto quando trocares de carta
    public void DefinirAnimalAlvo(string novoId)
    {
        animalAlvoId = novoId;
    }
}
