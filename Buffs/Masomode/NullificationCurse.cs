using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace FargowiltasSouls.Buffs.Masomode
{
    public class NullificationCurse : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Nullification Curse");
            Description.SetDefault("You cannot dodge and Moon Lord has cycling damage type immunities!");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "无效诅咒");
            Description.AddTranslation((int)GameCulture.CultureName.Chinese, "无法躲避,并且月球领主进行循环伤害免疫!");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<FargoSoulsPlayer>().noDodge = true;
        }
    }
}