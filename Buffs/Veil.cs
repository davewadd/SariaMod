using Microsoft.Xna.Framework;
using SariaMod.Dusts;
using SariaMod.Netcode;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Buffs
{
    /*
	 * This file contains all the code necessary for a minion
	 * - ModItem
	 *     the weapon which you use to summon the minion with
	 * - ModBuff
	 *     the icon you can click on to despawn the minion
	 * - ModProjectile 
	 *     the minion itself
	 *     
	 * It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overwiew.
	 * This is NOT an in-depth guide to advanced minion AI
	 */
    public class Veil : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Veil");
            Description.SetDefault("An aqua veil surrounds You!\n Can keep you safe from the heat of hell, Space and will allow you to enter Lava\nYou can now also breath in water\n \nYou will Freeze solid in cold Biomes!!");
            Main.debuff[base.Type] = false;
            Main.buffNoSave[base.Type] = true;
            Main.buffNoTimeDisplay[base.Type] = false;
        }
        private const int sphereRadius = 30;
        public override void Update(Player player, ref int buffIndex)
        {
            // Get the per-player VeilPlayer to track freeze state
            VeilPlayer veilPlayer = player.GetModPlayer<VeilPlayer>();
            veilPlayer.hasVeilBuff = true;
            
            bool Warm = player.behindBackWall && player.HasBuff(BuffID.Campfire);
            bool immunityToCold = player.HasBuff(BuffID.Warmth) || player.HasBuff(BuffID.OnFire) || player.arcticDivingGear;
            
            // Reset freeze state when buff is first applied (buffTime near max)
            if (player.buffTime[buffIndex] == 10798)
            {
                veilPlayer.veilFreezeState = 0;
            }
            
            // Apply water-related effects when not frozen
            if (veilPlayer.veilFreezeState == 0)
            {
                player.waterWalk = true;
                player.gills = true;
                player.accFlipper = true;
                player.ignoreWater = true;
            }
            
            // Always apply lava immunity
            player.lavaImmune = true;
            player.fireWalk = true;
            player.lavaTime = 180000;
            
            // Thaw if player has immunity to cold or is warm
            if (immunityToCold == true || Warm == true)
            {
                if (veilPlayer.veilFreezeState > 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), player.Center);
                    veilPlayer.veilFreezeState = 0;
                    
                    // Sync the unfrozen state
                    if (Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.SinglePlayer)
                    {
                        PlayerDebuffSyncNetworking.SendVeilFreezeState(player.whoAmI, false);
                    }
                }
            }
            
            // Lava visual effects
            if (player.lavaWet)
            {
                if (Main.rand.NextBool(80))
                {
                    SoundEngine.PlaySound(SoundID.LiquidsHoneyLava, player.Center);
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<BubbleDust2>(), speed * -2, Scale: 2.7f);
                        d.noGravity = true;
                    }
                }
            }
            
            // Underworld visual effects
            if (player.ZoneUnderworldHeight)
            {
                if (Main.rand.NextBool(80))
                {
                    SoundEngine.PlaySound(SoundID.LiquidsHoneyLava, player.Center);
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust d = Dust.NewDustPerfect(player.Center, ModContent.DustType<BubbleDust2>(), speed * -2, Scale: 2.7f);
                        d.noGravity = true;
                    }
                }
            }
            
            // Check if player is in a cold biome
            bool inColdBiome = player.ZoneSnow || player.ZoneSkyHeight;
            
            // FIXED: Use player.ZoneSkyHeight instead of Main.player[Main.myPlayer].ZoneSkyHeight
            // This was causing the freeze to only check the local player's zone instead of the actual player with the buff
            if (inColdBiome)
            {
                if (immunityToCold != true && Warm != true)
                {
                    // Play freeze sound when first freezing
                    if (veilPlayer.veilFreezeState == 0)
                    {
                        SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HardIce"), player.Center);
                        veilPlayer.veilFreezeState = 1;
                        
                        // Sync the frozen state
                        if (Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.SinglePlayer)
                        {
                            PlayerDebuffSyncNetworking.SendVeilFreezeState(player.whoAmI, true);
                        }
                    }
                    player.frozen = true;
                }
            }
            else
            {
                // Player left the cold biome - thaw them but keep the Veil buff
                if (veilPlayer.veilFreezeState > 0)
                {
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/mist"), player.Center);
                    veilPlayer.veilFreezeState = 0;
                    
                    // Sync the unfrozen state
                    if (Main.myPlayer == player.whoAmI && Main.netMode != NetmodeID.SinglePlayer)
                    {
                        PlayerDebuffSyncNetworking.SendVeilFreezeState(player.whoAmI, false);
                    }
                }
            }
            
            // Bubble visual effects when not frozen
            if (veilPlayer.veilFreezeState == 0)
            {
                if (Main.rand.NextBool(800))
                {
                    SoundEngine.PlaySound(SoundID.Drown, player.Center);
                }
                if (Main.rand.NextBool(4))
                {
                    float radius = (float)Math.Sqrt(Main.rand.Next(sphereRadius * sphereRadius));
                    double angle = Main.rand.NextDouble() * 2.0 * Math.PI;
                    Dust.NewDust(new Vector2(player.Center.X + radius * (float)Math.Cos(angle), player.Center.Y + radius * (float)Math.Sin(angle)), 0, 0, ModContent.DustType<BubbleDustSmall>(), 0f, 0f, 0, default(Color), 1.5f);
                }
            }
        }
    }
}