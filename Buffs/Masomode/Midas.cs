using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace FargowiltasSouls.Buffs.Masomode
{
    public class Midas : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Midas");
            Description.SetDefault("Drop money on hit");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "点金手");
            Description.AddTranslation((int)GameCulture.CultureName.Chinese, "被攻击时掉落钱币");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FargoSoulsPlayer>().Midas = true;
        }
    }
}