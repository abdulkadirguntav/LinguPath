using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;

public static class CanvasScalerFixer
{
    [MenuItem("Tools/Tüm Sahnelerdeki Canvas Scaler'ları Düzelt")]
    public static void FixAllScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        string[] scenePaths = Directory.GetFiles("Assets/Scene", "*.unity", SearchOption.AllDirectories);

        int totalFixed = 0;

        foreach (string scenePath in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int count = FixScalersInLoadedScene();
            totalFixed += count;
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[CanvasScalerFixer] {scenePath}: {count} Canvas Scaler düzeltildi.");
        }

        if (!string.IsNullOrEmpty(currentScenePath))
            EditorSceneManager.OpenScene(currentScenePath);

        EditorUtility.DisplayDialog(
            "Tamamlandı",
            $"{totalFixed} Canvas Scaler düzeltildi.\n\nAyarlar:\n• Scale With Screen Size\n• Referans: 1080x1920\n• Match: 0.5 (Dengeli)",
            "Tamam"
        );
    }

    private static int FixScalersInLoadedScene()
    {
        CanvasScaler[] scalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
        foreach (var scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
        }
        return scalers.Length;
    }
}
