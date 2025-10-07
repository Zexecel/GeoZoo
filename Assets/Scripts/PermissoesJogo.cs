// Assets/Scripts/PermissoesJogo.cs
public static class PermissoesJogo
{
    /// Enquanto for false, não pode arrastar nem fazer flip.
    public static bool PodeInteragir { get; private set; } = false;

    /// Garante que só “liberta” uma vez (primeira carta comprada).
    public static bool JaLibertou { get; private set; } = false;

    public static void HabilitarInteracao()
    {
        PodeInteragir = true;
        JaLibertou = true;
    }

    public static void BloquearInteracao()
    {
        PodeInteragir = false;
        // Se precisares, cria um Reset para JaLibertou noutro sítio.
    }
}
