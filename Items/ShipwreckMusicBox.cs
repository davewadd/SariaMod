using SariaMod.Tiles;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
namespace SariaMod.Items
{
    public class ShipwreckMusicBox : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(" Shipwreck Music Box  ");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Shipwreck"), ModContent.ItemType<ShipwreckMusicBox>(), ModContent.TileType<ShipwreckMusicBoxTile>());
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.ShipwreckMusicBoxTile>();
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.value = 100000;
            Item.accessory = true;
        }
        public override void AddRecipes()
        {
            Recipe modRecipe = /* base */Recipe.Create(this.Type);
            modRecipe.AddIngredient(ItemID.MusicBox, 1);
            modRecipe.AddTile(TileID.WorkBenches);
            modRecipe.Register();
        }
    }
}
