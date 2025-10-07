public static class PermissoesJogo
{
    public static bool PodeInteragir { get; private set; } = false;
    public static bool JaLibertou { get; private set; } = false;

    public static void HabilitarInteracao()
    {
        PodeInteragir = true;
        JaLibertou = true;
    }

    public static void BloquearInteracao()
    {
        PodeInteragir = false;
    }
}
