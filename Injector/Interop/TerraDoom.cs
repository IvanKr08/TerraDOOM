using System;
using System.Collections.Generic;
using System.Reflection;

using ManagedDoom;
using ManagedDoom.Audio;
using ManagedDoom.UserInput;
using ManagedDoom.Video;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using DoomContent = ManagedDoom.GameContent;

namespace Terraria.FirstApril {
    internal partial class TerraDoom : IMusic, IVideo, IUserInput {
        DoomContent content;
        Exception   exception;
        Renderer    renderer;
        Config      config;
        Doom        doom;
        Main        main;

        Color[] textureData;

        KeyboardState prevState;

        bool resetGuard;

        MethodInfo  tripWireMethod;

        HashSet<DoomKey> prevInput = new HashSet<DoomKey>();
        int              turnHeldTime, fpsScale, frameCount, prevMsgTime;
        short            prevAction, prevFire, prevEsc;
        short[]          prevWeapons   = new short[7];

        public TerraDoom() {
            Console.WriteLine("Инициализация...");

            try {
                var args = new CommandLineArgs(Environment.GetCommandLineArgs());

                main    = Main.instance;
                config  = new Config(ConfigUtilities.GetConfigPath());
                content = new DoomContent(args);

                config.video_screenwidth  = 320;
                config.video_screenheight = 240;
                
                renderer    = new Renderer(config, content);
                textureData = new Color[renderer.Width * renderer.Height];
                doom        = new Doom(args, config, content, this, null, this, this);

                fpsScale   = args.timedemo.Present ? 1 : config.video_fpsscale;
                frameCount = -1;

                tripWireMethod = typeof(Wiring).GetMethod("TripWire", BindingFlags.NonPublic | BindingFlags.Static);

                var verMsg = $" ({string.Join(", ", content.Wad.Names)})";
                Main.versionNumber  += verMsg;
                Main.versionNumber2 += verMsg;
                
                Console.WriteLine("TerraDOOM загружен успешно!");
            }
            catch (Exception e) {
                var verMsg = $"ОШИБКА: проверьте наличие WAD файла! ({e.Message})";
                Main.versionNumber  = verMsg;
                Main.versionNumber2 = verMsg;
                Halt(e);
            }

        }

