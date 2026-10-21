using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Editor only. Renders the model of every ItemPickup in the open scenes to a PNG with a
// transparent background in Data/Items/Icons, imports it as a sprite and sets it as that
// item's icon. Run Last Light > Bake Item Icons with each level open. It overwrites the
// old icons, so run it again after changing a pickup's model.
public static class ItemIconBaker
{
    const string IconFolder = "Assets/_Project/Data/Items/Icons";
    const int IconSize = 256;
    const int Supersample = 4;          // render 4x larger and average down, for smooth edges
    const float Padding = 1.08f;
    const float FrontYaw = 210f;        // front three-quarter view; the models face +z
    const float StandingPitch = 20f;
    const float FlatPitch = 70f;        // items lying down, like the bolt cutters, are seen from above

    [MenuItem("Last Light/Bake Item Icons")]
    static void BakeOpenScenes()
    {
        var pickups = Object.FindObjectsByType<ItemPickup>(FindObjectsInactive.Include);
        if (pickups.Length == 0)
        {
            Debug.LogWarning("ItemIconBaker: no ItemPickup in the open scenes.");
            return;
        }

        foreach (var pickup in pickups)
            Bake(pickup);
    }

    public static Sprite Bake(ItemPickup pickup)
    {
        ItemData item = pickup.Item;
        if (item == null)
        {
            Debug.LogWarning($"ItemIconBaker: {pickup.name} has no item.", pickup);
            return null;
        }

        Texture2D image;
        Scene scene = EditorSceneManager.NewPreviewScene();
        try
        {
            image = Render(pickup.gameObject, scene);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(scene);
        }

        if (image == null)
        {
            Debug.LogWarning($"ItemIconBaker: {pickup.name} has no visible mesh.", pickup);
            return null;
        }

        if (!AssetDatabase.IsValidFolder(IconFolder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(IconFolder).Replace('\\', '/'), Path.GetFileName(IconFolder));

        string path = $"{IconFolder}/Icon_{item.name.Replace("Item_", "")}.png";
        File.WriteAllBytes(path, image.EncodeToPNG());
        Object.DestroyImmediate(image);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = IconSize;
        importer.SaveAndReimport();

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        var serializedItem = new SerializedObject(item);
        serializedItem.FindProperty("icon").objectReferenceValue = sprite;
        serializedItem.ApplyModifiedProperties();
        AssetDatabase.SaveAssetIfDirty(item);

        Debug.Log($"ItemIconBaker: {path} is now the icon of {item.name}.", sprite);
        return sprite;
    }

    // Copies the pickup into the preview scene and renders it with its own lights, so the
    // level's lighting and fog don't change the icon.
    static Texture2D Render(GameObject source, Scene scene)
    {
        GameObject model = Object.Instantiate(source, NewObject("Icon", scene).transform);
        model.SetActive(true);
        model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        model.transform.localScale = source.transform.lossyScale;
        foreach (var light in model.GetComponentsInChildren<Light>())
            light.enabled = false;
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            renderer.enabled &= renderer is MeshRenderer || renderer is SkinnedMeshRenderer;

        List<Vector3> points = MeshPoints(model);
        if (points.Count == 0)
            return null;

        var bounds = new Bounds(points[0], Vector3.zero);
        foreach (var point in points)
            bounds.Encapsulate(point);

        // Standing items get the front view. Flat ones are turned to whichever angle shows
        // them biggest, which puts long items like the cutters on the diagonal.
        bool lyingFlat = bounds.size.y < 0.25f * Mathf.Max(bounds.size.x, bounds.size.z);
        Quaternion view = Quaternion.Euler(lyingFlat ? FlatPitch : StandingPitch, FrontYaw, 0f);
        Bounds frame = ViewBounds(points, view);
        if (lyingFlat)
        {
            for (float yaw = 0f; yaw < 360f; yaw += 5f)
            {
                Quaternion candidate = Quaternion.Euler(FlatPitch, yaw, 0f);
                Bounds candidateFrame = ViewBounds(points, candidate);
                if (Mathf.Max(candidateFrame.size.x, candidateFrame.size.y) < Mathf.Max(frame.size.x, frame.size.y))
                {
                    view = candidate;
                    frame = candidateFrame;
                }
            }
        }

        var camera = NewObject("Camera", scene).AddComponent<Camera>();
        camera.scene = scene;
        camera.enabled = false;
        camera.transform.SetPositionAndRotation(view * (frame.center - Vector3.forward * (frame.extents.z + 1f)), view);
        camera.orthographic = true;
        camera.orthographicSize = 0.5f * Mathf.Max(frame.size.x, frame.size.y) * Padding;
        camera.nearClipPlane = 0.5f;
        camera.farClipPlane = frame.size.z + 1.5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.allowHDR = false;        // HDR targets have no alpha channel
        camera.allowMSAA = false;
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = false;
        cameraData.renderShadows = false;

        AddLight(scene, view * Quaternion.Euler(40f, 30f, 0f), 0.95f, new Color(1f, 0.96f, 0.9f));   // key, from the upper left
        AddLight(scene, view * Quaternion.Euler(10f, -50f, 0f), 0.35f, new Color(0.7f, 0.8f, 1f));   // fill, from the right
        AddLight(scene, view * Quaternion.Euler(10f, 160f, 0f), 0.5f, Color.white);                // rim, so edges show on the dark slot

        int renderSize = IconSize * Supersample;
        var target = RenderTexture.GetTemporary(renderSize, renderSize, 24, GraphicsFormat.R8G8B8A8_SRGB);
        Unsupported.SetOverrideLightingSettings(scene);
        try
        {
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            var ambient = new SphericalHarmonicsL2();
            ambient.AddAmbientLight(new Color(0.25f, 0.25f, 0.28f));
            RenderSettings.ambientProbe = ambient;
            camera.targetTexture = target;
            camera.Render();
        }
        finally
        {
            Unsupported.RestoreOverrideLightingSettings();
            camera.targetTexture = null;
        }

        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var full = new Texture2D(renderSize, renderSize, TextureFormat.RGBA32, false);
        full.ReadPixels(new Rect(0, 0, renderSize, renderSize), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);

        Texture2D icon = Downsample(full);
        Object.DestroyImmediate(full);
        return icon;
    }

    // World-space vertices of the visible meshes, so the frame fits the model tightly.
    static List<Vector3> MeshPoints(GameObject model)
    {
        var points = new List<Vector3>();
        foreach (var renderer in model.GetComponentsInChildren<Renderer>())
        {
            Mesh mesh = null;
            if (renderer is SkinnedMeshRenderer skinned)
                mesh = skinned.sharedMesh;
            else if (renderer.TryGetComponent(out MeshFilter filter))
                mesh = filter.sharedMesh;
            if (!renderer.enabled || mesh == null)
                continue;

            Matrix4x4 toWorld = renderer.transform.localToWorldMatrix;
            foreach (var vertex in mesh.vertices)
                points.Add(toWorld.MultiplyPoint3x4(vertex));
        }
        return points;
    }

    // The points' bounds as seen by a camera with this rotation.
    static Bounds ViewBounds(List<Vector3> points, Quaternion view)
    {
        Quaternion toView = Quaternion.Inverse(view);
        var bounds = new Bounds(toView * points[0], Vector3.zero);
        foreach (var point in points)
            bounds.Encapsulate(toView * point);
        return bounds;
    }

    // Averages each Supersample x Supersample block in linear space. Colours are weighted
    // by alpha, so edge pixels keep the model's colour instead of fading to black.
    static Texture2D Downsample(Texture2D full)
    {
        Color[] source = full.GetPixels();
        var pixels = new Color[IconSize * IconSize];
        int fullSize = IconSize * Supersample;
        for (int y = 0; y < IconSize; y++)
        {
            for (int x = 0; x < IconSize; x++)
            {
                Color sum = Color.clear;
                float coverage = 0f;
                for (int sy = 0; sy < Supersample; sy++)
                {
                    for (int sx = 0; sx < Supersample; sx++)
                    {
                        Color sample = source[(y * Supersample + sy) * fullSize + x * Supersample + sx];
                        sum += sample.linear * sample.a;
                        coverage += sample.a;
                    }
                }

                Color pixel = coverage > 0f ? (sum / coverage).gamma : Color.clear;
                pixel.a = coverage / (Supersample * Supersample);
                pixels[y * IconSize + x] = pixel;
            }
        }

        var icon = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
        icon.SetPixels(pixels);
        icon.Apply();
        return icon;
    }

    // HideAndDontSave keeps the open level from being marked as changed.
    static GameObject NewObject(string name, Scene scene)
    {
        var gameObject = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        return gameObject;
    }

    static void AddLight(Scene scene, Quaternion rotation, float intensity, Color color)
    {
        var light = NewObject("Light", scene).AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = rotation;
        light.intensity = intensity;
        light.color = color;
    }
}
