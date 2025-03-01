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
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.PirateInvasion
{
    public class PirateCrossbower : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.PirateCrossbower);

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //num3 = 80, num5 = 16f, num8 = Math.Abs(num7) * .08f, damage = 32/40, num12 = 800f?
            if (npc.ai[2] > 0f && npc.ai[1] <= 45f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.X += Main.rand.Next(-20, 21);
                    speed.Y += Main.rand.Next(-20, 21);
                    speed.Normalize();
                    speed *= 11f;

                    int damage = Main.expertMode ? 32 : 40;
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed, ModContent.ProjectileType<PirateCrossbowerArrow>(), damage, 0f, Main.myPlayer);
                }
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, npc.Center);
                npc.ai[2] = 0f;
                npc.ai[1] = 0f;
                npc.netUpdate = true;
            }
        }
    }
}
