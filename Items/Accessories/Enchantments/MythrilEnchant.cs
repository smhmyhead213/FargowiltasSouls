using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FargowiltasSouls.Toggler;

namespace FargowiltasSouls.Items.Accessories.Enchantments
{
    public class MythrilEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Mythril Enchantment");
            Tooltip.SetDefault(
@"15% increased weapon use speed
Taking damage temporarily removes this weapon use speed increase
'You feel the knowledge of your weapons seep into your mind'");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "秘银魔石");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese,
@"增加15%武器使用速度
受到伤害时武器使用速度增加效果会暂时失效
'你感觉你对武器的知识渗透进了你的脑海中");
        }

        protected override Color nameColor => new Color(157, 210, 144);

        public override void SetDefaults()
        {
            base.SetDefaults();
            
            Item.rare = ItemRarityID.Pink;
            Item.value = 100000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FargoSoulsPlayer fargoPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (player.GetToggleValue("Mythril"))
            {
                fargoPlayer.MythrilEnchantActive = true;
                if (!fargoPlayer.DisruptedFocus)
                    fargoPlayer.AttackSpeed += fargoPlayer.WizardEnchantActive ? .2f : .15f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddRecipeGroup("FargowiltasSouls:AnyMythrilHead")
            .AddIngredient(ItemID.MythrilChainmail)
            .AddIngredient(ItemID.MythrilGreaves)
            //flintlock pistol
            //.AddIngredient(ItemID.LaserRifle);
            .AddIngredient(ItemID.ClockworkAssaultRifle)
            .AddIngredient(ItemID.Gatligator)
            .AddIngredient(ItemID.OnyxBlaster)


            .AddTile(TileID.CrystalBall)
            .Register();

        }
    }
}
