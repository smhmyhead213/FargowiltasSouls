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
    public class Jellyfishes : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.BloodJelly,
            NPCID.BlueJellyfish,
            NPCID.GreenJellyfish,
            NPCID.PinkJellyfish,
            NPCID.FungoFish
        );

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (npc.wet && npc.ai[1] == 1f) //when they be electrocuting
            {
                Player p = Main.LocalPlayer;
                if (npc.Distance(p.Center) < 200 && p.wet && Collision.CanHitLine(p.Center, 2, 2, npc.Center, 2, 2))
                    p.AddBuff(BuffID.Electrified, 2);

                for (int i = 0; i < 10; i++)
                {
                    Vector2 offset = new Vector2();
                    double angle = Main.rand.NextDouble() * 2d * Math.PI;
                    offset.X += (float)(Math.Sin(angle) * 200);
                    offset.Y += (float)(Math.Cos(angle) * 200);
                    if (Framing.GetTileSafely(npc.Center + offset - new Vector2(4, 4)).LiquidAmount == 0) //dont display outside liquids
                        continue;
                    Dust dust = Main.dust[Dust.NewDust(
                        npc.Center + offset - new Vector2(4, 4), 0, 0,
                        DustID.Electric, 0, 0, 100, Color.White, 1f
                        )];
                    dust.velocity = npc.velocity;
                    if (Main.rand.NextBool(3))
                        dust.velocity += Vector2.Normalize(offset) * -5f;
                    dust.noGravity = true;
                }
            }
        }
    }
}
