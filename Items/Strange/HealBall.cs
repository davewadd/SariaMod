using Microsoft.Xna.Framework;
using SariaMod.Diagnostics;
using SariaMod.Items.zPearls;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod;
using SariaMod.Items.zTalking;
namespace SariaMod.Items.Strange
{
    public class HealBall : ModItem
    {
        public int RightClickTimer;
        public bool RightClickShort;
        public bool RightCharging;
        private bool wasRightClickShortLastFrame;
        private bool hasUntargetedThisHold; // Track if we've cleared targeting when crossing threshold
        private bool _middleClickConsumed;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("HealBall");
            Tooltip.SetDefault(SariaModUtilities.ColorMessage("Calls on Saria, the Champion of Foresight!", new Color(135, 206, 180)) + "\n" + SariaModUtilities.ColorMessage("Requires 3 minion slots to summon but doens't occupy the slots", new Color(50, 200, 250)) + "\n" + SariaModUtilities.ColorMessage("Saria will level up as you battle with her!", new Color(0, 200, 250, 200)) + "\n " + SariaModUtilities.ColorMessage("As she levels up, she learns new attacks and gives added buffs depending on what biome she is in", new Color(0, 200, 250, 200)) + "\n" + SariaModUtilities.ColorMessage("Left Click to change available forms, Hold left to start a charged attack!", new Color(0, 20, 250, 200)) + "\n " + SariaModUtilities.ColorMessage("You must hold the Pokeball for Saria to target enemies!", new Color(0, 20, 250, 200)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.knockBack = 13f;
            Item.width = 32;
            Item.height = 32;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 0, 0, 0);
            Item.rare = ItemRarityID.Master;
            Item.shootSpeed = 8;
            Item.noUseGraphic = true;
            Item.channel = true;
            // These below are needed for a minion weapon
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            // No buffTime because otherwise the item tooltip would say something like "1 minute duration"
            Item.shoot = ModContent.ProjectileType<Transform>();
        }
        public override void HoldItem(Player player)
        {
            // Mouse 3 toggle: flip LinkCable and camera focus on Saria
            if (Main.myPlayer == player.whoAmI)
            {
                if (Main.mouseMiddle)
                {
                    if (!_middleClickConsumed)
                    {
                        var fp = player.GetModPlayer<FairyPlayer>();
                        fp.LinkCable = !fp.LinkCable;
                        if (!fp.LinkCable)
                            fp.LinkCableTarget = Vector2.Zero;
                        _middleClickConsumed = true;

                        // Mark the exact transition in the spawn log so every SpawnRate/
                        // SpawnGate window can be attributed to the right mode. Splits the
                        // "vanilla path misbehaving" evidence from the "split path
                        // misbehaving" evidence when reading debugsaria.txt.
                        SariaDebug.Log("SpawnGate",
                            $"LinkCable toggled {(fp.LinkCable ? "ON — split spawning engages (player region + Saria region)" : "OFF — vanilla spawning resumes (no Saria influence)")} " +
                            $"by player[{player.whoAmI}] \"{player.name}\"",
                            fp.LinkCable ? Color.Lime : Color.Orange);

                        // Multiplayer: the SERVER runs NPC.SpawnNPC, so it must learn the
                        // new LinkCable state or split spawning never engages there. The
                        // server relays to all other clients (see SyncLinkCable handler).
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            var packet = SariaMod.Instance.GetPacket();
                            packet.Write((byte)SariaMod.SoundMessageType.SyncLinkCable);
                            packet.Write(player.whoAmI);
                            packet.Write(fp.LinkCable);
                            packet.Send();
                        }
                    }
                }
                else
                {
                    _middleClickConsumed = false;
                }
            }

            // While LinkCable is active, right-click is reserved for marker placement, so
            // suppress vanilla minion targeting. Terraria sets MinionAttackTargetNPC
            // automatically on right-click (it bypasses AltFunctionUse), so clear it every
            // frame to guarantee Saria never targets enemies while being guided.
            if (player.GetModPlayer<FairyPlayer>().LinkCable)
            {
                player.MinionAttackTargetNPC = -1;
            }

            // Reset bools each frame
            RightClickShort = false;
            RightCharging = false;

