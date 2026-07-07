using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Strange;

namespace SariaMod.Netcode.SariaSoundSync
{
    public enum SariaSoundId : byte
    {
        Hover = 1,
        Fly = 2,
        Step1 = 3,
        Step2 = 4,
        IceBarrier1 = 5,
        IceBarrier2 = 6,
    }

    public static class SariaSoundSyncMessage
    {
        internal const byte PacketId = 250;

        internal static void Write(ModPacket packet, Projectile projectile, SariaSoundId soundId)
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
            SariaSoundId soundId = (SariaSoundId)reader.ReadByte();

            PlaySound(owner, identity, soundId);
        }

        internal static void PlaySound(byte owner, short identity, SariaSoundId soundId)
        {
            Projectile proj = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == owner && Main.projectile[i].identity == identity && Main.projectile[i].type == ModContent.ProjectileType<Saria>())
                {
                    proj = Main.projectile[i];
                    break;
                }
            }

            if (proj == null)
                return;

            SoundStyle style = soundId switch
            {
                SariaSoundId.Hover => new SoundStyle("SariaMod/Sounds/Hover"),
                SariaSoundId.Fly => new SoundStyle("SariaMod/Sounds/Fly"),
                SariaSoundId.Step1 => new SoundStyle("SariaMod/Sounds/Step1"),
                SariaSoundId.Step2 => new SoundStyle("SariaMod/Sounds/Step2"),
                SariaSoundId.IceBarrier1 => SoundID.Item20,
                SariaSoundId.IceBarrier2 => SoundID.Item28,
                _ => default
            };

            if (style != default)
                SoundEngine.PlaySound(style, proj.Center);
        }
    }
}
