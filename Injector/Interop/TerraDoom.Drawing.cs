using System;

using ManagedDoom;

using Microsoft.Xna.Framework;

using DoomPlayer = ManagedDoom.Player;

namespace Terraria.FirstApril {
    internal partial class TerraDoom {
        const int NUM_CLEAR = -1000;

        int[] numbers = new int[] {
            1, 1, 1, 1,
            1, 0, 0, 1,
            1, 0, 0, 1,
            1, 0, 0, 1,
            1, 1, 1, 1,

            0, 0, 1, 0,
            0, 1, 1, 0,
            0, 0, 1, 0,
            0, 0, 1, 0,
            0, 1, 1, 1,

            0, 1, 1, 0,
            1, 0, 0, 1,
            0, 0, 1, 0,
            0, 1, 0, 0,
            1, 1, 1, 1,

            1, 1, 1, 0,
            0, 0, 0, 1,
            0, 1, 1, 0,
            0, 0, 0, 1,
            1, 1, 1, 0,

            1, 0, 0, 1,
            1, 0, 0, 1,
            1, 1, 1, 1,
            0, 0, 0, 1,
            0, 0, 0, 1,

            1, 1, 1, 1,
            1, 0, 0, 0,
            1, 1, 1, 0,
            0, 0, 0, 1,
            1, 1, 1, 0,

            0, 1, 1, 0,
            1, 0, 0, 0,
            1, 1, 1, 0,
            1, 0, 0, 1,
            0, 1, 1, 0,

            1, 1, 1, 1,
            0, 0, 0, 1,
            0, 0, 1, 0,
            0, 1, 0, 0,
            0, 1, 0, 0,

            1, 1, 1, 1,
            1, 0, 0, 1,
            1, 1, 1, 1,
            1, 0, 0, 1,
            1, 1, 1, 1,

            0, 1, 1, 1,
            1, 0, 0, 1,
            0, 1, 1, 1,
            0, 0, 0, 1,
            0, 1, 1, 0,

            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        };

