﻿using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.EternityMode.Net;
using FargowiltasSouls.EternityMode.Net.Strategies;
using FargowiltasSouls.EternityMode.NPCMatching;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.Cavern
{
    public class ExclusiveEnemies : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => 
            new NPCMatcher().MatchTypeRange(
                NPCID.Crawdad,
                NPCID.GiantShelly,
                NPCID.Salamander,
                NPCID.Salamander2,
                NPCID.Salamander3,
                NPCID.Salamander4,
                NPCID.GiantShelly2,
                NPCID.Salamander5,
                NPCID.Salamander6,
                NPCID.Salamander7,
                NPCID.Salamander8,
                NPCID.Crawdad2
            );

        public override void OnSpawn(NPC npc)
        {
            base.OnSpawn(npc);

            switch (npc.type)
            {
                case NPCID.Crawdad:
                case NPCID.GiantShelly:
                    if (Main.rand.NextBool(5)) //pick a random salamander
                        npc.Transform(Main.rand.Next(498, 507));
                    break;

                case NPCID.Salamander:
                case NPCID.Salamander2:
                case NPCID.Salamander3:
                case NPCID.Salamander4:
                case NPCID.GiantShelly2:
                    if (Main.rand.NextBool(5)) //pick a random crawdad
                        npc.Transform(Main.rand.Next(494, 496));
                    break;

                case NPCID.Salamander5:
                case NPCID.Salamander6:
                case NPCID.Salamander7:
                case NPCID.Salamander8:
                case NPCID.Crawdad2:
                    if (Main.rand.NextBool(5)) //pick a random shelly
                        npc.Transform(Main.rand.Next(496, 498));
                    break;

                default: break;
            }
        }
    }
}
