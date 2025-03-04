﻿using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.NPCs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Buffs
{
    internal class FargoGlobalBuff : GlobalBuff
    {
        public override void ModifyBuffTip(int type, ref string tip, ref int rare)
        {
            if (FargoSoulsWorld.EternityMode)
            {
                if (type == BuffID.ShadowDodge)
                    tip += "\nEternity Mode: Dodging will reduce your attack speed";
                else if (type == BuffID.IceBarrier)
                    tip += "\nEternity Mode: Effectiveness reduced to 15%";
                else if (type == BuffID.ManaSickness)
                    tip += "\nEternity Mode: Halved attack speed";
            }
        }

        public static int[] DebuffsToLetDecreaseNormally => new int[] {
            BuffID.Stoned,
            BuffID.Cursed,
            ModContent.BuffType<Fused>(),
            ModContent.BuffType<Stunned>()
        };

        public override void Update(int type, Player player, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.Slimed:
                    Main.buffNoTimeDisplay[type] = false;
                    if (FargoSoulsWorld.EternityMode)
                        player.GetModPlayer<FargoSoulsPlayer>().Slimed = true;
                    break;

                case BuffID.BrainOfConfusionBuff:
                    if (FargoSoulsWorld.EternityMode)
                        player.AddBuff(ModContent.BuffType<BrainOfConfusionDebuff>(), player.buffTime[buffIndex] * 2);
                    break;

                case BuffID.OnFire:
                    if (FargoSoulsWorld.EternityMode && Main.raining && player.position.Y < Main.worldSurface
                        && Framing.GetTileSafely(player.Center).WallType == WallID.None && player.buffTime[buffIndex] > 1)
                        player.buffTime[buffIndex] -= 1;
                    break;

                case BuffID.Chilled:
                    if (FargoSoulsWorld.EternityMode && player.buffTime[buffIndex] > 60 * 15)
                        player.buffTime[buffIndex] = 60 * 15;
                    break;

                case BuffID.Dazed:
                    if (player.whoAmI == Main.myPlayer && player.buffTime[buffIndex] % 60 == 55)
                        Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/DizzyBird"));
                    break;

                case BuffID.SwordWhipPlayerBuff:
                case BuffID.CoolWhipPlayerBuff:
                case BuffID.ScytheWhipPlayerBuff:
                case BuffID.ThornWhipPlayerBuff:
                    if (FargoSoulsWorld.EternityMode)
                    {
                        if (player.GetModPlayer<EModePlayer>().HasWhipBuff)
                            player.buffTime[buffIndex] = Math.Min(player.buffTime[buffIndex], 1);
                        player.GetModPlayer<EModePlayer>().HasWhipBuff = true;
                    }
                    break;

                default:
                    break;
            }

            if (FargoSoulsWorld.EternityMode && player.buffTime[buffIndex] > 2 && Main.debuff[type] && !Main.buffNoTimeDisplay[type] && !BuffID.Sets.NurseCannotRemoveDebuff[type]
                && player.GetModPlayer<EModePlayer>().ShorterDebuffsTimer <= 0)
            {
                player.buffTime[buffIndex] -= 1;
            }

            base.Update(type, player, ref buffIndex);
        }

        public override void Update(int type, NPC npc, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.BrokenArmor:
                    npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().BrokenArmor = true;
                    break;

                //                //case BuffID.Chilled: npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().Chilled = true; break;

                case BuffID.Darkness:
                    npc.color = Color.Gray;

                    if (npc.buffTime[buffIndex] % 30 == 0)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC target = Main.npc[i];
                            if (target.active && !target.friendly && Vector2.Distance(npc.Center, target.Center) < 250)
                            {
                                Vector2 velocity = Vector2.Normalize(target.Center - npc.Center) * 5;
                                Projectile.NewProjectile(npc.GetSource_Buff(buffIndex), npc.Center, velocity, ProjectileID.ShadowFlame, 40 +FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0, Main.myPlayer);
                                if (Main.rand.NextBool(3))
                                    break;
                            }
                        }
                    }
                    break;

                case BuffID.Electrified:
                    npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().Electrified = true;
                    break;

                case BuffID.Suffocation:
                    npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().Suffocation = true;
                    break;

                case BuffID.OnFire:
                    if (FargoSoulsWorld.EternityMode && Main.raining && npc.position.Y < Main.worldSurface
                        && Framing.GetTileSafely(npc.Center).WallType == WallID.None && npc.buffTime[buffIndex] > 1)
                        npc.buffTime[buffIndex] -= 1;
                    break;

                case BuffID.BoneWhipNPCDebuff:
                case BuffID.MaceWhipNPCDebuff:
                case BuffID.RainbowWhipNPCDebuff:
                case BuffID.SwordWhipNPCDebuff:
                case BuffID.ThornWhipNPCDebuff:
                    if (FargoSoulsWorld.EternityMode && npc.GetGlobalNPC<EModeGlobalNPC>().HasWhipDebuff)
                        npc.buffTime[buffIndex] = Math.Min(npc.buffTime[buffIndex], 1);
                    npc.GetGlobalNPC<EModeGlobalNPC>().HasWhipDebuff = true;
                    break;

                default:
                    break;
            }
        }
    }
}