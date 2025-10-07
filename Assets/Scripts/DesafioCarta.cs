// Assets/Scripts/DesafioCarta.cs
using UnityEngine;

[CreateAssetMenu(fileName = "DesafioCarta", menuName = "GeoZoo/Desafio Carta", order = 10)]
public class DesafioCarta : ScriptableObject
{
    [Header("Visual")]
    public Sprite spriteFrente;

    [Header("Máscara da forma que o jogador deve montar")]
    [Tooltip("Largura do padrão (nº de colunas)")]
    public int colunas = 2;
    [Tooltip("Altura do padrão (nº de linhas)")]
    public int linhas  = 2;

    [Tooltip("Uma string por linha, '1' = célula exigida, '0' = vazia. Exatamente 'colunas' chars por linha.")]
    public string[] padraoLinhas;

    [Header("Regras")]
    [Tooltip("Aceitar rotações 90/180/270 como válidas")]
    public bool aceitarRot90 = false;

    [Tooltip("Se true, não pode haver tiles fora da forma exigida (match exato). Se false, apenas exige as '1' (pode haver extras).")]
    public bool matchExato = true;

    public bool Requer(int x, int y)
    {
        if (padraoLinhas == null || y < 0 || y >= linhas) return false;
        var linha = padraoLinhas[y];
        if (string.IsNullOrEmpty(linha) || x < 0 || x >= colunas) return false;
        return linha[x] == '1';
    }
}