        void DrawDigit(int posX, int posY, int digit) {
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 4; x++) {
                    var tile = Main.tile[posX + x, posY + y];
                    if (tile != null && tile.type == 445) tile.frameX = (short)(numbers[digit * 20 + y * 4 + x] * 18);
                }
            }
        }
        void DrawWeaponCell(int posX, int posY, bool val) {
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 2; x++) {
                    var tile = Main.tile[posX + x, posY + y];
                    if (tile != null && tile.type == 445) tile.frameX = (short)((val ? 1 : 0) * 18);
                }
            }
        }
        void DrawAvailableWeapons(bool[] arms) {
            DrawWeaponCell(2150, 188, arms[1]);
            DrawWeaponCell(2153, 188, arms[2]);
            DrawWeaponCell(2156, 188, arms[3]);

            DrawWeaponCell(2150, 194, arms[4]);
            DrawWeaponCell(2153, 194, arms[5]);
            DrawWeaponCell(2156, 194, arms[6]);
        }
        void Reset() {
            try {
                // Music
                StartMusic(Bgm.NONE, false);

                // HP
                DrawNum3(2041, 162, NUM_CLEAR);

                // AP
                DrawNum3(2041, 188, NUM_CLEAR);

                // Ammo
                DrawNum3(2152, 162, NUM_CLEAR);

                DrawAvailableWeapons(new bool[] { false, false, false, false, false, false, false });

                // Keycards
                for (int y = 0; y < 6; y++) {
                    for (int x = 0; x < 6; x++) {
                        var tile = Main.tile[2040 + x, 209 + y];
                        if (tile != null && tile.type == 429) tile.frameX = 0;
                    }
                }

                // Game field
                int w = renderer.Width;
                int h = renderer.Height;
                for (float y = 0; y < h; y += w / 101f) {
                    for (float x = 0; x < w; x += h / 60) {
                        var texData = textureData[(int)x * h + (int)y];

                        int tileX = (int)(x * (101f / w) + 2048).Clamp(2048, Main.maxTilesX);
                        int tileY = (int)(y * ( 60f / h) +  156).Clamp(156 , Main.maxTilesY);

                        var t = Main.tile[tileX, tileY] ?? new Tile();
                        if (t.type == 429) {
                            t.frameX       = 0;  
                            t.frameY       = 0;  
                            t.sTileHeader |= 0x20;
                        }
                        Main.tile[tileX, tileY] = t;
                    }
                }
            }
            catch (Exception e) {
                Halt(e);
            }
        }
        void DrawHUD(DoomPlayer player) {
            DrawNum3(2041, 162, player.Health);
            DrawNum3(2041, 188, player.ArmorPoints);

            switch (player.ReadyWeapon) {
                case WeaponType.Fist:
                case WeaponType.Chainsaw:
                default:
                    DrawNum3(2152, 162, NUM_CLEAR);
                    break;
                case WeaponType.Pistol:
                case WeaponType.Chaingun:
                    DrawNum3(2152, 162, player.Ammo[0]);
                    break;
                case WeaponType.Shotgun:
                case WeaponType.SuperShotgun:
                    DrawNum3(2152, 162, player.Ammo[1]);
                    break;
                case WeaponType.Missile:
                    DrawNum3(2152, 162, player.Ammo[2]);
                    break;
                case WeaponType.Plasma:
                case WeaponType.Bfg:
                    DrawNum3(2152, 162, player.Ammo[3]);
                    break;

            }

            DrawAvailableWeapons(player.WeaponOwned);

            int frameX = (player.Cards[1] ? 8 : 0) | (player.Cards[0] ? 4 : 0) | (player.Cards[2] ? 1 : 0);
            for (int y = 0; y < 6; y++) {
                for (int x = 0; x < 6; x++) {
                    var tile = Main.tile[2040 + x, 209 + y];
                    if (tile != null && tile.type == 429) tile.frameX = (short)(frameX * 18);
                }
            }
        }
        void DrawNum3(int x, int y, int val) {
            if (val != NUM_CLEAR) {
                DrawDigit(x, y, val / 100 % 10);
                DrawDigit(x, y + 6, val / 10 % 10);
                DrawDigit(x, y + 12, val / 1 % 10);
            }
            else {
                DrawDigit(x, y     , 10);
                DrawDigit(x, y + 6 , 10);
                DrawDigit(x, y + 12, 10);
            }
        }
        public unsafe void Render(Doom doom, Fixed frameFrac) {
            try {
                fixed (Color* p = textureData) {
                    renderer.Render(doom, (byte*)p, frameFrac);
                }

                int w = renderer.Width;
                int h = renderer.Height;
                for (float y = 0; y < h; y += w / 101f) {
                    for (float x = 0; x < w; x += h / 60) {
                        var texData = textureData[(int)x * h + (int)y];

                        int tileX = (int)(x * (101f / w) + 2048).Clamp(2048, Main.maxTilesX);
                        int tileY = (int)(y * ( 60f / h) +  156).Clamp(156 , Main.maxTilesY);

                        var t = Main.tile[tileX, tileY] ?? new Tile();
                        if (t.type == 0 || t.type == 429) {
                            short mid = (short)((texData.R + texData.G + texData.B) / 3);
                        
                            t.type         = 429;  // (ushort)(textureData[x * h + y].R % 32 + 1);
                            t.frameX       = 0;    // t.frameX = (short)(mid % 16 * 18);
                            t.frameY       = 0;    // (short)(mid % 2 * 18);
                            t.sTileHeader |= 0x20;

                            if (mid > 32) {
                                if ((((texData.G + texData.B) / 2) - texData.R) < -16 || (texData.R % 128) > 64) t.frameX |= 1;
                                if ((((texData.R + texData.B) / 2) - texData.G) < -16 || (texData.G % 128) > 64) t.frameX |= 2;
                                if ((((texData.R + texData.G) / 2) - texData.B) < -16 || (texData.B % 128) > 64) t.frameX |= 4;
                                if (mid > 128) t.frameX |= 8;
                            }
                            t.frameX *= 18;

                        }
                        Main.tile[tileX, tileY] = t;
                    }
                }
            }
            catch (Exception e) {
                Halt(e);
            }
        }
    }
}