            // Track right mouse button
            if (Main.mouseRight)
            {
                RightClickTimer++;

                // Z-Target Window: timer > 24
                if (RightClickTimer > 24)
                {
                    RightCharging = true;

                    // Clear minion target exactly once when crossing threshold (at frame 25)
                    if (!hasUntargetedThisHold)
                    {
                        player.MinionAttackTargetNPC = -1;
                        hasUntargetedThisHold = true;
                    }
                }
                // While in Targeting Window (1-24), do NOT set any target yet - wait for release
            }
            else
            {
                // Right mouse released - check if it was a short click (Targeting Window)
                if (RightClickTimer >= 1 && RightClickTimer <= 24)
                {
                    RightClickShort = true;

                    // LinkCable mode: short right-click places the navigation marker at the cursor.
                    // Only when the cable is active, we are the local player, and Saria is not cursed.
                    if (Main.myPlayer == player.whoAmI)
                    {
                        var fp = player.GetModPlayer<FairyPlayer>();
                        if (fp.LinkCable)
                        {
                            bool sariaCursed = false;
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile proj = Main.projectile[i];
                                if (proj.active && proj.owner == player.whoAmI && proj.ModProjectile is Saria s)
                                {
                                    sariaCursed = s.CursedValue;
                                    break;
                                }
                            }
                            if (!sariaCursed)
                            {
                                // Snap the click position to the nearest walkable tile footprint.
                                // Uses the same footprint size as Saria's A* pathfinder (2x3 tiles).
                                const int fw = 2; // FollowPathFootprintWidth
                                const int fh = 3; // FollowPathFootprintHeight
                                Vector2 cursor = Main.MouseWorld;

                                // Convert world position to tile origin (top-left of footprint).
                                int tileOriginX = (int)Math.Round(cursor.X / 16f - fw * 0.5f);
                                int tileOriginY = (int)Math.Round(cursor.Y / 16f - fh * 0.5f);
                                var tileOrigin  = new Microsoft.Xna.Framework.Point(tileOriginX, tileOriginY);

                                Microsoft.Xna.Framework.Point validTile;
                                if (SariaPathfinder.FootprintFits(tileOrigin.X, tileOrigin.Y, fw, fh))
                                {
                                    validTile = tileOrigin;
                                }
                                else
                                {
                                    validTile = SariaPathfinder.NudgeToOpen(tileOrigin, fw, fh);
                                }

                                // Only place the marker if a valid spot was found.
                                if (validTile.X != int.MinValue)
                                {
                                    fp.LinkCableTarget = new Vector2(
                                        (validTile.X + fw * 0.5f) * 16f,
                                        (validTile.Y + fh * 0.5f) * 16f);
                                }
                                // else: no valid spot nearby — silently ignore the click
                            }
                        }
                    }
                }

                // Reset timer and flags on release
                RightClickTimer = 0;
                hasUntargetedThisHold = false;
            }

            // Update the flag for RightClickShort from the previous frame
            wasRightClickShortLastFrame = RightClickShort;
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, Color.LightPink.ToVector3() * 2f);

            // Reset state when item is on the ground (not being held)
            RightClickTimer = 0;
            RightClickShort = false;
            RightCharging = false;
            hasUntargetedThisHold = false;
            _middleClickConsumed = false;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.HasBuff(ModContent.BuffType<SariaBuff>()) && !SariaUISystem.IsDialogueActive)
            {
                return true;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<HealBallProjectile>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<HealBallProjectile2>()] > 0f || player.ownedProjectileCounts[ModContent.ProjectileType<ReturnBall>()] > 0f)
            {
                return false;
            }
            else if (!(player.HasBuff(ModContent.BuffType<SariaBuff>())))
            {
                return true;
            }
            return false;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            int owner = player.whoAmI;
            if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] <= 0f))
            {
                if (player.direction == -1)
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, velocity.X, velocity.Y, ModContent.ProjectileType<HealBallProjectile>(), damage, 0f, player.whoAmI);
                }
                else if (player.direction == 1)
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, velocity.X, velocity.Y, ModContent.ProjectileType<HealBallProjectile>(), damage, 0f, player.whoAmI);
                }
            }
            else if (player.altFunctionUse != 2 && (player.ownedProjectileCounts[ModContent.ProjectileType<Saria>()] > 0f))
            {
                for (int i = 0; i < 1000; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].ModProjectile is Saria modProjectile && ((Main.projectile[i].owner == owner)))
                    {
                        if (modProjectile.ChangeForm <= 0)
                        {
                            Projectile.NewProjectile(Item.GetSource_FromThis(), position.X + 0, position.Y + 0, 0, 0, ModContent.ProjectileType<Transform>(), damage, 0f, player.whoAmI);
                        }
                    }
                }
            }
            return false;
        }
        public override bool AltFunctionUse(Player player)
        {
            // Disable NPC targeting entirely while LinkCable is active — right-click
            // is used for marker placement in that mode, not combat targeting.
            if (player.GetModPlayer<FairyPlayer>().LinkCable)
                return false;

            // Only allow right-click (alt function) targeting if RightClickShort is true
            if (!RightClickShort)
                return false;

            return true;
        }
        public override void AddRecipes()
        {
            {
                Recipe recipe = CreateRecipe();
                recipe.AddIngredient(ItemID.Glass, 3);
                recipe.AddRecipeGroup("IronBar", 3);
                recipe.AddIngredient(ItemID.ManaCrystal, 3);
                recipe.AddIngredient(ModContent.ItemType<XpPearl>(), 3);
                recipe.AddTile(ModContent.TileType<Tiles.StrangeBookcase>());
                recipe.Register();
            }
        }
    }
}