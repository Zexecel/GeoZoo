// Assets/Scripts/PecaFlip.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteAlways] // também actualiza no Editor
[DisallowMultipleComponent]
public class PecaFlip : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite SpriteFrente;
    public Sprite SpriteVerso;

    [Header("Estado Inicial")]
    public bool iniciarNaFrente = true;  // mão = true; carta saída do deck substitui isto

    [Header("Animação")]
    [Range(0.05f, 0.6f)] public float TempoFlip = 0.18f;
    public AnimationCurve Curva = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Image _img;
    bool _viradaFrente;

    void OnEnable()
    {
        if (_img == null) _img = GetComponent<Image>();
        if (_img != null) _img.preserveAspect = true;

        // No Editor e no Play, mostra já o lado pedido
        if (!Application.isPlaying)
        {
            AplicarEstadoInicialEditor();
        }
        else
        {
            if (iniciarNaFrente) MostrarFrente(); else MostrarVerso();
        }
    }

    void AplicarEstadoInicialEditor()
    {
        if (_img == null) return;
        // Não mexe se algum dev colocou manualmente um Source Image;
        // mas se estiver em None, mostramos o inicial para pré-visualizar.
        if (_img.sprite == null)
        {
            _img.sprite = iniciarNaFrente ? SpriteFrente : SpriteVerso;
        }
    }

    public void Configurar(Sprite frente, Sprite verso)
    {
        SpriteFrente = frente;
        SpriteVerso  = verso;
        // não força visual aqui — quem chama decide chamar MostrarVerso/Frente
    }

    public void MostrarVerso()
    {
        if (_img) _img.sprite = SpriteVerso;
        _viradaFrente = false;
    }

    public void MostrarFrente()
    {
        if (_img) _img.sprite = SpriteFrente;
        _viradaFrente = true;
    }

    public IEnumerator FlipParaFrente()
    {
        if (_viradaFrente) yield break;
        yield return FlipInterno(true);
    }
    public IEnumerator FlipParaVerso()
    {
        if (!_viradaFrente) yield break;
        yield return FlipInterno(false);
    }

    IEnumerator FlipInterno(bool mostrarFrente)
    {
        if (_img == null) yield break;
        var rt = (RectTransform)transform;

        float half = Mathf.Max(0.0001f, TempoFlip * 0.5f);
        float t = 0f;

        // 1) fechar (scaleX 1->0)
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float e = Curva.Evaluate(Mathf.Clamp01(t / half));
            float sx = Mathf.LerpUnclamped(1f, 0f, e);
            rt.localScale = new Vector3(Mathf.Max(0.0001f, sx), rt.localScale.y, 1f);
            yield return null;
        }

        if (mostrarFrente) MostrarFrente(); else MostrarVerso();

        // 2) abrir (scaleX 0->1)
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float e = Curva.Evaluate(Mathf.Clamp01(t / half));
            float sx = Mathf.LerpUnclamped(0f, 1f, e);
            rt.localScale = new Vector3(Mathf.Max(0.0001f, sx), rt.localScale.y, 1f);
            yield return null;
        }
        rt.localScale = new Vector3(1f, rt.localScale.y, 1f);
        _viradaFrente = mostrarFrente;
    }
}
