﻿using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.EternityMode.Net;
using FargowiltasSouls.EternityMode.Net.Strategies;
using FargowiltasSouls.EternityMode.NPCMatching;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Projectiles;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.SolarEclipse
{
    public class Butcher : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Butcher);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.knockBackResist = 0;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            npc.position.X += npc.velocity.X;
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(ModContent.BuffType<Berserked>(), 600);
            target.AddBuff(BuffID.Bleeding, 600);
        }
    }
}
