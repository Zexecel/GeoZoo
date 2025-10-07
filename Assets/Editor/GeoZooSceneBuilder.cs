#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class GeoZooSceneBuilder
{
    [MenuItem("Tools/GeoZoo/Build Base Scene")]
    public static void BuildBaseScene()
    {
        // Canvas
        var canvasGO = new GameObject("CV_Main", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // EventSystem
        var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // GRD_Tabuleiro
        var grd = new GameObject("GRD_Tabuleiro", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
        grd.transform.SetParent(canvasGO.transform, false);
        var grdRT = grd.GetComponent<RectTransform>();
        grdRT.anchorMin = new Vector2(0.5f, 0.5f);
        grdRT.anchorMax = new Vector2(0.5f, 0.5f);
        grdRT.sizeDelta = new Vector2(800, 600);
        grdRT.anchoredPosition = new Vector2(-200, 0);
        grd.GetComponent<Image>().color = new Color(1,1,1,0.05f);
        var grid = grd.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160,160);
        grid.spacing = new Vector2(10,10);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // UI_BarraPecas
        var barra = new GameObject("UI_BarraPecas", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        barra.transform.SetParent(canvasGO.transform, false);
        var barraRT = barra.GetComponent<RectTransform>();
        barraRT.anchorMin = new Vector2(0.5f, 0);
        barraRT.anchorMax = new Vector2(0.5f, 0);
        barraRT.pivot = new Vector2(0.5f, 0);
        barraRT.sizeDelta = new Vector2(900, 180);
        barraRT.anchoredPosition = new Vector2(0, 30);
        barra.GetComponent<Image>().color = new Color(0,0,0,0.05f);
        var hlg = barra.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 20;

        // UI_DragLayer
        var dragLayer = new GameObject("UI_DragLayer", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        dragLayer.transform.SetParent(canvasGO.transform, false);
        var dlRT = dragLayer.GetComponent<RectTransform>();
        dlRT.anchorMin = Vector2.zero;
        dlRT.anchorMax = Vector2.one;
        dlRT.offsetMin = Vector2.zero;
        dlRT.offsetMax = Vector2.zero;
        var dlCanvas = dragLayer.GetComponent<Canvas>();
        dlCanvas.overrideSorting = true;
        dlCanvas.sortingOrder = 1000;

        // UI_HUD
        var hud = new GameObject("UI_HUD", typeof(RectTransform));
        hud.transform.SetParent(canvasGO.transform, false);
        var hudRT = hud.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0,1);
        hudRT.anchorMax = new Vector2(1,1);
        hudRT.pivot = new Vector2(0.5f,1);
        hudRT.sizeDelta = new Vector2(0, 120);
        hudRT.anchoredPosition = new Vector2(0, -20);

        // TMP Essentials
        if (TMP_Settings.instance == null)
        {
            if (EditorUtility.DisplayDialog("TextMeshPro", "Os TMP Essentials não estão importados.\nQueres importar agora?", "Importar", "Cancelar"))
            {
                TMPro.TMP_PackageUtilities.ImportProjectResourcesMenu();
            }
        }

        // TXT_Tempo
        var tempoGO = new GameObject("TXT_Tempo", typeof(RectTransform), typeof(TextMeshProUGUI));
        tempoGO.transform.SetParent(hud.transform, false);
        var tempoRT = tempoGO.GetComponent<RectTransform>();
        tempoRT.anchorMin = new Vector2(0,1);
        tempoRT.anchorMax = new Vector2(0,1);
        tempoRT.pivot = new Vector2(0,1);
        tempoRT.anchoredPosition = new Vector2(20, -20);
        var tempoTMP = tempoGO.GetComponent<TextMeshProUGUI>();
        tempoTMP.text = "TIME: 2:30";
        tempoTMP.fontSize = 48;
        tempoTMP.alignment = TextAlignmentOptions.Left;

        // TXT_Zoo
        var zooGO = new GameObject("TXT_Zoo", typeof(RectTransform), typeof(TextMeshProUGUI));
        zooGO.transform.SetParent(hud.transform, false);
        var zooRT = zooGO.GetComponent<RectTransform>();
        zooRT.anchorMin = new Vector2(1,1);
        zooRT.anchorMax = new Vector2(1,1);
        zooRT.pivot = new Vector2(1,1);
        zooRT.anchoredPosition = new Vector2(-20, -20);
        var zooTMP = zooGO.GetComponent<TextMeshProUGUI>();
        zooTMP.text = "ZOO: 0";
        zooTMP.fontSize = 48;
        zooTMP.alignment = TextAlignmentOptions.Right;

        // GO_Sistemas
        var sistemas = new GameObject("GO_Sistemas");
        sistemas.transform.SetParent(canvasGO.transform, false);

        var gg = sistemas.AddComponent<GestorGrelha>();
        var cj = sistemas.AddComponent<ControladorJogo>();

        // Prefabs rápidos
        var pfCelula = new GameObject("PF_Celula",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CelulaTabuleiro));

        var pfPeca = new GameObject("PF_Peca",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup),
            typeof(Peca), typeof(PecaFlip), typeof(TileID)); // 👈 adicionados

        pfCelula.GetComponent<Image>().color = Color.white;
        var imgPeca = pfPeca.GetComponent<Image>();
        imgPeca.color = new Color(0.2f, 0.5f, 1f, 1f);
        imgPeca.preserveAspect = true;

        string prefabDir = "Assets/Prefabs";
        if (!System.IO.Directory.Exists(prefabDir))
            System.IO.Directory.CreateDirectory(prefabDir);

        var celulaPath = $"{prefabDir}/PF_Celula.prefab";
        var pecaPath   = $"{prefabDir}/PF_Peca.prefab";

        var celulaPrefab = PrefabUtility.SaveAsPrefabAsset(pfCelula, celulaPath);
        var pecaPrefab   = PrefabUtility.SaveAsPrefabAsset(pfPeca, pecaPath);
        Object.DestroyImmediate(pfCelula);
        Object.DestroyImmediate(pfPeca);

        // Ligar referências (usar RectTransform nos aliases)
        gg.PaiTabuleiro  = grd.transform  as RectTransform;
        gg.PrefabCelula  = celulaPrefab;
        gg.PaiBarraPecas = barra.transform as RectTransform;
        gg.PrefabPeca    = pecaPrefab;

        gg.GridRoot = gg.PaiTabuleiro;
        gg.MaoRoot  = gg.PaiBarraPecas;

        cj.TxtTempo = tempoTMP;
        cj.TxtZoo   = zooTMP;

        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog("GeoZoo", "Cena base construída!\nCarrega Play para testar o drag & drop.", "OK");
    }
}
#endif
