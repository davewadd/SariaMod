using Microsoft.Xna.Framework;
using SariaMod.Buffs;
using SariaMod.Items.LilHarpy;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using SariaMod.Items.zDinner;
using SariaMod.Gores;
namespace SariaMod.Items.zDinner
{
    public class KingsDinner : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("King's Dinner");
            Tooltip.SetDefault("When they said life in the castle was boring");
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; 
        }
        public int DinnerTime;
        public override void SetDefaults()
        {
            Item.knockBack = 20f;
            Item.width = 160;
            Item.height = 160;
            Item.useTime = 3;
            Item.useAnimation = 15;
            Item.useStyle = 1;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = true;
            Item.noMelee = false;
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.buffTime = 20;
            Item.scale = 2;
            Item.UseSound = new SoundStyle("SariaMod/Sounds/Dinnerswing");
        }
        public override void AddRecipes()
        {
        }
        public override bool AltFunctionUse(Player player)
        {
            // Return true to enable right-click functionality.
            return true;
        }
        public override bool? UseItem(Player player)
        {
            var modPlayer = player.Fairy();
            if (player.altFunctionUse == 2)
            {
                // --- RIGHT-CLICK BEHAVIOR ---
                Item.useStyle = 1;
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.noMelee = true; // Use noMelee to prevent the melee swing on right click
                if (player.whoAmI == Main.myPlayer)
                {
                    // Get the mouse position for the projectile's target
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 direction = mousePosition - player.Center;
                    direction.Normalize();
                    direction *= 15f;
                    // Spawn a specific projectile (e.g., DinnerProjectile1)
                    // Use the older format for NewProjectile
                    Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, direction, ModContent.ProjectileType<DinnerUI>(), player.HeldItem.damage, player.HeldItem.knockBack, player.whoAmI);
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/MenuCursor"), player.Center);
                }
            }
            else
            {
                // --- LEFT-CLICK BEHAVIOR (ADAPTED FROM UseItem) ---
                Item.useStyle = 1;
                Item.useTime = 4;
                Item.useAnimation = 4;
                Item.noMelee = false;
                DinnerTime++;
                if (player.HasBuff(ModContent.BuffType<TriforceofCourage>()))
                {
                    Item.scale = 6.5f;
                    // Ensure the projectile only spawns on the owner's client
                    if (player.whoAmI == Main.myPlayer && modPlayer.KingsDinnerCooldownTimer >= FairyPlayer.KingsDinnerResetTime)
                    {
                        Vector2 mousePosition = Main.MouseWorld;
                        Vector2 direction = mousePosition - player.Center;
                        direction.Normalize();
                        direction *= 10f;
                        // Spawn the projectile
                        // Use the older format for NewProjectile
                        Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, direction, ModContent.ProjectileType<Dinner>(), player.HeldItem.damage, player.HeldItem.knockBack, player.whoAmI);
                        
                        DinnerTime = 0;
                        modPlayer.ResetKingsDinnerTimer();
                    }
                }
                else if (!player.HasBuff(ModContent.BuffType<TriforceofCourage>()))
                {
                    Item.scale = 2;
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 direction = mousePosition - player.Center;
                    direction.Normalize();
                    direction *= 20f;
                    if (Main.rand.NextBool(50))
                    {
                        // Ensure the projectile only spawns on the owner's client
                        if (player.whoAmI == Main.myPlayer)
                        {
                            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, direction, ModContent.ProjectileType<Dinner2>(), player.HeldItem.damage, player.HeldItem.knockBack, player.whoAmI);
                           
                        }
                    }
                    else if (player.whoAmI == Main.myPlayer && modPlayer.KingsDinnerCooldownTimer >= FairyPlayer.KingsDinnerResetTime)
                    {
                        Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, direction, ModContent.ProjectileType<Dinner2>(), player.HeldItem.damage, player.HeldItem.knockBack, player.whoAmI);
                        modPlayer.ResetKingsDinnerTimer();
                    }
                }
                if ((player.ownedProjectileCounts[ModContent.ProjectileType<ReflectingProjectile>()] <= 0f))
                {
                    // This spawns the projectile
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center,Vector2.Zero, ModContent.ProjectileType<ReflectingProjectile>(), 0, 0f,player.whoAmI);
                }
                // Check if DinnerTime is over 100
                if (DinnerTime >= 100)
                {
                    // Give it a chance to shoot the projectile
                    if (Main.rand.NextBool(20))
                    {
                        // Ensure the projectile only spawns on the owner's client
                        if (player.whoAmI == Main.myPlayer)
                        {
                            Vector2 mousePosition = Main.MouseWorld;
                            Vector2 direction = mousePosition - player.Center;
                            direction.Normalize();
                            direction *= 10f;
                            // Spawn the projectile
                            // Use the older format for NewProjectile
                            Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), player.Center, direction, ModContent.ProjectileType<Dinner>(), player.HeldItem.damage, player.HeldItem.knockBack, player.whoAmI);
                            // Reset DinnerTime after successfully spawning the projectile
                            DinnerTime = 0;
                            modPlayer.ResetKingsDinnerTimer();
                        }
                    }
                }
            }
            return base.CanUseItem(player);
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            FairyPlayer modPlayer = player.Fairy();
            if (player.HasBuff(ModContent.BuffType<TriforceofCourage>()) && player.whoAmI == Main.myPlayer)
            {
                Vector2 direction2 = target.Center;
                direction2.Normalize();
                direction2 *= 0f;
                int projectileType2;
                projectileType2 = ModContent.ProjectileType<DinnerBomb>();
                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, direction2, projectileType2, Item.damage, Item.knockBack, player.whoAmI);
            }
            if (player.whoAmI == Main.myPlayer)
            {
                if (modPlayer.Serving <= 99)
                {
                    modPlayer.Serving += 1;
                }
                SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/KinglyWhack"), player.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    int rand = Main.rand.Next(3); // Get a random number from 0 to 2
                    int goreType;
                        if (rand == 0)
                        {
                            goreType = ModContent.GoreType<MessyDinner1>();
                        }
                        else if (rand == 1)
                        {
                            goreType = ModContent.GoreType<MessyDinner2>();
                        }
                        else // rand == 2
                        {
                            goreType = ModContent.GoreType<MessyDinner3>();
                        }
                    for (int D = 0; D < 2; D++)
                    {
                        // Spawn the gore at the NPC's position with a random velocity
                        Gore.NewGore(player.GetSource_OnHit(target), target.position,new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)),goreType, 2);
                    }
                }
                bool existingProjectile = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI)
                    {
                        int type = p.type;
                        if (type == ModContent.ProjectileType<DinnerSoundHit1>() || type == ModContent.ProjectileType<DinnerSoundHit2>() ||  type == ModContent.ProjectileType<DinnerSoundHit3>())
                        {
                            existingProjectile = true;
                            break;
                        }
                    }
                }
                // If no matching projectile is found and we have a rare chance, spawn one
                // Example: 1 in 20 chance
                if (!existingProjectile && Main.rand.NextBool(20))
                {
                    // Get the mouse position for the projectile's target
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 direction = mousePosition - player.Center;
                    direction.Normalize();
                    direction *= 10f; // Set the projectile speed
                    // Select one of the three projectiles at random
                    int rand = Main.rand.Next(3);
                    int projectileType;
                    if (rand == 0)
                    {
                        projectileType = ModContent.ProjectileType<DinnerSoundHit1>();
                    }
                    else if (rand == 1)
                    {
                        projectileType = ModContent.ProjectileType<DinnerSoundHit2>();
                    }
                    else
                    {
                        projectileType = ModContent.ProjectileType<DinnerSoundHit3>();
                    }
                    // Spawn the projectile
                    Projectile.NewProjectile(player.GetSource_OnHit(target),target.Center, direction,projectileType, Item.damage,Item.knockBack,player.whoAmI );
                }
                if (player.whoAmI == Main.myPlayer && target.life <= 0 && !target.boss)
                {
                    // Play the "YouWillDie" sound effect at the NPC's center.
                    // Replace "SariaMod/Sounds/YouWillDie" with the correct path to your sound file.
                    SoundEngine.PlaySound(new SoundStyle("SariaMod/Sounds/YouWillDie"), target.Center);
                }
            }
        }
    }
}