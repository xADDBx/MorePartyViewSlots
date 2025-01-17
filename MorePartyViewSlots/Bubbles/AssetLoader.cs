﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityModManagerNet.UnityModManager;
using UnityEngine;

namespace MorePartyViewSlots {
    class AssetLoader {
        public static Sprite LoadInternal(string folder, string file, Vector2Int size) {
            return Image2Sprite.Create($"{Main.ModEntry.Path}{file}", size);
        }
        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static class Image2Sprite {
            public static string icons_folder = "";
            public static Sprite Create(string filePath, Vector2Int size) {
                var bytes = File.ReadAllBytes(icons_folder + filePath);
                var texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
                _ = texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, size.x, size.y), new Vector2(0, 0));
            }
        }
    }

    public struct ModIcon {
        private readonly string name;
        private readonly Vector2Int size;

        public ModIcon(string name, int w, int h) {
            _Sprite = null;
            this.name = name;
            this.size = new Vector2Int(w, h);
        }
        public ModIcon(string name, Vector2Int size) {
            _Sprite = null;
            this.name = name;
            this.size = size;
        }

        private Sprite _Sprite;
        public Sprite Sprite => _Sprite ??= (AssetLoader.LoadInternal("icons", name + ".png", size) ?? AssetLoader.LoadInternal("icons", "missing", new Vector2Int(32, 32)));

    }
}
