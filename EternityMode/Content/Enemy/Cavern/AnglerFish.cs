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

namespace FargowiltasSouls.EternityMode.Content.Enemy.Cavern
{
    public class AnglerFish : EModeNPCBehaviour
    {
        public bool WasHit;

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.AnglerFish);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.Opacity /= 5;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (!WasHit)
                Lighting.AddLight(npc.Center, 0.1f, 0.5f, 0.5f);
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(BuffID.Bleeding, 300);
        }

        public override void OnHitByAnything(NPC npc, Player player, int damage, float knockback, bool crit)
        {
            base.OnHitByAnything(npc, player, damage, knockback, crit);

            if (!WasHit)
            {
                WasHit = true;
                npc.Opacity *= 5;
            }
        }
    }
}
