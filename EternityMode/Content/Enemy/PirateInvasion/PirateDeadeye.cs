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
    public class PirateDeadeye : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.PirateDeadeye);

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //num3 = 40, num5 = 14f, num8 = 0f, damage = 20/25, num12 = 550f?
            if (npc.ai[2] > 0f && npc.ai[1] <= 25f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.X += Main.rand.Next(-20, 21);
                    speed.Y += Main.rand.Next(-20, 21);
                    speed.Normalize();
                    speed *= 14f;

                    int damage = Main.expertMode ? 20 : 25;
                    Projectile.NewProjectile(npc.GetSource_FromThis(),  npc.Center, speed, ModContent.ProjectileType<PirateDeadeyeBullet>(), damage, 0f, Main.myPlayer);
                }
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item11, npc.Center);
                npc.ai[2] = 0f;
                npc.ai[1] = 0f;
                npc.netUpdate = true;
            }
        }
    }
}
