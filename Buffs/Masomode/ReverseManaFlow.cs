using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace FargowiltasSouls.Buffs.Masomode
{
    public class ReverseManaFlow : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reverse Mana Flow");
            Description.SetDefault("Your magic weapons cost life instead of mana");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "反魔力流");
            Description.AddTranslation((int)GameCulture.CultureName.Chinese, "魔法武器消耗生命,而不是法力");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            //mana cost also damages
            player.GetModPlayer<FargoSoulsPlayer>().ReverseManaFlow = true;
            player.GetDamage(DamageClass.Magic) -= 1.5f;
            if (player.HeldItem.DamageType == DamageClass.Magic)
                player.GetModPlayer<FargoSoulsPlayer>().AttackSpeed -= 0.5f;
        }
    }
}