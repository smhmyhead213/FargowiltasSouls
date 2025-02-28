﻿using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.EternityMode.Net;
using FargowiltasSouls.EternityMode.Net.Strategies;
using FargowiltasSouls.EternityMode.NPCMatching;
using FargowiltasSouls.Items.Accessories.Masomode;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Projectiles;
using FargowiltasSouls.Projectiles.Masomode;
using FargowiltasSouls.Projectiles.Souls;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.EternityMode.Content.Enemy.BloodMoon
{
    public class Dreadnautilus : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BloodNautilus);

        public bool StupidIdiotSquidsAreAround;

        public override Dictionary<Ref<object>, CompoundStrategy> GetNetInfo() =>
            new Dictionary<Ref<object>, CompoundStrategy> {
                { new Ref<object>(StupidIdiotSquidsAreAround), BoolStrategies.CompoundStrategy },
            };

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            if (npc.Distance(FargoSoulsUtil.ClosestPointInHitbox(target.Hitbox, npc.Center)) > npc.height / 2)
                return false;

            return base.CanHitPlayer(npc, target, ref CooldownSlot);
        }

        public override bool PreAI(NPC npc)
        {
            if (!npc.HasValidTarget)
            {
                npc.velocity.Y -= 1f;
            }

            if (npc.ai[1] == 0)
            {
                StupidIdiotSquidsAreAround = NPC.AnyNPCs(NPCID.BloodSquid);
                NetSync(npc);
            }

            switch ((int)npc.ai[0])
            {
                case 0: //between attacks
                    if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BloodDrinker)
                    {
                        if (npc.HasValidTarget)
                        {
                            float modifier = npc.Distance(Main.player[npc.target].Center) > 900 ? 0.3f : 0.1f;
                            npc.velocity += modifier * npc.DirectionTo(Main.player[npc.target].Center);
                        }
                        npc.position += npc.velocity;
                    }
                    break;

                case 1: //spinning around player, dashes at ai1=90, ends at ai1=270
                    if (npc.ai[1] <= 90)
                    {
                        if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BloodDrinker)
                            npc.ai[1]++; //less startup
                    }
                    else
                    {
                        if (StupidIdiotSquidsAreAround)
                            npc.position -= npc.velocity / 2;
                        else if (!npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BloodDrinker)
                            npc.position -= npc.velocity / 4;

                        if (npc.ai[1] % 2 == 0) //spawn thorn missiles on outside of spin
                        {
                            float rotation = npc.velocity.ToRotation();
                            float diff = MathHelper.WrapAngle(rotation - npc.DirectionTo(Main.player[npc.target].Center).ToRotation());
                            rotation += MathHelper.PiOver2 * System.Math.Sign(diff);

                            Vector2 vel = Vector2.UnitX.RotatedBy(rotation);
                            Vector2 spawnOffset = 100f * vel;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + spawnOffset, vel, ModContent.ProjectileType<BloodThornMissile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);
                        }
                    }
                    break;

                case 2: //spit blood on you, shoot repeatedly after ai=90, ends at ai1=180
                    if (npc.ai[1] < 90 && npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BloodDrinker)
                        npc.ai[1]++;

                    npc.ai[1] += 0.5f;
                    break;

                case 3: //glowing, spawning blood squids, ends at ai1=180
                    {
                        void Checks()
                        {
                            if (npc.ai[1] == 60)
                                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, npc.Center, 0);

                            if (npc.ai[1] >= 120 && npc.ai[1] % 15 == 5) //when done
                            {
                                for (int i = 0; i < 10; i++) //spawn blood spikes around player
                                {
                                    Vector2 target = Main.player[npc.target].Center + Main.rand.NextVector2Circular(64, 16);
                                    Vector2 spawnPos = Main.player[npc.target].Bottom + new Vector2(Main.rand.NextFloat(-256, 256), Main.rand.NextFloat(64));
                                    spawnPos.Y += 250;

                                    for (int j = 0; j < 40; j++)
                                    {
                                        Tile tile = Framing.GetTileSafely(spawnPos);
                                        if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                                            break;
                                        spawnPos.Y += 16;
                                    }

                                    Vector2 vel = Vector2.Normalize(target - spawnPos);

                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos + vel * 50, 0.6f * vel, ModContent.ProjectileType<BloodThornMissile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer);

                                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, 16f * vel.RotatedByRandom(MathHelper.ToRadians(10)), ProjectileID.SharpTears, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, 0f, Main.rand.NextFloat(0.5f, 1f));
                                        if (p != Main.maxProjectiles)
                                        {
                                            Main.projectile[p].hostile = true;
                                            Main.projectile[p].friendly = false;
                                            Main.projectile[p].GetGlobalProjectile<EModeGlobalProjectile>().FriendlyProjTurnedHostile = true;
                                        }
                                    }
                                }
                            }
                        }

                        Checks();

                        if (npc.GetGlobalNPC<FargoSoulsGlobalNPC>().BloodDrinker && npc.ai[1] % 10 != 0)
                        {
                            npc.ai[1]++;
                            Checks();
                        }
                    }
                    break;

                default:
                    break;
            }

            //suck in and kill blood squids
            foreach (NPC n in Main.npc.Where(n => n.active && n.type == NPCID.BloodSquid && n.life < n.lifeMax * 0.6 && npc.Distance(n.Center) < 600))
            {
                if (npc.Distance(n.Center) < npc.width / 4)
                {
                    npc.AddBuff(ModContent.BuffType<BloodDrinker>(), 360);

                    //int heal = n.life;
                    //npc.life += heal;
                    //if (npc.life > npc.lifeMax)
                    //    npc.life = npc.lifeMax;
                    //CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal, true);

                    n.life = 0;
                    n.HitEffect();
                    n.checkDead();
                    n.active = false;
                    CombatText.NewText(n.Hitbox, Color.Red, n.life, true);
                }
                else
                {
                    n.position -= n.velocity;
                    n.position += npc.velocity / 3;
                    n.position += n.DirectionTo(npc.Center) * n.velocity.Length() * 1.5f;
                }
            }

            return base.PreAI(npc);
        }

        public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            //if (StupidIdiotSquidsAreAround) damage *= 0.5;

            return base.StrikeNPC(npc, ref damage, defense, ref knockback, hitDirection, ref crit);
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(ModContent.BuffType<Anticoagulation>(), 600);
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);

            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<DreadShell>(), 5));
        }
    }

    public class BloodSquid : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BloodSquid);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            //npc.knockBackResist += 0.1f;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //FargoSoulsUtil.PrintAI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            base.OnHitPlayer(npc, target, damage, crit);

            target.AddBuff(ModContent.BuffType<Anticoagulation>(), 1200);
        }
    }
}
