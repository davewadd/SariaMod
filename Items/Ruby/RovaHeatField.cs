using System;
using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.TileGlow;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Ruby
{
    /// <summary>
    /// One invisible damage controller for all damaging heated tiles owned by
    /// a player. Its broad hitbox stretches across the complete heat field,
    /// while Colliding checks the actual heated tiles so gaps never deal damage.
    /// </summary>
    public class RovaHeatField : ModProjectile
    {
        private int cachedCollisionTick = -1;
        private Rectangle cachedTargetHitbox;
        private bool cachedCollisionResult;
        private TileHeatData cachedCollisionHeat;

        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rova Heat Field");
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.netImportant = true;
            Projectile.hide = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = TileHeatManager.HeatDamageIntervalTicks;
            Projectile.minionSlots = 0f;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            int owner = Projectile.owner;
            if (owner < 0 || owner >= Main.maxPlayers)
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = Vector2.Zero;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                Projectile.timeLeft = 60;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanDamage()
        {
            // Colliding performs the cheap target-local heat lookup. Avoid a
            // full heat-field bounds scan here because tModLoader can ask this
            // hook repeatedly while processing many NPCs.
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return TryGetCollisionHeat(targetHitbox, out _);
        }

        public override void ModifyHitNPC(
            NPC target,
            ref int damage,
            ref float knockback,
            ref bool crit,
            ref int hitDirection)
        {
            if (!TryGetCollisionHeat(target.Hitbox, out TileHeatData heat))
            {
                damage = 0;
                return;
            }

            damage = Math.Max(1, heat.Damage / 5);
            knockback = 0f;
            crit = false;
            hitDirection = 0;
            target.buffImmune[ModContent.BuffType<Burning2>()] = false;
            target.AddBuff(ModContent.BuffType<Burning2>(), 16);
            target.GetGlobalNPC<FairyGlobalNPC>().RovaBurnedHit = true;
        }

        private bool TryGetCollisionHeat(Rectangle targetHitbox, out TileHeatData heat)
        {
            int currentTick = (int)Main.GameUpdateCount;
            if (cachedCollisionTick == currentTick && cachedTargetHitbox.Equals(targetHitbox))
            {
                heat = cachedCollisionHeat;
                return cachedCollisionResult;
            }

            cachedCollisionTick = currentTick;
            cachedTargetHitbox = targetHitbox;
            cachedCollisionResult = TileHeatManager.TryGetHottestOwnedHeatInArea(
                targetHitbox,
                TileHeatManager.HeatCollisionOverhangPixels,
                Projectile.owner,
                TileHeatManager.HeatDamageIntensityThreshold,
                out cachedCollisionHeat,
                out _);
            heat = cachedCollisionHeat;
            return cachedCollisionResult;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
