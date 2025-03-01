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
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.Jungle
{
    public class GiantTortoise : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GiantTortoise);

        public override void OnSpawn(NPC npc)
        {
            base.OnSpawn(npc);

            if (Main.rand.NextBool(3))
                EModeGlobalNPC.Horde(npc, Main.rand.Next(2, 6));
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            npc.reflectsProjectiles =
                npc.ai[0] == 3f //spinning
                && npc.HasValidTarget //while near player or line of sight
                && (npc.Distance(Main.player[npc.target].Center) < 10 * 16 || Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0));
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
        {
            base.OnHitByItem(npc, player, item, damage, knockback, crit);

            if (npc.type == NPCID.GiantTortoise)
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was impaled by a Giant Tortoise."), damage / 2, 0);
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(ModContent.BuffType<Defenseless>(), 300);

            target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(ModContent.BuffType<Stunned>(), 120);
            target.velocity = Vector2.Normalize(target.Center - npc.Center) * 30;
        }
    }
}
