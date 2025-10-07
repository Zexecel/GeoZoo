// Assets/Scripts/PecaFlip.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
public class PecaFlip : MonoBehaviour, IPointerClickHandler
{
    [Header("Estado")]
    public bool viradaFrente = true;

    [Header("Sprites (opcional)")]
    public Sprite spriteFrente;
    public Sprite spriteVerso;

    [Header("Cores fallback")]
    public Color corFrente = new Color(0.20f, 0.56f, 1f);
    public Color corVerso  = new Color(0.15f, 0.15f, 0.15f);

    [Header("Animação")]
    [Range(0.05f, 0.6f)] public float tempoFlip = 0.18f;
    public AnimationCurve curva = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Image _img;
    RectTransform _rt;
    bool _aVirar;

    void Awake()
    {
        _img = GetComponent<Image>();
        _rt  = GetComponent<RectTransform>();
        AplicarVisual(viradaFrente);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!PermissoesJogo.PodeInteragir) return;
        if (eventData.button == PointerEventData.InputButton.Right)
            Virar();
    }

    /// <summary>Inverte frente/verso com animação.</summary>
    public void Virar()
    {
        if (_aVirar) return;
        StartCoroutine(FazerFlipAnimado(!viradaFrente));
    }

    /// <summary>
    /// Alias para compatibilidade com scripts que chamam Toggle().
    /// Mantém o teu comportamento original sem teres de tocar no Peca.cs.
    /// </summary>
    public void Toggle() => Virar();

    public void SetFrente(bool frente, bool animado = false)
    {
        if (_aVirar)
        {
            viradaFrente = frente;
            return;
        }
        if (animado) StartCoroutine(FazerFlipAnimado(frente));
        else { viradaFrente = frente; AplicarVisual(viradaFrente); }
    }

    // === API usada pelo DeckController (se precisares) ===
    public void ConfigurarFaces(Sprite frente, Sprite verso)
    {
        spriteFrente = frente;
        spriteVerso  = verso;
        AplicarVisual(viradaFrente);
    }

    public void MostrarVerso()  => SetFrente(false, false);
    public void MostrarFrente() => SetFrente(true,  false);

    public IEnumerator FlipParaFrente(float dur = 0.25f)
    {
        if (viradaFrente) yield break;
        yield return FazerFlipAnimadoExt(true, dur);
    }

    public IEnumerator FlipParaVerso(float dur = 0.25f)
    {
        if (!viradaFrente) yield break;
        yield return FazerFlipAnimadoExt(false, dur);
    }
    // === fim da API ===

    IEnumerator FazerFlipAnimado(bool novoEstadoFrente)
    {
        yield return FazerFlipAnimadoExt(novoEstadoFrente, tempoFlip);
    }

    IEnumerator FazerFlipAnimadoExt(bool novoEstadoFrente, float dur)
    {
        _aVirar = true;
        float half = Mathf.Max(0.01f, dur * 0.5f);

        // 1) 1 -> 0
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            float e = curva.Evaluate(t / half);
            _rt.localScale = new Vector3(Mathf.Lerp(1f, 0f, e), 1f, 1f);
            yield return null;
        }
        _rt.localScale = new Vector3(0f, 1f, 1f);

        viradaFrente = novoEstadoFrente;
        AplicarVisual(viradaFrente);

        // 2) 0 -> 1
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            float e = curva.Evaluate(t / half);
            _rt.localScale = new Vector3(Mathf.Lerp(0f, 1f, e), 1f, 1f);
            yield return null;
        }
        _rt.localScale = Vector3.one;

        _aVirar = false;
    }

    void AplicarVisual(bool frente)
    {
        if (_img == null) return;

        if (spriteFrente != null || spriteVerso != null)
        {
            _img.sprite = frente ? spriteFrente : spriteVerso;
            _img.color  = Color.white;
        }
        else
        {
            _img.sprite = null;
            _img.color  = frente ? corFrente : corVerso;
        }
    }
}