        void Halt(Exception e) {
            Console.WriteLine(e);
            Console.WriteLine("TerraDOOM был отключен из-за исключения");
            exception = e;
        }
        void TripWire(int x, int y, int w, int h) => tripWireMethod.Invoke(null, new object[] { x, y, w, h });
        public void Handle() {
            if (string.IsNullOrEmpty(Main.worldName) ||
                exception                 != null    ||
                Main.worldName            != "Doom"  ||
                Main.tile[2091, 218]      == null    ||
                Main.tile[2091, 218].type != 132)
                return;

            // Об этом никто не узнает, про это никто не услышит
            var newS = Keyboard.GetState();
            if (newS.IsKeyDown(Keys.OemTilde) && !prevState.IsKeyDown(Keys.OemTilde)) Main.LocalPlayer.ghost = !Main.LocalPlayer.ghost;
            prevState = newS;

            if (Main.tile[2091, 218].frameX == 0) {
                if (!resetGuard) Reset();
                resetGuard = true;
                return;
            }
            resetGuard = false;

            try {
                frameCount++;

                if (frameCount % fpsScale == 0) {
                    UpdateInput();

                    if (doom.Update() == UpdateResult.Completed) {
                        main.Exit();
                    }

                    var world = doom.Game.World;
                    if (world != null && world.ConsolePlayer != null) {
                        var player = world.ConsolePlayer;
                        if (player != null) {
                            if (player.Health < 1) {
                                Main.LocalPlayer.KillMe(DataStructures.PlayerDeathReason.ByCustomReason("Happy April Fools' Day!"), 9999, 0, false);
                            }
                            if (!string.IsNullOrWhiteSpace(player.Message) && player.MessageTime > prevMsgTime) {
                                Main.NewTextMultiline(player.Message, force: false, Color.Pink, 460);
                            }
                            prevMsgTime = player.MessageTime;

                            DrawHUD(player);
                        }
                    }
                }

                Render(doom, Fixed.FromInt(frameCount % fpsScale + 1) / fpsScale);
            }
            catch (Exception e) {
                Halt(e);
            }
        }
        public void StartMusic(Bgm music, bool loop) {
            // На всякий случай отключим все
            for (int i = 2076; i < 2076 + 7 * 2; i += 2) {
                if (Main.tile[i, 218].frameX > 0) {
                    TripWire(i, 222, 1, 1);
                }
            }

            if (music == Bgm.INTRO) {       // Меню
                TripWire(2084, 222, 1, 1);
            }
            else if (music == Bgm.NONE) {   // Сброс
            }
            else if (music == Bgm.INTER) {  // Eerie (Otherworld)
                TripWire(2082, 222, 1, 1);
            }
            else if ((int)music % 5 == 0) { // Boss 2
                TripWire(2080, 222, 1, 1);
            }
            else if ((int)music % 5 == 1) { // Eerie
                TripWire(2086, 222, 1, 1);
            }
            else if ((int)music % 5 == 2) { // Hell
                TripWire(2088, 222, 1, 1);
            }
            else if ((int)music % 5 == 3) { // Boss 1 (Otherworld)
                TripWire(2076, 222, 1, 1);
            }
            else if ((int)music % 5 == 4) { // Boss 2 (Otherworld)
                TripWire(2078, 222, 1, 1);
            }

            Console.WriteLine($"Новая музыка: {music} {loop}");
        }
        public void UpdateInput() {
            var input = new HashSet<DoomKey>();

            var up    = Main.tile[2096, 183].frameX;
            var left  = Main.tile[2094, 189].frameX;
            var right = Main.tile[2102, 189].frameX;
            var down  = Main.tile[2096, 199].frameX;
            var door  = Main.tile[2097, 185].frameX;
            var fire  = Main.tile[2095, 182].frameX;
            var esc   = Main.tile[2098, 184].frameY;

            if (up    > 0) input.Add(DoomKey.Up);
            if (left  > 0) input.Add(DoomKey.Left);
            if (right > 0) input.Add(DoomKey.Right);
            if (down  > 0) input.Add(DoomKey.Down);
            if (door != prevAction) {
                input.Add(DoomKey.Space);
                input.Add(DoomKey.Enter);
                prevAction = door;
            }
            if (fire != prevFire) {
                input.Add(DoomKey.LControl);
                prevFire = fire;
            }
            if (esc != prevEsc) {
                input.Add(DoomKey.Escape);
                prevEsc = esc;
            }

            for (int i = 0; i < 7; i++) {
                var weapon = Main.tile[2095 + i, 181].frameY;
                if (weapon != prevWeapons[i]) input.Add(DoomKey.Num1 + i);
                prevWeapons[i] = weapon;
            }

            var newKeys = new HashSet<DoomKey>(input);
            newKeys.ExceptWith(prevInput);
            foreach (var i in newKeys) {
                doom.PostEvent(new DoomEvent(EventType.KeyDown, i));
            }

            var missingKeys = new HashSet<DoomKey>(prevInput);
            missingKeys.ExceptWith(input);
            foreach (var i in missingKeys) {
                doom.PostEvent(new DoomEvent(EventType.KeyUp, i));
            }

            prevInput = input;
        }
        public unsafe void BuildTicCmd(TicCmd cmd) {
            var keyForward      = IsPressed(config.key_forward);
            var keyBackward     = IsPressed(config.key_backward);
            var keyStrafeLeft   = IsPressed(config.key_strafeleft);
            var keyStrafeRight  = IsPressed(config.key_straferight);
            var keyTurnLeft     = IsPressed(config.key_turnleft);
            var keyTurnRight    = IsPressed(config.key_turnright);
            var keyFire         = IsPressed(config.key_fire);
            var keyUse          = IsPressed(config.key_use);
            var keyRun          = IsPressed(config.key_run);
            var keyStrafe       = IsPressed(config.key_strafe);

            cmd.Clear();

            var strafe  = keyStrafe;
            var speed   = keyRun ? 1 : 0;
            var forward = 0;
            var side    = 0;

            if (config.game_alwaysrun) {
                speed = 1 - speed;
            }

            if (keyTurnLeft || keyTurnRight) {
                turnHeldTime++;
            }
            else {
                turnHeldTime = 0;
            }

            int turnSpeed = turnHeldTime < PlayerBehavior.SlowTurnTics ? 2 : speed;

            if (strafe) {
                if (keyTurnRight) {
                    side += PlayerBehavior.SideMove[speed];
                }
                if (keyTurnLeft) {
                    side -= PlayerBehavior.SideMove[speed];
                }
            }
            else {
                if (keyTurnRight) {
                    cmd.AngleTurn -= (short)PlayerBehavior.AngleTurn[turnSpeed];
                }
                if (keyTurnLeft) {
                    cmd.AngleTurn += (short)PlayerBehavior.AngleTurn[turnSpeed];
                }
            }

            if (keyForward) {
                forward += PlayerBehavior.ForwardMove[speed];
            }
            if (keyBackward) {
                forward -= PlayerBehavior.ForwardMove[speed];
            }

            if (keyStrafeLeft) {
                side -= PlayerBehavior.SideMove[speed];
            }
            if (keyStrafeRight) {
                side += PlayerBehavior.SideMove[speed];
            }

            if (keyFire) {
                cmd.Buttons |= TicCmdButtons.Attack;
            }

            if (keyUse) {
                cmd.Buttons |= TicCmdButtons.Use;
            }

            // Check weapon keys.
            for (var i = 0; i < 7; i++) {
                if (prevInput.Contains(DoomKey.Num1 + i)) {
                    cmd.Buttons |= TicCmdButtons.Change;
                    cmd.Buttons |= (byte)(i << TicCmdButtons.WeaponShift);
                    break;
                }
            }

            if (forward > PlayerBehavior.MaxMove) {
                forward = PlayerBehavior.MaxMove;
            }
            else if (forward < -PlayerBehavior.MaxMove) {
                forward = -PlayerBehavior.MaxMove;
            }
            if (side > PlayerBehavior.MaxMove) {
                side = PlayerBehavior.MaxMove;
            }
            else if (side < -PlayerBehavior.MaxMove) {
                side = -PlayerBehavior.MaxMove;
            }

            cmd.ForwardMove += (sbyte)forward;
            cmd.SideMove    += (sbyte)side;
        }
        private bool IsPressed(KeyBinding keyBinding) {
            foreach (var key in keyBinding.Keys) {
                if (prevInput.Contains(key)) return true;
            }

            return false;
        }

        #region Internals
        void IVideo.InitializeWipe() => renderer.InitializeWipe();
        bool IVideo.HasFocus() => true;
        
        void IUserInput.Reset() { }
        void IUserInput.GrabMouse() { }
        void IUserInput.ReleaseMouse() { }

        int  IUserInput.MaxMouseSensitivity { get => 0; }
        int  IUserInput.MouseSensitivity { get => 0; set => _ = value; }

        int  IMusic.MaxVolume { get => 1; }
        int  IMusic.Volume { get => 1; set => _ = value; }

        int  IVideo.WipeBandCount => renderer.WipeBandCount;
        int  IVideo.WipeHeight => renderer.WipeHeight;
        int  IVideo.MaxWindowSize => renderer.MaxWindowSize;
        int  IVideo.WindowSize { get => renderer.WindowSize; set => renderer.WindowSize = value; }
        bool IVideo.DisplayMessage { get => renderer.DisplayMessage; set => renderer.DisplayMessage = value; }
        int  IVideo.MaxGammaCorrectionLevel => renderer.MaxGammaCorrectionLevel;
        int  IVideo.GammaCorrectionLevel { get => renderer.GammaCorrectionLevel; set => renderer.GammaCorrectionLevel = value; }
        #endregion
    }
}
