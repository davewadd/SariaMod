using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SariaMod.Items.Bands; 
using SariaMod.Items.zPearls; 
using SariaMod.Items.Amber; // Potentially needed
using SariaMod.Items.zBookcases; // For notes
using SariaMod.Items.zDinner; // For KingsDinner
using SariaMod.Items.Emerald; // For Shards

namespace SariaMod
{
    // This class contains recipes that are suspected to be for testing/debugging purposes.
    // The user intends to remove this class before publishing.
    public class TestItemRecipes : ModSystem
    {
        public override void AddRecipes()
        {
            // Hookshot
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<HookShot>());
                recipe.AddIngredient(ItemID.DirtBlock, 1);
                recipe.AddTile(TileID.WorkBenches);
                recipe.Register();
            }

            // Longshot
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<Longshot>());
                recipe.AddIngredient(ModContent.ItemType<HookShot>(), 1);
                recipe.AddIngredient(ItemID.DirtBlock, 10);
                recipe.AddTile(TileID.WorkBenches);
                recipe.Register();
            }

            // BirdieRattle
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<BirdieRattle>());
                recipe.AddIngredient(ItemID.Feather, 5);
                recipe.Register();
            }

            // CalmingCandle
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<CalmingCandle>(), 1);
                recipe.Register();
            }

            // ReajCandle
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<ReajCandle>(), 1);
                recipe.Register();
            }

            // RainOcarina
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<RainOcarina>());
                recipe.Register();
            }

            // PaperNote
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<PaperNote>(), 1);
                recipe.Register();
            }

            // XPStaffNote
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<XPStaffNote>(), 1);
                recipe.Register();
            }

            // KingsDinner
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<KingsDinner>());
                recipe.Register();
            }

            // TMForget
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<TMForget>(), 5);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<OcarinaOfTime>());
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<OasisOcarina>());
                recipe.Register();
            }
            // FairyScarf
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<FairyScarf>());
                recipe.AddIngredient(ItemID.DirtBlock, 1);
                recipe.Register();
            }

            // DerpyRod
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<Items.Strange.DerpyRod>());
                recipe.Register();
            }

            // FeelingRod
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<Items.Strange.FeelingRod>());
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ModContent.ItemType<Items.Strange.TestingStaff>());
                recipe.Register();
            }
            // PearlWood
            {
                Recipe recipe = Recipe.Create(ItemID.Pearlwood, 100);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.MagicConch, 1);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.SandBlock, 100);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.ScarabBomb, 100);
                recipe.Register();
            }
            {
                Recipe recipe = Recipe.Create(ItemID.DrillContainmentUnit, 1);
                recipe.Register();
            }
        }
    }
}
