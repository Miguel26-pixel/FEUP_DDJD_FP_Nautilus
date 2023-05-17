using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class SpliceItemSprite : MonoBehaviour
{
    public Texture2D texture;
    public string outputFolder = "Assets/Resources/ItemIcons";
    public string outputName = "test";

    public int sliceWidth = 128;
    public int sliceHeight = 128;

    public void Splice()
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.isReadable = true;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        List<SpriteRect> spriteData = new();

        int slicesX = texture.width / sliceWidth;
        int slicesY = texture.height / sliceHeight;

        for (int y = 0; y < slicesY; y++)
        {
            for (int x = 0; x < slicesX; x++)
            {
                SpriteRect spriteRect = new SpriteRect
                {
                    name = $"{outputName}_{y}_{x}",
                    spriteID = GUID.Generate(),
                    rect = new Rect(x * sliceWidth, (slicesY - y - 1) * sliceHeight, sliceWidth, sliceHeight),
                    pivot = new Vector2(0.5f, 0.5f)
                };

                // Check if pixels are transparent, if so, don't add to sprite sheet
                Texture2D sprite = new((int)spriteRect.rect.width, (int)spriteRect.rect.height);
                sprite.SetPixels(texture.GetPixels(
                    (int)spriteRect.rect.x,
                    (int)spriteRect.rect.y,
                    (int)spriteRect.rect.width,
                    (int)spriteRect.rect.height)
                );

                bool transparent = sprite.GetPixels().All(pixel => pixel.a == 0);

                if (transparent)
                {
                    continue;
                }

                spriteData.Add(spriteRect);
            }
        }

        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        dataProvider.SetSpriteRects(spriteData.ToArray());

        ISpriteNameFileIdDataProvider spriteNameFileIdDataProvider =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        List<SpriteNameFileIdPair> nameFileIdPairs = spriteNameFileIdDataProvider.GetNameFileIdPairs().ToList();
        nameFileIdPairs.AddRange(spriteData.Select(spriteRect =>
            new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID)));

        spriteNameFileIdDataProvider.SetNameFileIdPairs(nameFileIdPairs);

        dataProvider.Apply();

        // importer.isReadable = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}