using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SariaMod.Items.Strange
{
    public class TestingStaffUISystem : ModSystem
    {
        private static readonly string[] ProjectileNames =
        {
            nameof(ColdWaveCenter),
            nameof(Flame),
            nameof(Locator),
            nameof(LocatorPellet),
            nameof(Explosion),
            nameof(Explosion2),
            nameof(Explosion3),
        };

        private const int MinimumShootSpeed = 0;
        private const int MaximumShootSpeed = 100;
        private const int MinimumDamage = 0;
        private const int MaximumDamage = 10000;
        private const int DefaultProjectileIndex = 0;
        private const int DefaultShootSpeed = 4;
        private const int DefaultDamage = 80;

        private static TestingStaffUIPanel panel;
        private UserInterface testingStaffInterface;

        public static bool IsOpen { get; private set; }
        public static int ProjectileOptionCount => ProjectileNames.Length;

        public static int SelectedProjectileIndex
        {
            get
            {
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                return WrapProjectileIndex(fairyPlayer?.TestingStaffProjectileIndex ?? DefaultProjectileIndex);
            }
            set
            {
                int wrappedIndex = WrapProjectileIndex(value);
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                if (fairyPlayer != null)
                    fairyPlayer.TestingStaffProjectileIndex = wrappedIndex;
            }
        }

        public static int SelectedShootSpeed
        {
            get
            {
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                int shootSpeed = fairyPlayer?.TestingStaffShootSpeed ?? DefaultShootSpeed;
                return System.Math.Clamp(shootSpeed, MinimumShootSpeed, MaximumShootSpeed);
            }
            set
            {
                int clampedSpeed = System.Math.Clamp(value, MinimumShootSpeed, MaximumShootSpeed);
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                if (fairyPlayer != null)
                    fairyPlayer.TestingStaffShootSpeed = clampedSpeed;
            }
        }

        public static int SelectedDamage
        {
            get
            {
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                return GetDamage(fairyPlayer);
            }
            set
            {
                int clampedDamage = System.Math.Clamp(value, MinimumDamage, MaximumDamage);
                FairyPlayer fairyPlayer = GetLocalFairyPlayer();
                if (fairyPlayer != null)
                    fairyPlayer.TestingStaffDamage = clampedDamage;
            }
        }

        public static int GetDamage(FairyPlayer fairyPlayer)
        {
            int damage = fairyPlayer?.TestingStaffDamage ?? DefaultDamage;
            return System.Math.Clamp(damage, MinimumDamage, MaximumDamage);
        }

        public static int SelectedProjectileType => GetProjectileType(SelectedProjectileIndex);
        public static string SelectedProjectileName => GetProjectileName(SelectedProjectileIndex);

        public override void Load()
        {
            if (Main.dedServ)
                return;

            panel = new TestingStaffUIPanel();
            panel.Activate();
            testingStaffInterface = new UserInterface();
            testingStaffInterface.SetState(panel);
        }

        public override void Unload()
        {
            panel = null;
            testingStaffInterface = null;
            IsOpen = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.dedServ || testingStaffInterface == null)
                return;

            if (Main.gameMenu || !Main.LocalPlayer.active)
            {
                IsOpen = false;
                return;
            }

            if (IsOpen)
                testingStaffInterface.Update(gameTime);
        }

        public override void PostUpdateInput()
        {
            if (Main.dedServ || !IsOpen || panel == null)
                return;

            // PostUpdateInput runs after controls are polled but before the
            // player consumes them. Capture both the lock and the list scroll
            // here so the same input cannot change the active hotbar item.
            panel.CaptureScrollInput();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ)
                return;

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex < 0)
                return;

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "SariaMod: Testing Staff UI",
                delegate
                {
                    if (IsOpen && testingStaffInterface != null)
                        testingStaffInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI));
        }

        public static void ToggleUI()
        {
            IsOpen = !IsOpen;
            if (IsOpen)
                panel?.ResetInteractionState();
            SoundEngine.PlaySound(IsOpen ? SoundID.MenuOpen : SoundID.MenuClose);
        }

        public static void CloseUI()
        {
            if (IsOpen)
                SoundEngine.PlaySound(SoundID.MenuClose);
            IsOpen = false;
        }

        public static bool IsMouseOverPanel()
        {
            return IsOpen && panel != null && panel.ContainsMouse();
        }

        public static void CycleProjectile(int direction)
        {
            if (direction != 0)
                SelectProjectile(SelectedProjectileIndex + System.Math.Sign(direction));
        }

        public static bool SelectProjectile(int index, bool playSound = true)
        {
            int wrappedIndex = WrapProjectileIndex(index);
            if (wrappedIndex == SelectedProjectileIndex)
                return false;

            SelectedProjectileIndex = wrappedIndex;
            if (playSound)
                SoundEngine.PlaySound(SoundID.MenuTick);
            return true;
        }

        public static bool AdjustShootSpeed(int amount)
        {
            int previousSpeed = SelectedShootSpeed;
            SelectedShootSpeed += amount;
            if (SelectedShootSpeed == previousSpeed)
                return false;

            SoundEngine.PlaySound(SoundID.MenuTick);
            return true;
        }

        public static bool AdjustDamage(int amount)
        {
            int previousDamage = SelectedDamage;
            SelectedDamage += amount;
            if (SelectedDamage == previousDamage)
                return false;

            SoundEngine.PlaySound(SoundID.MenuTick);
            return true;
        }

        public static string GetProjectileName(int index)
        {
            return ProjectileNames[WrapProjectileIndex(index)];
        }

        public static int GetProjectileType(int index)
        {
            return WrapProjectileIndex(index) switch
            {
                0 => ModContent.ProjectileType<ColdWaveCenter>(),
                1 => ModContent.ProjectileType<Flame>(),
                2 => ModContent.ProjectileType<Locator>(),
                3 => ModContent.ProjectileType<LocatorPellet>(),
                4 => ModContent.ProjectileType<Explosion>(),
                5 => ModContent.ProjectileType<Explosion2>(),
                6 => ModContent.ProjectileType<Explosion3>(),
                _ => ModContent.ProjectileType<ColdWaveCenter>(),
            };
        }

        public static void RestoreSavedSettings(
            FairyPlayer fairyPlayer,
            string projectileName,
            int shootSpeed,
            int damage)
        {
            int savedIndex = System.Array.IndexOf(ProjectileNames, projectileName);
            fairyPlayer.TestingStaffProjectileIndex = savedIndex >= 0 ? savedIndex : 0;
            fairyPlayer.TestingStaffShootSpeed = System.Math.Clamp(shootSpeed, MinimumShootSpeed, MaximumShootSpeed);
            fairyPlayer.TestingStaffDamage = System.Math.Clamp(damage, MinimumDamage, MaximumDamage);
        }

        public static void GetSpawnAI(Player player, int projectileType, out float ai0, out float ai1)
        {
            // Flame normally receives its owner in ai[0]. Explosion receives the
            // Fire Upgrade 1 snapshot in ai[1], matching its normal attack spawn.
            ai0 = projectileType == ModContent.ProjectileType<Flame>() ? player.whoAmI : 0f;
            ai1 = projectileType == ModContent.ProjectileType<Explosion>()
                && player.Fairy().HasEruptionClusterUpgrade
                    ? 1f
                    : 0f;
        }

        private static int WrapProjectileIndex(int index)
        {
            int wrapped = index % ProjectileNames.Length;
            if (wrapped < 0)
                wrapped += ProjectileNames.Length;
            return wrapped;
        }

        private static FairyPlayer GetLocalFairyPlayer()
        {
            if (Main.dedServ || Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers)
                return null;

            Player player = Main.player[Main.myPlayer];
            if (player == null || !player.active)
                return null;

            return player.GetModPlayer<FairyPlayer>();
        }
    }
}
