using Microsoft.Xna.Framework;
using SariaMod.Items.Strange;
using SariaMod.Netcode.SariaSoundSync;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Ruby;

namespace SariaMod.Netcode.FireSoundSync
{
    public enum FireSoundId : byte
    {
        Bomb = 1,
        Item116 = 2,
        Dragon = 3,
    }

    public static class FireSoundSyncMessage
    {
        internal const byte PacketId = 249; // 250=Saria, 251=IceDome, 252=FrozenGore/TileGlow, 253=FrozenTimer, 254=Hookshot

        /// <summary>
        /// Sends a fire sound sync packet. Call this from the projectile owner.
        /// </summary>
        internal static void Write(ModPacket packet, Projectile projectile, FireSoundId soundId)
        {
            packet.Write(PacketId);
            packet.Write((byte)projectile.owner);
            packet.Write((short)projectile.identity);
            packet.Write((byte)soundId);
        }

        internal static void Receive(BinaryReader reader)
        {
            byte owner = reader.ReadByte();
            short identity = reader.ReadInt16();
            FireSoundId soundId = (FireSoundId)reader.ReadByte();

            PlaySound(owner, identity, soundId);
        }

        internal static void PlaySound(byte owner, short identity, FireSoundId soundId)
        {
            Projectile proj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == owner && Main.projectile[i].identity == identity && ((Main.projectile[i].type == ModContent.ProjectileType<Explosion>()) || (Main.projectile[i].type == ModContent.ProjectileType<Explosion2>())))
                {
                    proj = Main.projectile[i];
                    break;
                }
            }

            if (proj == null)
                return;

            SoundStyle style = soundId switch
            {
                FireSoundId.Bomb => new SoundStyle("SariaMod/Sounds/Bomb"),
                FireSoundId.Item116 => SoundID.Item116,
                FireSoundId.Dragon => SoundID.DD2_SkyDragonsFuryShot,
                _ => default
            };

            if (style != default)
                SoundEngine.PlaySound(style, proj.Center);
        }
    }
}

