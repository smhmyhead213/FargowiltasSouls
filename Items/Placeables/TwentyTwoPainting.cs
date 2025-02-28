using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Items.Placeables
{
    public class TwentyTwoPainting : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("22 Painting");
            Tooltip.SetDefault("'Keuhm E. Dee'");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.createTile = ModContent.TileType<TwentyTwoPaintingSheet>();
        }
    }
}