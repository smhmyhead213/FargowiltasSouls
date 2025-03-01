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

namespace FargowiltasSouls.EternityMode.Content.Enemy
{
    public class MeteorHead : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.MeteorHead);

        public int Counter;

        public override Dictionary<Ref<object>, CompoundStrategy> GetNetInfo() =>
            new Dictionary<Ref<object>, CompoundStrategy> {
                { new Ref<object>(Counter), IntStrategies.CompoundStrategy },
            };

        public override void OnSpawn(NPC npc)
        {
            base.OnSpawn(npc);

            if (NPC.downedGolemBoss && Main.rand.NextBool(4))
                npc.Transform(NPCID.SolarCorite);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (++Counter > 120)
            {
                Counter = 0;

                int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                if (t != -1 && npc.Distance(Main.player[t].Center) < 600 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.velocity *= 5;
                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }

            EModeGlobalNPC.Aura(npc, 100, BuffID.Burning, false, DustID.Torch);
        }
    }
}
