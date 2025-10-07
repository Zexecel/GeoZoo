// Assets/Scripts/AnimalPattern.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GeoZoo/Animal Pattern")]
public class AnimalPattern : ScriptableObject
{
    public string Id;
    public Sprite CartaFrente;   // sprite da carta de deck que ativa este padrão
    public int Largura = 2;
    public int Altura = 2;

    [Tooltip("Células relativas do padrão (0,0 topo-esquerda).")]
    public List<CelulaRequerida> Celulas = new List<CelulaRequerida>();

    [Serializable]
    public class CelulaRequerida
    {
        public int X;
        public int Y;                 // 0 = topo
        public TileTipo Tipo;
        [Range(0,3)] public int RotacaoSteps; // 0=0°,1=90°,2=180°,3=270°
    }

    public int RotacaoGraus(int steps) => ((steps % 4) + 4) % 4 * 90;
}
