﻿using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using FargowiltasSouls.Projectiles.BossWeapons;

namespace FargowiltasSouls.Items.Weapons.SwarmDrops
{
    public class DragonBreath2 : SoulsItem
    {
        public int skullTimer;

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            DisplayName.SetDefault("Dragon's Demise");
            Tooltip.SetDefault(@"Uses gel for ammo
66% chance to not consume ammo
'The reward for slaughtering many..'");
        }

        public override void SetDefaults()
        {
            Item.damage = 160;
            Item.knockBack = 1f;
            Item.shootSpeed = 12f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useAnimation = 30;
            Item.useTime = 3;
            Item.width = 54;
            Item.height = 14;
            Item.shoot = ModContent.ProjectileType<HellFlame>();
            Item.useAmmo = AmmoID.Gel;
            Item.UseSound = SoundID.DD2_BetsyFlameBreath;

            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 15);
            Item.rare = ItemRarityID.Purple;
            Item.DamageType = DamageClass.Ranged;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position + Vector2.Normalize(velocity) * 60f, velocity, type, damage, knockback, player.whoAmI);
            if (--skullTimer < 0)
            {
                skullTimer = 5;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot);
                //float ai = Main.rand.NextFloat((float)Math.PI * 2);
                /*for (int i = 0; i <= 4; i++)
                {
                    int p = Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedByRandom(MathHelper.Pi / 18),
                        ModContent.ProjectileType<DragonFireball>(), damage * 3, knockBack, player.whoAmI);
                    Main.projectile[p].netUpdate = true;
                }*/
                Projectile.NewProjectile(source, position, 2f * velocity,//.RotatedByRandom(MathHelper.Pi / 18),
                    ModContent.ProjectileType<DragonFireball>(), damage, knockback * 6f, player.whoAmI);
            }
            return false;
        }

        public override bool CanConsumeAmmo(Player player)
        {
            return Main.rand.NextBool(3);
        }

        //make them hold it different
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-30, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(null, "DragonBreath")
            .AddIngredient(null, "AbomEnergy", 10)
            .AddIngredient(ModContent.Find<ModItem>("Fargowiltas", "EnergizerBetsy"))
            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))
            
            .Register();
        }
    }
}