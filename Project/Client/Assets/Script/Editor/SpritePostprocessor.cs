using UnityEditor;
using UnityEngine;

public class SpritePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;

        if (assetPath.Contains("Assets/SpriteAtlas/Atlas") && assetPath.EndsWith(".png"))
        {
            textureImporter.textureType = TextureImporterType.Sprite;
        }
    }
}

