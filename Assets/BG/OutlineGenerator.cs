using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OutlineGenerator : MonoBehaviour
{
    public Sprite spriteToOutline;           // The sprite to generate the outline for
    public Color outlineColor = Color.black; // Color of the outline
    public float outlineWidth = 0.05f;       // Width of the outline
    private Texture2D outlineTexture;        // To store the generated outline texture

    // Method to generate the outline as an image from the sprite
    public void GenerateOutlineAsImage()
    {
        if (spriteToOutline == null)
        {
            Debug.LogError("No sprite assigned for outline generation.");
            return;
        }

        // Get the sprite texture (original sprite texture)
        Texture2D spriteTexture = spriteToOutline.texture;

        // Create a RenderTexture to render the outline
        RenderTexture renderTexture = new RenderTexture(spriteTexture.width, spriteTexture.height, 24);
        renderTexture.Create();

        // Set the render target to the renderTexture
        RenderTexture.active = renderTexture;

        // Create a temporary camera to render the outline
        Camera camera = new GameObject("OutlineCamera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = spriteTexture.height / 2f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;

        // Set the camera's position to match the sprite's position
        camera.transform.position = new Vector3(spriteTexture.width / 2f, spriteTexture.height / 2f, -10);

        // Create a new Material with the outline shader
        Material outlineMaterial = new Material(Shader.Find("Custom/OutlineShader"));
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);

        // Create a quad to render the sprite with the outline effect
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.GetComponent<Renderer>().material = outlineMaterial;
        quad.transform.localScale = new Vector3(spriteTexture.width, spriteTexture.height, 1);
        quad.transform.position = new Vector3(spriteTexture.width / 2f, spriteTexture.height / 2f, 0);
        quad.GetComponent<Renderer>().material.mainTexture = spriteTexture;

        // Render the scene with the outline effect
        camera.Render();

        // Create a Texture2D from the RenderTexture
        outlineTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        outlineTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        outlineTexture.Apply();

        // Clean up
        DestroyImmediate(quad);
        DestroyImmediate(camera.gameObject);
        RenderTexture.active = null;
        renderTexture.Release();

        // Save the texture as an image (PNG format)
        SaveTextureToPNG(outlineTexture, "OutlineImage.png");
    }

    // Method to save the texture as a PNG image
    private void SaveTextureToPNG(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Outline image saved to {path}");
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Outline as Image")]
    private void GenerateOutlineAsImageContextMenu()
    {
        GenerateOutlineAsImage();
    }
#endif
}
