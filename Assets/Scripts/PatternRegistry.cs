// Assets/Scripts/PatternRegistry.cs
using System.Collections.Generic;
using UnityEngine;

public class PatternRegistry : MonoBehaviour
{
    public static PatternRegistry Instancia;

    [System.Serializable]
    public class Entrada
    {
        public Sprite CartaFrente;
        public AnimalPattern Pattern;
    }

    public List<Entrada> Entradas = new();

    void Awake() => Instancia = this;

    public AnimalPattern ObterPorCarta(Sprite cartaFrente)
    {
        if (cartaFrente == null) return null;
        for (int i = 0; i < Entradas.Count; i++)
            if (Entradas[i].CartaFrente == cartaFrente)
                return Entradas[i].Pattern;
        return null;
    }
}
