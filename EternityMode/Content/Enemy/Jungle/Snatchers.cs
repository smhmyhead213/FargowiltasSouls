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
    public class Snatchers : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Snatcher,
            NPCID.ManEater,
            NPCID.AngryTrapper
        );

        public int DashTimer;
        public int BiteTimer;
        public int BittenPlayer = -1;

        public override Dictionary<Ref<object>, CompoundStrategy> GetNetInfo() =>
            new Dictionary<Ref<object>, CompoundStrategy> {
                { new Ref<object>(DashTimer), IntStrategies.CompoundStrategy },
                { new Ref<object>(BiteTimer), IntStrategies.CompoundStrategy },
                { new Ref<object>(BittenPlayer), IntStrategies.CompoundStrategy },
            };

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.buffImmune[BuffID.Poisoned] = true;
            npc.buffImmune[BuffID.Venom] = true;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            int dashTime = npc.type == NPCID.AngryTrapper ? 120 : 360;

            if (BittenPlayer != -1)
            {
                DashTimer = 0;

                Player victim = Main.player[BittenPlayer];
                if (BiteTimer > 0 && victim.active && !victim.ghost && !victim.dead 
                    && (npc.Distance(victim.Center) < 160 || victim.whoAmI != Main.myPlayer)
                    && victim.GetModPlayer<FargoSoulsPlayer>().MashCounter < 20)
                {
                    victim.AddBuff(ModContent.BuffType<Buffs.Boss.Grabbed>(), 2);
                    victim.velocity = Vector2.Zero;
                    npc.Center = victim.Center;
                }
                else
                {
                    BittenPlayer = -1;
                    BiteTimer = -90; //cooldown

                    //retract towards home
                    npc.velocity = 15f * npc.DirectionTo(new Vector2(npc.ai[0] * 16, npc.ai[1] * 16));

                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }
            else if (++DashTimer > dashTime && npc.Distance(new Vector2((int)npc.ai[0] * 16, (int)npc.ai[1] * 16)) < 1000 && npc.HasValidTarget)
            {
                DashTimer = 0;
                npc.velocity = 15f * Vector2.Normalize(Main.player[npc.target].Center - npc.Center);
            }

            if (DashTimer == dashTime - 30)
                NetSync(npc);

            if (BiteTimer < 0)
                BiteTimer++;
            if (BiteTimer > 0)
                BiteTimer--;
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(BuffID.Bleeding, 300);

            if (BittenPlayer == -1 && BiteTimer == 0)
            {
                BittenPlayer = target.whoAmI;
                BiteTimer = 360;
                NetSync(npc, false);
            }

            if (FargoSoulsWorld.MasochistModeReal && target.statLife + damage < (npc.type == NPCID.AngryTrapper ? 200 : 100))
                target.KillMe(PlayerDeathReason.ByCustomReason(target.name + " was swallowed whole."), 999, 0);
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);

            if (npc.type == NPCID.AngryTrapper)
                FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Vine, 2));
        }
    }
}
