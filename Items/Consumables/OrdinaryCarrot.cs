using FargowiltasSouls.Toggler;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace FargowiltasSouls.Items.Consumables
{
    public class OrdinaryCarrot : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ordinary Carrot");
            Tooltip.SetDefault(@"Increases night vision
Minor improvements to all stats
1 minute duration
Right click to increase view range while in inventory
'Plucked from the face of a defeated foe'");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "普通的胡萝卜");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese,
@"'从被打败的敌人的脸上拔下来的'
提高夜视能力
小幅提升所有属性
1分钟持续时间
(失落军团掉落)");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 30;
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.consumable = true;
            Item.UseSound = SoundID.Item2;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }

        public override void UpdateInventory(Player player)
        {
            if (player.GetToggleValue("MasoCarrot", false))
                player.scope = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.AddBuff(BuffID.NightOwl, 3600);
                player.AddBuff(BuffID.WellFed, 3600);
            }
            return true;
        }
    }
}