﻿using FargowiltasSouls.NPCs;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Buffs.Souls
{
    public class Frozen : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Frozen");
            Description.SetDefault("You cannot move");
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override string Texture => "FargowiltasSouls/Buffs/PlaceholderDebuff";

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<FargoSoulsGlobalNPC>().TimeFrozen = true;
            npc.GetGlobalNPC<FargoSoulsGlobalNPC>().Chilled = true;
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.buffTime[buffIndex] += time;
            return base.ReApply(npc, time, buffIndex);
        }
    }
}