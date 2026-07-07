using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using SariaMod.Items.Strange;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ObjectData;
using SariaMod.Items;
using SariaMod.Buffs;
using SariaMod.Items.zTalking;
using SariaMod.Dusts;
using SariaMod.Items.Amber;
using SariaMod.Items.Amethyst;
using SariaMod.Items.Bands;
using SariaMod.Items.Emerald;
using SariaMod.Items.Ruby;
using SariaMod.Items.Sapphire;
using SariaMod.Items.Topaz;
using SariaMod.Items.zPearls;
using Terraria.Localization;
namespace SariaMod.Items.Emerald
{
    public class RupeeShardBounce2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            base.DisplayName.SetDefault("Saria");
            Main.projFrames[base.Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[base.Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            base.Projectile.width = 10;
            base.Projectile.height = 10;
            base.Projectile.netImportant = true;
            base.Projectile.friendly = true;
            base.Projectile.ignoreWater = true;
            base.Projectile.usesLocalNPCImmunity = true;
            base.Projectile.localNPCHitCooldown = 7;
            base.Projectile.minionSlots = 0f;
            base.Projectile.extraUpdates = 1;
            base.Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            base.Projectile.timeLeft = 500;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
        public override bool MinionContactDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Lighting.AddLight(Projectile.Center, Color.Silver.ToVector3() * 2f);
            base.Projectile.rotation += 0.25f;
			float speed = 70f;
			float inertia = 20f;
			Vector2 idlePosition = player.Center;
			idlePosition.X += (80 * player.direction) + 5;
			Vector2 direction = idlePosition - Projectile.Center;
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
            // Minion has a target: attack (here, fly towards the enemy)
            float distanceFromTarget = 10f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            if (player.HasMinionAttackTargetNPC && Projectile.timeLeft <= 400)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                // Reasonable distance away so it doesn't target across multiple screens
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }
            if (!foundTarget && Projectile.timeLeft <= 400)
            {
                // This code is required either way, used for finding a target
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && npc.active && (Main.myPlayer == Projectile.owner))
                    {
                        float between = Vector2.Distance(npc.Center, Main.MouseWorld);
                        bool closest = Vector2.Distance(Main.MouseWorld, targetCenter) > between;
                        bool closeThroughWall = between < 1500f;
                        if (((closest) || !foundTarget) && (closeThroughWall))
                            {
                                distanceFromTarget = between;
                                targetCenter = npc.Center;
                                foundTarget = true;
                            }
                    }
                }
            }
            if (foundTarget)
			{
				// The immediate range around the target (so it doesn't latch onto it when close)
				Vector2 direction2 = targetCenter - Projectile.Center;
				direction2.Normalize();
				direction2 *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 2) + direction2) / inertia;
			}
			else if (Projectile.timeLeft > 400 && !foundTarget)
			{
				// Minion doesn't have a target: return to player and idle
				if (distanceToIdlePosition > 600f)
				{
					// Speed up the minion if it's away from the player
					speed = 12f;
					inertia = 60f;
				}
				else
				{
					// Slow down the minion if closer to the player
					speed = 4f;
					inertia = 80f;
				}
				if (distanceToIdlePosition > 20f)
				{
					// The immediate range around the player (when it passively floats about)
					// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				}
				else if (Projectile.velocity == Vector2.Zero)
				{
					// If there is a case where it's not moving at all, give it a little "poke"
					Projectile.velocity.X = -0.15f;
					Projectile.velocity.Y = -0.05f;
				}
			}
		}
    }
}
