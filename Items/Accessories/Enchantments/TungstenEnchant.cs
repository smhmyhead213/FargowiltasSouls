using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using FargowiltasSouls.Toggler;
using FargowiltasSouls.Projectiles;

namespace FargowiltasSouls.Items.Accessories.Enchantments
{
    public class TungstenEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Tungsten Enchantment");
            
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "钨魔石");
            
            string tooltip =
@"150% increased weapon size
Every half second a projectile will be doubled in size
Enlarged swords and projectiles deal 10% more damage and have an additional chance to crit
'Bigger is always better'";
            Tooltip.SetDefault(tooltip);

            string tooltip_ch =
@"增加150%剑的尺寸
每过0.5秒便会使一个弹幕的尺寸翻倍
尺寸变大的剑和弹幕会额外造成10%伤害并且有额外几率暴击
'大就是好'";
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, tooltip_ch);
        }

        protected override Color nameColor => new Color(176, 210, 178);

        public override void SetDefaults()
        {
            base.SetDefaults();
            
            Item.rare = ItemRarityID.Blue;
            Item.value = 40000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            TungstenEffect(player);
        }

        public static void TungstenEffect(Player player)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            modPlayer.TungstenEnchantActive = true;
        }

        public static void TungstenIncreaseWeaponSize(Item item, FargoSoulsPlayer modPlayer)
        {
            modPlayer.Player.GetAttackSpeed(DamageClass.Melee) -= 0.5f;

            float tungstenScale = modPlayer.TerraForce ? 4f : 2.5f;

            //if (heldItem.damage > 0 && !heldItem.noMelee)
            //{
            modPlayer.TungstenPrevSizeSave = item.scale;
            item.scale *= tungstenScale;
            //}
            //else if (((modPlayer.Toggler != null && !player.GetToggleValue("Tungsten", false)) || !TungstenEnchant) && modPlayer.TungstenPrevSizeSave != -1)
            //{
            //    heldItem.scale = modPlayer.TungstenPrevSizeSave;
            //}
        }

        public static bool TungstenAlwaysAffectProj(Projectile projectile)
        {
            return projectile.aiStyle == ProjAIStyleID.Spear 
                || projectile.aiStyle == ProjAIStyleID.Yoyo 
                || projectile.aiStyle == ProjAIStyleID.ShortSword 
                || ProjectileID.Sets.IsAWhip[projectile.type]
                || projectile.type == ProjectileID.MonkStaffT2 
                || projectile.type == ProjectileID.Arkhalis 
                || projectile.type == ProjectileID.Terragrim
                || projectile.type == ProjectileID.PiercingStarlight;
        }

        public static bool TungstenCanAffectProj(Projectile projectile)
        {
            return projectile.friendly 
                && projectile.aiStyle != 99
                && projectile.damage != 0
                && !projectile.npcProj
                && !projectile.trap 
                && !(FargoSoulsUtil.IsSummonDamage(projectile, true, false) && !ProjectileID.Sets.MinionShot[projectile.type] && !ProjectileID.Sets.SentryShot[projectile.type]);
        }

        public static void TungstenIncreaseProjSize(Projectile projectile, FargoSoulsPlayer modPlayer)
        {
            if (TungstenAlwaysAffectProj(projectile) || (modPlayer.TungstenCD == 0 && TungstenCanAffectProj(projectile)))
            {
                float scale = modPlayer.TerraForce ? 3f : 2f;

                projectile.position = projectile.Center;
                projectile.scale *= scale;
                projectile.width = (int)(projectile.width * scale);
                projectile.height = (int)(projectile.height * scale);
                projectile.Center = projectile.position;
                FargoSoulsGlobalProjectile globalProjectile = projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>();
                globalProjectile.TungstenScale = scale;
                modPlayer.TungstenCD = 30;

                //    if (modPlayer.Eternity)
                //    {
                //        modPlayer.TungstenCD = 0;
                //    }
                //    else if (modPlayer.TerraForce || modPlayer.WizardEnchant)
                //    {
                //        modPlayer.TungstenCD /= 2;
                //    }
            }
        }

        public static void TungstenModifyDamage(Player player, ref int damage, ref bool crit, DamageClass damageClass)
        {
            bool forceBuff = player.GetModPlayer<FargoSoulsPlayer>().TerraForce;

            damage = (int)(damage * (forceBuff ? 1.2 : 1.1));

            int max = forceBuff ? 2 : 1;
            for (int i = 0; i < max; i++)
            {
                if (crit)
                    break;

                crit = Main.rand.Next(0, 100) <= player.ActualClassCrit(damageClass);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TungstenHelmet)
                .AddIngredient(ItemID.TungstenChainmail)
                .AddIngredient(ItemID.TungstenGreaves)
                .AddIngredient(ItemID.TungstenBroadsword)
                .AddIngredient(ItemID.Ruler)
                .AddIngredient(ItemID.CandyCaneSword)

                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}
