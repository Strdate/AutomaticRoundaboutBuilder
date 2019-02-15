using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using System.Reflection;

/* Direct copy from Parallel Road Tool */

namespace RoundaboutBuilder.UI
{
    class ResourceLoader
    {
        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D texture2D = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false);
            texture2D.filterMode = FilterMode.Bilinear;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream manifestResourceStream = executingAssembly.GetManifestResourceStream("RoundaboutBuilder.Resources." + textureFile);
            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);
            texture2D.LoadImage(array);
            texture2D.Apply(true, true);
            UITextureAtlas uitextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Material material = UnityEngine.Object.Instantiate<Material>(baseMaterial);
            material.mainTexture = texture2D;
            uitextureAtlas.material = material;
            uitextureAtlas.name = atlasName;
            int num2;
            for (int i = 0; i < spriteNames.Length; i = num2)
            {
                float num = 1f / (float)spriteNames.Length;
                UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = texture2D,
                    region = new Rect((float)i * num, 0f, num, 1f)
                };
                uitextureAtlas.AddSprite(spriteInfo);
                num2 = i + 1;
            }
            return uitextureAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++)
            {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++)
            {
                if (atlases[i].name == name)
                    return atlases[i];
            }

            return UIView.GetAView().defaultAtlas;
        }

        private static Texture2D loadTextureFromAssembly(string path)
        {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }

    }
}
