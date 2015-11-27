using UnityEngine;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System.Reflection;
using System.IO;
using System.Linq;

namespace AdvancedToolbar {
	internal class TextureLoader {
		public static string AssemblyDirectory{
			get{
				PluginManager pluginManager = Singleton<PluginManager>.instance;
				var plugins = pluginManager.GetPluginsInfo();
				foreach(PluginManager.PluginInfo info in plugins){
					if(info.name.Equals(ToolbarCenterMod.modName)) //|| info.name.Equals("540758804"))
						return info.modPath;
				}
				return "";
			}
		}

		public enum FileDirectory{
			Images,
			ROOT
		}

		private static string GetFilePath(FileDirectory dir,string filename){
			if(dir == FileDirectory.ROOT){
				return filename;
			}
			return Path.Combine(AssemblyDirectory,Path.Combine(dir.ToString(), filename));
		}

		public static UITextureAtlas CreateTextureAtlas(string atlasName, string filename, FileDirectory dir, string[] spriteNames, int rows, int cols){
			Texture2D texture2D = LoadTextureFromFile(GetFilePath(dir,filename + ".png"));
			Debug.Print(texture2D.width, texture2D.height);
			UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();
			Material material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
			material.mainTexture = texture2D;
			atlas.material = material;
			atlas.name = atlasName;

			float spriteWidth = 1.0f/cols;
			float spriteHeight = 1.0f/rows;

			for(int i = 0; i < spriteNames.Length; i++){
				float x = (i % cols) / (float)cols;
				float y = (1.0f - spriteHeight) - ((i / cols) / (float)rows);
				atlas.AddSprite(new UITextureAtlas.SpriteInfo {
					name = spriteNames[i],
					texture = texture2D,
					region = new Rect(x, y, spriteWidth, spriteHeight)
				});
			}

			return atlas;

		}

		public static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames, FileDirectory dir){
			int maxSize = 1024;
			Texture2D texture2D = new Texture2D(maxSize, maxSize, TextureFormat.ARGB32, false);
			Texture2D[] textures = new Texture2D[spriteNames.Length];
			Rect[] regions = new Rect[spriteNames.Length];

			for (int i = 0; i < spriteNames.Length; i++)
				textures[i] = TextureLoader.LoadTextureFromFile(GetFilePath(dir,spriteNames[i] + ".png"));

			regions = texture2D.PackTextures(textures, 2, maxSize);

			UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
			Material material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
			material.mainTexture = texture2D;
			textureAtlas.material = material;
			textureAtlas.name = atlasName;

			for (int i = 0; i < spriteNames.Length; i++) {
				UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo {
					name = spriteNames[i],
					texture = textures[i],
					region = regions[i],
				};

				textureAtlas.AddSprite(item);
			}

			return textureAtlas;
		}

		private static Texture2D LoadTextureFromFile(string filepath){
			System.IO.Stream resourceStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open);
			byte[] array = new byte[resourceStream.Length];
			resourceStream.Read(array, 0, array.Length);

			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
			texture2D.LoadImage(array);
			return texture2D;
		}

        /**
         * Method sourced from SamsamTSs Airport Roads mod for Cities Skylines
         * Adds array of Texture2D objects to a UITextureAtlas
         * https://github.com/SamsamTS/CS-AirportRoads/blob/master/AirportRoads/AirportRoads.cs
         **/
        private static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures) {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++) {
                // Locked textures workaround
                Texture2D texture2D = atlas.sprites[i].texture;

                RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                Graphics.Blit(texture2D, renderTexture);

                RenderTexture active = RenderTexture.active;
                texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                RenderTexture.active = renderTexture;
                texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                texture2D.Apply();
                RenderTexture.active = active;

                RenderTexture.ReleaseTemporary(renderTexture);

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }
    }
}