using SariaMod.Buffs;
using SariaMod.Gores;
using SariaMod.Netcode;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Sapphire
{
    internal static partial class SapphireColdStatus
    {
        internal const int ColdWaveHardFreezeMinimumTimeLeft = 200;

        public static bool IsIceAttack(Projectile projectile)
        {
            bool isIceAttack = projectile.ModProjectile is Bubble
                or Bubble2
                or ColdWaveCenter
                or ColdWaveHitBox
                or IceBarrier
                or HealBubble;

            // TestItemRecipes.cs can add temporary test sources here. Because this is
            // an optional partial method, deleting that file also deletes the behavior.
            IncludeTestIceAttacks(projectile, ref isIceAttack);
            return isIceAttack;
        }

        static partial void IncludeTestIceAttacks(Projectile projectile, ref bool isIceAttack);

        public static bool PreservesHardFreeze(Projectile projectile)
        {
            return projectile.ModProjectile is ColdWaveCenter
                or ColdWaveHitBox
                or IceBarrier
                or HealBubble;
        }

        public static void ApplyFromProjectileHit(Projectile projectile, NPC target)
        {
            if (!IsIceAttack(projectile) || projectile.damage <= 0 || Main.netMode == NetmodeID.Server)
                return;

            bool isLocalOwner = Main.netMode == NetmodeID.SinglePlayer || projectile.owner == Main.myPlayer;
            if (!isLocalOwner)
                return;

            int frozenBuffType = ModContent.BuffType<EnemyFrozen>();
            if (target.HasBuff(frozenBuffType))
            {
                FrozenNPCVisualManager.MarkNPCAsFrozenLocal(target.whoAmI);
                return;
            }

            bool startsHardFreeze = !target.boss
                && (projectile.ModProjectile is IceBarrier
                    || projectile.ModProjectile is HealBubble
                    || projectile.ModProjectile is ColdWaveHitBox
                        && projectile.timeLeft >= ColdWaveHardFreezeMinimumTimeLeft);

            if (startsHardFreeze)
            {
                StartHardFreeze(projectile, target, frozenBuffType);
                return;
            }

            FrozenNPCVisualManager.MarkNPCAsChilledLocal(target.whoAmI);
            FrozenNPCNetworking.SendSyncTimer(target.whoAmI, 0);
        }

        private static void StartHardFreeze(Projectile projectile, NPC target, int frozenBuffType)
        {
            target.buffImmune[frozenBuffType] = false;
            if (target.TryGetGlobalNPC(out FairyGlobalNPC fairyNPC))
                fairyNPC.SetFreezeInitiator(projectile.owner);

            bool quietLocalPrediction = Main.netMode == NetmodeID.MultiplayerClient;
            target.AddBuff(frozenBuffType, EnemyFrozen.MaximumBuffTime, quietLocalPrediction);
            FrozenNPCVisualManager.MarkNPCAsFrozenLocal(target.whoAmI);
            SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/HardIce"), target.Center);
            FrozenNPCNetworking.SendFreezeNPC(target.whoAmI, projectile.owner);
        }
    }
}
