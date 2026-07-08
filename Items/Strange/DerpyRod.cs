using Microsoft.Xna.Framework;
using SariaMod.Netcode;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SariaMod.Items.Strange
{
    public class DerpyRod : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("DerpyRod");
            Tooltip.SetDefault(MiscUtilities.ColorMessage("Left-click: spawn a chilled Derpling (broken-out state)\nRight-click: spawn a frozen Hoplite", new Color(0, 200, 250, 200)));
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.knockBack = 13f;
            Item.mana = 1;
            Item.width = 32;
            Item.height = 32;
            base.Item.useTime = (base.Item.useAnimation = 10);
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.damage = 80;
            Item.DamageType = DamageClass.Summon;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return true;

            // Only the server (or singleplayer host) spawns NPCs authoritatively
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return true;

            Vector2 spawnPos = Main.MouseWorld;
            int npcType = player.altFunctionUse == 2 ? 481 : NPCID.Derpling;

            int npcIndex = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPos.X, (int)spawnPos.Y, npcType);
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
            {
                NPC npc = Main.npc[npcIndex];
                npc.aiStyle = 0;
                npc.velocity = Vector2.Zero;
                npc.netUpdate = true;
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
                FrozenGoreMarkingNetworking.SendMarkNPCFrozen(npcIndex);
            }

            return true;
        }
    }
}
