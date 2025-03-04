using Terraria;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Buffs.Masomode;

namespace FargowiltasSouls.Items.Accessories.Masomode
{
    public class ReinforcedPlating : SoulsItem
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reinforced Plating");
            Tooltip.SetDefault(@"Grants immunity to Defenseless, Nano Injection, and knockback
Reduces damage taken by 5%
'The sturdiest piece of a defeated foe'");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "强化钢板");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, @"'被打败的敌人最坚强的一面'
免疫毫无防御,昏迷和击退
减少10%所受伤害");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 4);
            Item.defense = 15;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[ModContent.BuffType<Defenseless>()] = true;
            player.buffImmune[ModContent.BuffType<NanoInjection>()] = true;
            player.endurance += 0.05f;
            player.noKnockback = true;
        }
    }
}