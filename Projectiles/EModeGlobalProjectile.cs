using System;
using System.Linq;
using FargowiltasSouls.Buffs.Boss;
using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.EternityMode;
using FargowiltasSouls.EternityMode.Content.Boss.HM;
using FargowiltasSouls.Projectiles.Champions;
using FargowiltasSouls.NPCs.Champions;
using Terraria.DataStructures;

namespace FargowiltasSouls.Projectiles
{
    public class EModeGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool HasKillCooldown;
        public bool NerfDamageBasedOnProjCount;
        public bool FriendlyProjTurnedHostile;
        public bool EModeCanHurt = true;

        private int counter;
        private bool firstTickAICheckDone;

        public override void SetDefaults(Projectile projectile)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.StardustCellMinionShot:
                case ProjectileID.EmpressBlade:
                    //NerfDamageBasedOnProjCount = true;
                    break;

                case ProjectileID.FinalFractal: //zenith
                    if (!FargoSoulsWorld.downedMutant)
                    {
                        projectile.usesLocalNPCImmunity = false;
                        projectile.localNPCHitCooldown = 0;

                        projectile.usesIDStaticNPCImmunity = true;
                        if (FargoSoulsWorld.downedAbom) 
                            projectile.idStaticNPCHitCooldown = 4;
                        else if (FargoSoulsWorld.downedChampions[(int)FargoSoulsWorld.Downed.CosmosChampion]) 
                            projectile.idStaticNPCHitCooldown = 6;
                        else
                            projectile.idStaticNPCHitCooldown = 7;

                        projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().noInteractionWithNPCImmunityFrames = true;
                    }
                    break;

                case ProjectileID.Sharknado:
                case ProjectileID.Cthulunado:
                    if (!FargoSoulsWorld.MasochistModeReal)
                    {
                        EModeCanHurt = false;
                        projectile.hide = true;
                    }
                    break;

                case ProjectileID.SpiritHeal:
                    projectile.timeLeft = 240 * 4; //account for extraupdates
                    break;

                case ProjectileID.DD2BetsyFlameBreath:
                    projectile.tileCollide = false;
                    projectile.penetrate = -1;
                    break;

                case ProjectileID.ChlorophyteBullet:
                    projectile.extraUpdates = 1;
                    projectile.timeLeft = 150;
                    break;

                case ProjectileID.CrystalBullet:
                case ProjectileID.HolyArrow:
                case ProjectileID.HallowStar:
                    //HasKillCooldown = true;
                    break;

                case ProjectileID.SaucerLaser:
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.CultistBossFireBall: //disable proj
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
                    {
                        if (Main.npc[EModeGlobalNPC.cultBoss].life < Main.npc[EModeGlobalNPC.cultBoss].lifeMax / 2)
                        {
                            projectile.timeLeft = 1;
                            EModeCanHurt = false;
                        }
                    }
                    break;

                case ProjectileID.CultistBossFireBallClone: //disable proj
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
                    {
                        projectile.timeLeft = 1;
                        EModeCanHurt = false;
                    }
                    break;

                case ProjectileID.AncientDoomProjectile:
                    projectile.scale *= 1.5f;
                    break;

                case ProjectileID.SharknadoBolt:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBossEX, NPCID.DukeFishron))
                        projectile.extraUpdates++;
                    break;

                case ProjectileID.FlamesTrap:
                    if (FargoSoulsUtil.NPCExists(NPC.golemBoss, NPCID.Golem) != null)
                        projectile.tileCollide = false;
                    break;

                case ProjectileID.UnholyTridentHostile:
                    projectile.extraUpdates++;
                    break;

                case ProjectileID.BulletSnowman:
                    projectile.tileCollide = false;
                    projectile.timeLeft = 600;
                    break;

                case ProjectileID.CannonballHostile:
                    projectile.scale = 2f;
                    break;

                case ProjectileID.EyeLaser:
                case ProjectileID.EyeFire:
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.QueenSlimeSmash:
                    if (!FargoSoulsWorld.MasochistModeReal && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.queenSlimeBoss, NPCID.QueenSlimeBoss))
                    {
                        projectile.timeLeft = 0;
                        EModeCanHurt = false;
                    }
                    break;

                case ProjectileID.QueenSlimeMinionBlueSpike:
                    projectile.scale *= 1.5f;
                    break;

                case ProjectileID.BloodShot:
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                    projectile.tileCollide = false;
                    break;

                default:
                    break;
            }
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.BloodShot:
                    {
                        if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.active && npc.type == NPCID.BloodSquid)
                            projectile.damage /= 2;
                    }
                    break;

                case ProjectileID.HallowBossLastingRainbow:
                    EModeCanHurt = false;
                    projectile.timeLeft += 60;
                    projectile.localAI[1] = projectile.velocity.ToRotation();
                    break;

                case ProjectileID.HallowBossRainbowStreak:
                    if (!FargoSoulsWorld.MasochistModeReal)
                    {
                        EModeCanHurt = false;

                        if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.active && npc.type == NPCID.HallowBoss && npc.ai[0] == 12)
                            projectile.velocity *= 0.7f;
                    }
                    break;

                case ProjectileID.Meowmere:
                    if (source is EntitySource_ItemUse)
                        FargoSoulsGlobalProjectile.SplitProj(projectile, 3, MathHelper.ToRadians(30), 1f);
                    break;

                case ProjectileID.FallingStar:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<NPCs.MutantBoss.MutantBoss>()))
                        projectile.active = false;
                    break;

                case ProjectileID.VampireHeal:
                    //each lifesteal hits timer again when above 33% life (total, halved lifesteal rate)
                    if (Main.player[projectile.owner].statLife > Main.player[projectile.owner].statLifeMax2 / 3)
                        Main.player[projectile.owner].lifeSteal -= projectile.ai[1];

                    //each lifesteal hits timer again when above 33% life (stacks with above, total 1/3rd lifesteal rate)
                    if (Main.player[projectile.owner].statLife > Main.player[projectile.owner].statLifeMax2 * 2 / 3)
                        Main.player[projectile.owner].lifeSteal -= projectile.ai[1];
                    break;

                default:
                    break;
            }
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.CanHitPlayer(projectile, target);

            if (!EModeCanHurt)
                return false;
            
            return base.CanHitPlayer(projectile, target);
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.CanHitNPC(projectile, target);

            if (!EModeCanHurt)
                return false;

            return base.CanHitNPC(projectile, target);
        }

        public override bool PreAI(Projectile projectile)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.PreAI(projectile);

            counter++;

            //spam clients with packets to make sure this is synced to them
            if (FriendlyProjTurnedHostile && counter <= 60 && counter % 10 == 0 && Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = FargowiltasSouls.Instance.GetPacket();
                packet.Write((byte)23);
                packet.Write(projectile.owner);
                packet.Write(projectile.identity);
                packet.Write(projectile.type);
                packet.Send();
            }

            //delay the very bottom piece of sharknados spawning in, also delays spawning sharkrons
            if (counter < 30 && projectile.ai[0] == 15 && !FargoSoulsWorld.MasochistModeReal
                && (projectile.type == ProjectileID.Sharknado || projectile.type == ProjectileID.Cthulunado)
                && projectile.ai[1] == (projectile.type == ProjectileID.Sharknado ? 15 : 24))
            {
                projectile.timeLeft++;
                return false;
            }

            return base.PreAI(projectile);
        }

        public override void AI(Projectile projectile)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.BloodShot:
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                    if (!Collision.SolidTiles(projectile.Center, 0, 0))
                    {
                        Lighting.AddLight(projectile.Center, TorchID.Crimson);

                        if (counter > 180)
                            projectile.tileCollide = true;
                    }
                    break;

                case ProjectileID.HallowBossLastingRainbow:
                    if (Math.Abs(MathHelper.WrapAngle(projectile.velocity.ToRotation() - projectile.localAI[1])) > MathHelper.Pi * 0.9f)
                        EModeCanHurt = true;
                    projectile.extraUpdates = EModeCanHurt ? 1 : 3;
                    break;

                case ProjectileID.HallowBossRainbowStreak:
                    EModeCanHurt = FargoSoulsWorld.MasochistModeReal || projectile.timeLeft < 100;
                    break;

                case ProjectileID.FairyQueenSunDance:
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.HallowBoss);
                        if (npc != null)
                        {
                            if (npc.ai[0] == 8 || npc.ai[0] == 9) //doing dash
                            {
                                projectile.rotation = projectile.ai[0]; //negate rotation

                                if (counter < 60) //force proj into active state faster
                                    counter += 9;
                                if (projectile.localAI[0] < 60)
                                    projectile.localAI[0] += 9;
                            }

                            if (npc.ai[0] == 1 || npc.ai[0] == 10) //while empress is moving back over player or p2 transition
                            {
                                EModeCanHurt = false;
                                counter = 0;
                                projectile.timeLeft = 0;
                            }
                            
                            if (counter >= 60 && projectile.scale > 0.5f && counter % 10 == 0)
                            {
                                float offset = MathHelper.ToRadians(90) * MathHelper.Lerp(0f, 1f, (counter % 50f) / 50f);
                                for (int i = -1; i <= 1; i += 2)
                                {
                                    if (Math.Abs(offset) < 0.001f && i < 0)
                                        continue;

                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        const float spawnOffset = 800;
                                        Projectile.NewProjectile(Projectile.InheritSource(projectile), projectile.Center + projectile.rotation.ToRotationVector2() * spawnOffset, Vector2.Zero, ProjectileID.FairyQueenLance, projectile.damage, projectile.knockBack, projectile.owner, projectile.rotation + offset * i, projectile.ai[0]);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.QueenBeeStinger:
                    projectile.velocity.Y -= 0.1f; //negate gravity
                    break;

                case ProjectileID.BeeHive:
                    if (projectile.timeLeft > 30 && (projectile.velocity.X != 0 || projectile.velocity.Y == 0))
                        projectile.timeLeft = 30;
                    break;

                case ProjectileID.TowerDamageBolt:
                    if (!firstTickAICheckDone)
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[0], NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex);
                        if (npc != null)
                        {
                            //not kill, because kill decrements shield
                            if (projectile.Distance(npc.Center) > 4000)
                                projectile.active = false;
                            int p = Player.FindClosest(projectile.Center, 0, 0);
                            if (p != -1 && !(Main.player[p].active && npc.Distance(Main.player[p].Center) < 4000))
                                projectile.active = false;
                        }
                    }
                    break;

                case ProjectileID.SpiritHeal:
                    projectile.position -= projectile.velocity / 4;
                    break;

                case ProjectileID.Sharknado: //this only runs after changes in preAI() finish blocking it
                case ProjectileID.Cthulunado:
                    EModeCanHurt = true;
                    projectile.hide = false;
                    if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBoss, NPCID.DukeFishron))
                        projectile.timeLeft = Math.Min(120, projectile.timeLeft);
                    break;

                case ProjectileID.WireKite:
                    if (Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().LihzahrdCurse
                        && Framing.GetTileSafely(projectile.Center).WallType == WallID.LihzahrdBrickUnsafe)
                    {
                        projectile.Kill();
                    }
                    break;

                case ProjectileID.Fireball:
                    {
                        NPC golem = FargoSoulsUtil.NPCExists(NPC.golemBoss, NPCID.Golem);
                        if (!FargoSoulsWorld.MasochistModeReal && golem != null && !golem.dontTakeDamage)
                            projectile.timeLeft = 0;
                    }
                    break;

                case ProjectileID.GeyserTrap:
                    if (!FargoSoulsWorld.MasochistModeReal && FargoSoulsUtil.NPCExists(NPC.golemBoss, NPCID.Golem) != null && counter > 45)
                        projectile.Kill();
                    break;

                case ProjectileID.CultistBossFireBall:
                    if (!FargoSoulsWorld.MasochistModeReal && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
                        projectile.position -= projectile.velocity * Math.Max(0, 1f - counter / 45f / projectile.MaxUpdates); //accel startup
                    break;

                case ProjectileID.NebulaSphere:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
                    {
                        int p = Player.FindClosest(projectile.Center, 0, 0);
                        if (p != -1 && projectile.Distance(Main.player[p].Center) > 240)
                            projectile.position += projectile.velocity;
                    }
                    break;

                case ProjectileID.PhantasmalBolt:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.moonBoss, NPCID.MoonLordCore) && !FargoSoulsWorld.SwarmActive && !FargoSoulsWorld.MasochistModeReal)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center,
                                    1.5f * Vector2.Normalize(projectile.velocity).RotatedBy(Math.PI / 2 / 2 * i),
                                    ModContent.ProjectileType<PhantasmalBolt2>(), projectile.damage, 0f, Main.myPlayer);
                            }
                            projectile.Kill();
                        }
                    }
                    break;

                case ProjectileID.EyeBeam:
                    if (FargoSoulsUtil.BossIsAlive(ref NPC.golemBoss, NPCID.Golem) && !FargoSoulsWorld.SwarmActive && !FargoSoulsWorld.MasochistModeReal)
                    {
                        if (!firstTickAICheckDone)
                        {
                            projectile.velocity.Normalize();
                            projectile.timeLeft = 180 * projectile.MaxUpdates;
                        }

                        if (projectile.timeLeft % (projectile.extraUpdates + 1) == 0) //only run once per tick
                        {
                            if (++projectile.localAI[1] < 90)
                            {
                                projectile.velocity *= 1.04f;
                            }
                        }
                    }
                    break;

                case ProjectileID.JavelinHostile:
                case ProjectileID.FlamingWood:
                    projectile.position += projectile.velocity * .5f;
                    break;

                case ProjectileID.VortexAcid:
                    projectile.position += projectile.velocity * .25f;
                    break;

                case ProjectileID.NebulaEye:
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.NebulaBrain);
                        if (npc != null)
                        {
                            if (npc.ai[0] < 45 && projectile.ai[0] >= 180 - 5)
                                projectile.ai[0] -= 180; //prevent firing shortly after teleport
                        }
                    }
                    break;

                case ProjectileID.CultistRitual:
                    if (!FargoSoulsWorld.SwarmActive)
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.CultistBoss);
                        if (npc != null && npc.ai[3] == -1f && npc.ai[0] == 5)
                        {
                            projectile.Center = Main.player[npc.target].Center;
                        }

                        if (!firstTickAICheckDone) //MP sync data to server
                        {
                            if (npc != null)
                            {
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    LunaticCultist cultistData = npc.GetEModeNPCMod<LunaticCultist>();

                                    var netMessage = Mod.GetPacket(); //sync damage contribution (which is client side) to server
                                    netMessage.Write((byte)10);
                                    netMessage.Write((byte)npc.whoAmI);
                                    netMessage.Write(cultistData.MeleeDamageCounter);
                                    netMessage.Write(cultistData.RangedDamageCounter);
                                    netMessage.Write(cultistData.MagicDamageCounter);
                                    netMessage.Write(cultistData.MinionDamageCounter);
                                    netMessage.Send();
                                }
                                else //refresh ritual
                                {
                                    for (int i = 0; i < Main.maxProjectiles; i++)
                                    {
                                        if (Main.projectile[i].active && Main.projectile[i].ai[1] == npc.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<CultistRitual>())
                                        {
                                            Main.projectile[i].Kill();
                                            break;
                                        }
                                    }

                                    Projectile.NewProjectile(npc.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<CultistRitual>(),FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f, Main.myPlayer, 0f, npc.whoAmI);
                                    const int max = 16;
                                    const float appearRadius = 1600f - 100f;
                                    for (int i = 0; i < max; i++)
                                    {
                                        float rotation = MathHelper.TwoPi / max * i;
                                        for (int j = -1; j <= 1; j += 2)
                                        {
                                            Vector2 spawnoffset = new Vector2(appearRadius, 0).RotatedBy(rotation);
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), projectile.Center + spawnoffset, j * Vector2.UnitY.RotatedBy(rotation), ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 18, npc.whoAmI);
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < Main.maxProjectiles; i++) //purge spectre mask bolts and homing nebula spheres
                            {
                                if (Main.projectile[i].active && (Main.projectile[i].type == ProjectileID.SpectreWrath || Main.projectile[i].type == ProjectileID.NebulaSphere))
                                    Main.projectile[i].Kill();
                            }
                        }

                        //if (Fargowiltas.Instance.MasomodeEXLoaded && projectile.ai[0] > 120f && projectile.ai[0] < 299f) projectile.ai[0] = 299f;

                        bool dunk = false;

                        if (projectile.ai[1] == -1)
                        {
                            if (counter == 5)
                                dunk = true;
                        }
                        else
                        {
                            counter = 0;
                            if (projectile.ai[0] == 299f)
                                dunk = true;
                        }

                        if (dunk) //pillar dunk
                        {
                            int cult = -1;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBoss && Main.npc[i].ai[2] == projectile.whoAmI)
                                {
                                    cult = i;
                                    break;
                                }
                            }

                            if (cult != -1)
                            {
                                float ai0 = Main.rand.Next(4);

                                LunaticCultist cultistData = Main.npc[cult].GetEModeNPCMod<LunaticCultist>();
                                int[] weight = new int[4];
                                weight[0] = cultistData.MagicDamageCounter;
                                weight[1] = cultistData.MeleeDamageCounter;
                                weight[2] = cultistData.RangedDamageCounter;
                                weight[3] = cultistData.MinionDamageCounter;

                                cultistData.MeleeDamageCounter = 0;
                                cultistData.RangedDamageCounter = 0;
                                cultistData.MagicDamageCounter = 0;
                                cultistData.MinionDamageCounter = 0;

                                if (Main.netMode == NetmodeID.Server)
                                    Main.npc[cult].GetGlobalNPC<NewEModeGlobalNPC>().NetSync(Main.npc[cult]);

                                int max = 0;
                                for (int i = 1; i < 4; i++)
                                    if (weight[max] < weight[i])
                                        max = i;
                                if (weight[max] > 0)
                                    ai0 = max;

                                if ((cultistData.EnteredPhase2 /*|| Fargowiltas.Instance.MasomodeEXLoaded*/ || FargoSoulsWorld.MasochistModeReal) && Main.netMode != NetmodeID.MultiplayerClient && !Main.projectile.Any(p => p.active && p.hostile && p.type == ModContent.ProjectileType<CelestialPillar>()))
                                {
                                    Projectile.NewProjectile(Main.npc[cult].GetSource_FromThis(), projectile.Center, Vector2.UnitY * -10f, ModContent.ProjectileType<CelestialPillar>(),
                                        Math.Max(75, FargoSoulsUtil.ScaledProjectileDamage(Main.npc[cult].damage, 4)), 0f, Main.myPlayer, ai0);
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.MoonLeech:
                    if (projectile.ai[0] > 0f && !FargoSoulsWorld.SwarmActive)
                    {
                        Vector2 distance = Main.player[(int)projectile.ai[1]].Center - projectile.Center - projectile.velocity;
                        if (distance != Vector2.Zero)
                            projectile.position += Vector2.Normalize(distance) * Math.Min(16f, distance.Length());
                    }
                    break;

                case ProjectileID.SandnadoHostile:
                    if (!FargoSoulsUtil.AnyBossAlive() && projectile.timeLeft == 1199 && NPC.CountNPCS(NPCID.SandShark) < 10 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int n = NPC.NewNPC(projectile.GetSource_FromThis(), (int)projectile.Center.X, (int)projectile.Center.Y, NPCID.SandShark);
                        if (n < Main.maxProjectiles)
                        {
                            Main.npc[n].velocity.X = Main.rand.NextFloat(-10, 10);
                            Main.npc[n].velocity.Y = Main.rand.NextFloat(-20, -10);
                            Main.npc[n].netUpdate = true;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        }
                    }
                    break;

                case ProjectileID.PhantasmalEye:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.moonBoss, NPCID.MoonLordCore) && !FargoSoulsWorld.SwarmActive)
                    {
                        if (projectile.ai[0] == 2 && counter > 60) //diving down and homing
                        {
                            projectile.velocity.Y = 9;
                        }
                        else
                        {
                            projectile.position.Y -= projectile.velocity.Y / 4;
                        }

                        if (projectile.velocity.X > 1)
                            projectile.velocity.X = 1;
                        else if (projectile.velocity.X < -1)
                            projectile.velocity.X = -1;
                    }
                    break;

                case ProjectileID.PhantasmalSphere:
                    if (!FargoSoulsWorld.SwarmActive && !FargoSoulsWorld.MasochistModeReal)
                    {
                        if (!firstTickAICheckDone)
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.MoonLordHand);
                            if (npc != null)
                                projectile.localAI[0] = 1;
                        }

                        EModeCanHurt = projectile.alpha == 0;

                        if (projectile.ai[0] == -1 && projectile.localAI[0] > 0) //sent to fly, flagged as from hand
                        {
                            if (++projectile.localAI[1] < 150)
                                projectile.velocity *= 1.018f;

                            if (projectile.localAI[0] == 1 && projectile.velocity.Length() > 11) //only do this once
                            {
                                projectile.localAI[0] = 2;
                                projectile.velocity.Normalize();

                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity, ModContent.ProjectileType<PhantasmalSphereDeathray>(),
                                        0, 0f, Main.myPlayer, 0f, projectile.identity);
                                }
                            }

                            projectile.netUpdate = true;
                        }
                    }
                    break;

                case ProjectileID.BombSkeletronPrime: //needs to be set every tick
                    if (!FargoSoulsWorld.SwarmActive)
                        projectile.damage = 40;
                    break;

                case ProjectileID.DD2BetsyFireball: //when spawned, also spawn a phoenix
                    if (!FargoSoulsWorld.SwarmActive
                        && !firstTickAICheckDone && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        bool phase2 = FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.betsyBoss, NPCID.DD2Betsy)
                            && Main.npc[EModeGlobalNPC.betsyBoss].GetEModeNPCMod<Betsy>().InPhase2;
                        int max = phase2 ? 2 : 1;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 speed = Main.rand.NextFloat(8, 12) * -Vector2.UnitY.RotatedByRandom(Math.PI / 2);
                            float ai1 = phase2 ? 60 + Main.rand.Next(60) : 90 + Main.rand.Next(30);
                            Projectile.NewProjectile(Projectile.InheritSource(projectile), projectile.Center, speed, ModContent.ProjectileType<BetsyPhoenix>(),
                                projectile.damage, 0f, Main.myPlayer, Player.FindClosest(projectile.Center, 0, 0), ai1);
                        }
                    }
                    break;

                case ProjectileID.DD2BetsyFlameBreath:
                    if (!FargoSoulsWorld.SwarmActive)
                    {
                        bool phase2 = FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.betsyBoss, NPCID.DD2Betsy)
                                    && Main.npc[EModeGlobalNPC.betsyBoss].GetEModeNPCMod<Betsy>().InPhase2;

                        //add chain blasts in maso p2
                        if (phase2 && !firstTickAICheckDone && FargoSoulsWorld.MasochistModeReal && Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(
                                Projectile.InheritSource(projectile),
                                projectile.Center + 100f * Vector2.Normalize(Main.npc[EModeGlobalNPC.betsyBoss].velocity),
                                Vector2.Zero,
                                ModContent.ProjectileType<EarthChainBlast>(),
                                projectile.damage,
                                0f,
                                Main.myPlayer,
                                Main.npc[EModeGlobalNPC.betsyBoss].velocity.ToRotation(),
                                7);
                        }

                        //add fireballs
                        if (counter > (phase2 ? 2 : 4))
                        {
                            counter = 0;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item34, projectile.Center);
                                Vector2 projVel = projectile.velocity.RotatedBy((Main.rand.NextDouble() - 0.5) * Math.PI / 10);
                                projVel.Normalize();
                                projVel *= Main.rand.NextFloat(8f, 12f);
                                int type = ProjectileID.CultistBossFireBall;
                                if (!phase2 || Main.rand.NextBool())
                                {
                                    type = ModContent.ProjectileType<Champions.WillFireball>();
                                    projVel *= 2f;
                                    if (phase2)
                                        projVel *= 1.5f;
                                }
                                Projectile.NewProjectile(Projectile.InheritSource(projectile), projectile.Center, projVel, type, projectile.damage, 0f, Main.myPlayer);
                            }
                        }
                    }
                    break;

                case ProjectileID.QueenSlimeGelAttack:
                    {
                        if (!FargoSoulsWorld.MasochistModeReal)
                        {
                            float ratio = Math.Max(0, 1f - counter / 60f / projectile.MaxUpdates);
                            projectile.position -= projectile.velocity * ratio; //accel startup
                            projectile.velocity.Y -= 0.15f * ratio; //compensate the gravity
                        }

                        if (!firstTickAICheckDone)
                        {
                            //if (projectile.velocity.Y > 0)
                            //    projectile.velocity.Y *= -.5f; //shoot up instead

                            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.queenSlimeBoss, NPCID.QueenSlimeBoss)
                                && Main.npc[EModeGlobalNPC.queenSlimeBoss].life > Main.npc[EModeGlobalNPC.queenSlimeBoss].lifeMax / 2)
                            {
                                projectile.velocity.Y -= 8f;
                            }
                        }

                        //when begins falling, spray out
                        if (projectile.velocity.Y > 0 && projectile.localAI[0] == 0)
                        {
                            projectile.localAI[0] = 1;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int j = -1; j <= 1; j += 2)
                                {
                                    if (Math.Sign(projectile.velocity.X) == -j) //very specific phrasing so 0 horiz sprays both ways
                                        continue;

                                    Vector2 baseVel = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(10) * j));
                                    const int max = 8;
                                    for (int i = 0; i < max; i++)
                                    {
                                        Vector2 vel = Main.rand.NextFloat(14f, 18f) * j * baseVel.RotatedBy(MathHelper.PiOver4 / max * i * -j);
                                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, vel, ProjectileID.QueenSlimeMinionBlueSpike, projectile.damage, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.QueenSlimeMinionPinkBall:
                    if (!FargoSoulsWorld.MasochistModeReal)
                    {
                        float ratio = Math.Max(0, 1f - counter / 60f / projectile.MaxUpdates);
                        projectile.position -= projectile.velocity * ratio; //accel startup
                        projectile.velocity.Y -= 0.15f * ratio; //compensate the gravity
                    }
                    break;

                default:
                    break;
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            //if (projectile.arrow) //change archery and quiver to additive damage
            //{
            //    if (Main.player[projectile.owner].archery)
            //    {
            //        damage = (int)(damage / 1.2);
            //        damage = (int)((double)damage * (1.0 + 0.2 / Main.player[projectile.owner].GetDamage(DamageClass.Ranged)));
            //    }

            //    if (Main.player[projectile.owner].magicQuiver)
            //    {
            //        damage = (int)(damage / 1.1);
            //        damage = (int)((double)damage * (1.0 + 0.1 / Main.player[projectile.owner].GetDamage(DamageClass.Ranged)));
            //    }
            //}

            if (NerfDamageBasedOnProjCount)
            {
                int projTypeToCheck = projectile.type;
                if (projectile.type == ProjectileID.StardustCellMinionShot)
                    projTypeToCheck = ProjectileID.StardustCellMinion;

                //note: projs needed to reach max nerf is the sum of these values
                const int allowedBeforeNerfBegins = 5;
                const int maxRampup = 10;

                float modifier = (float)(Main.player[projectile.owner].ownedProjectileCounts[projTypeToCheck] - allowedBeforeNerfBegins) / maxRampup;
                if (modifier < 0)
                    modifier = 0;
                if (modifier > 1)
                    modifier = 1;

                const float maxNerfStrength = 0.25f;
                damage = (int)(damage * (1f - modifier * maxNerfStrength));
            }

            //if (projectile.type == ProjectileID.ChlorophyteBullet)
            //{
            //    damage = (int)(damage * 0.75);
            //}
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.OnTileCollide(projectile, oldVelocity);

            switch (projectile.type)
            {
                case ProjectileID.SnowBallHostile:
                    projectile.active = false; //no block
                    break;

                case ProjectileID.SandBallFalling:
                    if (projectile.ai[0] == 2f) //antlion sand
                    {
                        int num129 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 32, 0f, projectile.velocity.Y / 2f, 0, default(Color), 1f);
                        Dust expr_59B0_cp_0 = Main.dust[num129];
                        expr_59B0_cp_0.velocity.X = expr_59B0_cp_0.velocity.X * 0.4f;
                        projectile.active = false;

                    }
                    break;

                case ProjectileID.QueenSlimeMinionPinkBall:
                case ProjectileID.QueenSlimeGelAttack:
                    if (!FargoSoulsWorld.MasochistModeReal)
                    {
                        //if (projectile.localAI[1] == 1)
                        projectile.timeLeft = 0;
                        //projectile.localAI[1] = 1;
                    }
                    break;

                case ProjectileID.ThornBall:
                    projectile.timeLeft = 0;

                    NPC plantera = FargoSoulsUtil.NPCExists(NPC.plantBoss, NPCID.Plantera);
                    if (plantera != null && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 vel = 200f / 25f * projectile.DirectionTo(plantera.Center);
                        Projectile.NewProjectile(plantera.GetSource_FromThis(), projectile.Center - projectile.oldVelocity, vel, ModContent.ProjectileType<DicerPlantera>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 0);
                    }
                    break;

                default:
                    break;
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public override void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            if (NPC.downedGolemBoss && projectile.type == ProjectileID.VortexLightning)
                damage *= 2;
        }

        public override void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
        {
            if (!FargoSoulsWorld.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.BloodShot:
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                case ProjectileID.SharpTears:
                    target.AddBuff(ModContent.BuffType<Anticoagulation>(), 600);
                    break;

                case ProjectileID.FairyQueenLance:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<NPCs.MutantBoss.MutantBoss>()))
                    {
                        target.GetModPlayer<FargoSoulsPlayer>().MaxLifeReduction += 100;
                        target.AddBuff(ModContent.BuffType<OceanicMaul>(), 5400);
                        target.AddBuff(ModContent.BuffType<MutantFang>(), 180);
                    }
                    goto case ProjectileID.FairyQueenSunDance;

                case ProjectileID.FairyQueenHymn:
                case ProjectileID.FairyQueenSunDance:
                case ProjectileID.HallowBossRainbowStreak:
                case ProjectileID.HallowBossLastingRainbow:
                case ProjectileID.HallowBossSplitShotCore:
                    target.AddBuff(ModContent.BuffType<Purified>(), 300);
                    target.AddBuff(ModContent.BuffType<Smite>(), 1200);
                    break;

                case ProjectileID.RollingCactus:
                case ProjectileID.RollingCactusSpike:
                    target.AddBuff(BuffID.Poisoned, 120);
                    break;

                case ProjectileID.TorchGod:
                    target.AddBuff(BuffID.OnFire, 60);
                    break;

                case ProjectileID.Boulder:
                    target.AddBuff(BuffID.BrokenArmor, 600);
                    break;

                case ProjectileID.JavelinHostile:
                    target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                    target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(ModContent.BuffType<Stunned>(), 60);
                    break;

                case ProjectileID.DemonSickle:
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 300);
                    break;

                case ProjectileID.HarpyFeather:
                    target.AddBuff(ModContent.BuffType<ClippedWings>(), 300);
                    break;

                case ProjectileID.SandBallFalling:
                    if (projectile.velocity.X != 0) //so only antlion sand and not falling sand 
                    {
                        target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(ModContent.BuffType<Stunned>(), 120);
                    }
                    break;

                case ProjectileID.Stinger:
                case ProjectileID.QueenBeeStinger:
                    target.AddBuff(ModContent.BuffType<Swarming>(), 300);
                    break;

                case ProjectileID.Skull:
                    target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(BuffID.Cursed, 30);
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.guardBoss, NPCID.DungeonGuardian))
                    {
                        target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 600);
                        /*target.AddBuff(ModContent.BuffType<GodEater>(), 420);
                        target.AddBuff(ModContent.BuffType<FlamesoftheUniverse>(), 420);
                        target.immune = false;
                        target.immuneTime = 0;*/
                    }
                    break;

                case ProjectileID.EyeLaser:
                case ProjectileID.GoldenShowerHostile:
                case ProjectileID.CursedFlameHostile:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.wallBoss, NPCID.WallofFlesh) && target.ZoneUnderworldHeight)
                        target.AddBuff(BuffID.OnFire, 300);
                    break;

                case ProjectileID.DeathSickle:
                    target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 600);
                    break;

                case ProjectileID.DrManFlyFlask:
                    switch (Main.rand.Next(7))
                    {
                        case 0:
                            target.AddBuff(BuffID.Venom, 300);
                            break;
                        case 1:
                            target.AddBuff(BuffID.Confused, 300);
                            break;
                        case 2:
                            target.AddBuff(BuffID.CursedInferno, 300);
                            break;
                        case 3:
                            target.AddBuff(BuffID.OgreSpit, 300);
                            break;
                        case 4:
                            target.AddBuff(ModContent.BuffType<LivingWasteland>(), 600);
                            break;
                        case 5:
                            target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                            break;
                        case 6:
                            target.AddBuff(ModContent.BuffType<Purified>(), 600);
                            break;

                        default:
                            break;
                    }
                    target.AddBuff(BuffID.Stinky, 1200);
                    break;

                case ProjectileID.SpikedSlimeSpike:
                    target.AddBuff(BuffID.Slimed, 120);
                    break;

                case ProjectileID.QueenSlimeGelAttack:
                case ProjectileID.QueenSlimeMinionBlueSpike:
                case ProjectileID.QueenSlimeMinionPinkBall:
                case ProjectileID.QueenSlimeSmash:
                    target.AddBuff(BuffID.Slimed, 180);
                    target.AddBuff(ModContent.BuffType<Smite>(), 360);
                    break;

                case ProjectileID.CultistBossLightningOrb:
                case ProjectileID.CultistBossLightningOrbArc:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.CultistBossIceMist:
                    target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(BuffID.Frozen, 45);
                    target.AddBuff(ModContent.BuffType<Hypothermia>(), 1200);
                    break;

                case ProjectileID.CultistBossFireBall:
                    target.AddBuff(BuffID.OnFire, 300);
                    target.AddBuff(BuffID.Burning, 120);
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.betsyBoss, NPCID.DD2Betsy))
                    {
                        //target.AddBuff(BuffID.OnFire, 600);
                        //target.AddBuff(BuffID.Ichor, 600);
                        target.AddBuff(BuffID.WitheredArmor, 300);
                        target.AddBuff(BuffID.WitheredWeapon, 300);
                        target.AddBuff(BuffID.Burning, 300);
                    }
                    break;

                case ProjectileID.CultistBossFireBallClone:
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 600);
                    break;

                case ProjectileID.PaladinsHammerHostile:
                    target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(ModContent.BuffType<Stunned>(), 60);
                    break;

                case ProjectileID.RuneBlast:
                    target.AddBuff(ModContent.BuffType<Hexed>(), 240);

                    if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<NPCs.DeviBoss.DeviBoss>()))
                    {
                        target.AddBuff(ModContent.BuffType<FlamesoftheUniverse>(), 60);
                        target.AddBuff(BuffID.Suffocation, 240);
                    }
                    break;

                case ProjectileID.PoisonSeedPlantera:
                    target.AddBuff(BuffID.Poisoned, 300);
                    goto case ProjectileID.SeedPlantera;
                case ProjectileID.SeedPlantera:
                case ProjectileID.ThornBall:
                    target.AddBuff(ModContent.BuffType<IvyVenom>(), 240);
                    break;

                case ProjectileID.DesertDjinnCurse:
                    if (target.ZoneCorrupt)
                        target.AddBuff(BuffID.CursedInferno, 240);
                    else if (target.ZoneCrimson)
                        target.AddBuff(BuffID.Ichor, 240);
                    break;

                case ProjectileID.BrainScramblerBolt:
                    target.AddBuff(ModContent.BuffType<Flipped>(), 60);
                    target.AddBuff(ModContent.BuffType<Unstable>(), 60);
                    break;

                case ProjectileID.MartianTurretBolt:
                case ProjectileID.GigaZapperSpear:
                    target.AddBuff(ModContent.BuffType<LightningRod>(), 300);
                    break;

                case ProjectileID.RayGunnerLaser:
                    target.AddBuff(BuffID.VortexDebuff, 180);
                    break;

                case ProjectileID.SaucerMissile:
                    target.AddBuff(ModContent.BuffType<ClippedWings>(), 300);
                    target.AddBuff(ModContent.BuffType<Crippled>(), 300);
                    break;

                case ProjectileID.SaucerLaser:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.UFOLaser:
                case ProjectileID.SaucerDeathray:
                    target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 360);
                    break;

                case ProjectileID.FlamingWood:
                case ProjectileID.GreekFire1:
                case ProjectileID.GreekFire2:
                case ProjectileID.GreekFire3:
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 180);
                    break;

                case ProjectileID.VortexAcid:
                case ProjectileID.VortexLaser:
                    target.AddBuff(ModContent.BuffType<LightningRod>(), 600);
                    target.AddBuff(ModContent.BuffType<ClippedWings>(), 300);
                    break;

                case ProjectileID.VortexLightning:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.LostSoulHostile:
                    //target.AddBuff(ModContent.BuffType<Hexed>(), 240);
                    target.AddBuff(ModContent.BuffType<ReverseManaFlow>(), 600);
                    break;

                case ProjectileID.InfernoHostileBlast:
                case ProjectileID.InfernoHostileBolt:
                    if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<NPCs.DeviBoss.DeviBoss>()))
                    {
                        if (Main.rand.NextBool(5))
                            target.AddBuff(ModContent.BuffType<Fused>(), 1800);
                    }
                    target.AddBuff(ModContent.BuffType<Jammed>(), 600);
                    break;

                case ProjectileID.ShadowBeamHostile:
                    if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<NPCs.DeviBoss.DeviBoss>()))
                    {
                        target.AddBuff(ModContent.BuffType<Rotting>(), 1800);
                        target.AddBuff(ModContent.BuffType<Shadowflame>(), 300);
                    }
                    target.AddBuff(ModContent.BuffType<Atrophied>(), 600);
                    break;

                case ProjectileID.PhantasmalDeathray:
                    target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 360);
                    break;

                case ProjectileID.PhantasmalBolt:
                case ProjectileID.PhantasmalEye:
                case ProjectileID.PhantasmalSphere:
                    target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 360);
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<NPCs.MutantBoss.MutantBoss>()))
                    {
                        target.GetModPlayer<FargoSoulsPlayer>().MaxLifeReduction += 100;
                        target.AddBuff(ModContent.BuffType<OceanicMaul>(), 5400);
                        target.AddBuff(ModContent.BuffType<MutantFang>(), 180);
                    }
                    break;

                case ProjectileID.RocketSkeleton:
                    target.AddBuff(BuffID.Dazed, 120);
                    target.AddBuff(ModContent.BuffType<Defenseless>(), 300);
                    break;

                case ProjectileID.FlamesTrap:
                case ProjectileID.GeyserTrap:
                case ProjectileID.Fireball:
                case ProjectileID.EyeBeam:
                    {
                        target.AddBuff(BuffID.OnFire, 300);
                        NPC npc = FargoSoulsUtil.NPCExists(NPC.golemBoss, NPCID.Golem);
                        if (npc != null)
                        {
                            target.AddBuff(BuffID.BrokenArmor, 600);
                            target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                            target.AddBuff(BuffID.WitheredArmor, 600);
                            if (Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16] == null || //outside temple
                                Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].WallType != WallID.LihzahrdBrickUnsafe)
                            {
                                target.AddBuff(BuffID.Burning, 120);
                            }
                        }

                        if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.championBoss, ModContent.NPCType<EarthChampion>()))
                        {
                            target.AddBuff(BuffID.Burning, 300);
                        }

                        if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.championBoss, ModContent.NPCType<TerraChampion>()))
                        {
                            target.AddBuff(BuffID.OnFire, 600);
                            target.AddBuff(ModContent.BuffType<LivingWasteland>(), 600);
                        }
                    }
                    break;

                case ProjectileID.SpikyBallTrap:
                    /*if (NPC.golemBoss != -1 && Main.npc[NPC.golemBoss].active && Main.npc[NPC.golemBoss].type == NPCID.Golem)
                    {
                        target.AddBuff(BuffID.BrokenArmor, 600);
                        target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                        target.AddBuff(BuffID.WitheredArmor, 600);
                    }*/
                    break;

                case ProjectileID.DD2BetsyFireball:
                case ProjectileID.DD2BetsyFlameBreath:
                    //target.AddBuff(BuffID.OnFire, 600);
                    //target.AddBuff(BuffID.Ichor, 600);
                    target.AddBuff(BuffID.WitheredArmor, 300);
                    target.AddBuff(BuffID.WitheredWeapon, 300);
                    target.AddBuff(BuffID.Burning, 300);
                    break;

                case ProjectileID.DD2DrakinShot:
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 600);
                    break;

                case ProjectileID.NebulaSphere:
                case ProjectileID.NebulaLaser:
                case ProjectileID.NebulaBolt:
                    target.AddBuff(ModContent.BuffType<Berserked>(), 300);
                    target.AddBuff(ModContent.BuffType<Lethargic>(), 300);
                    break;

                case ProjectileID.StardustJellyfishSmall:
                case ProjectileID.StardustSoldierLaser:
                case ProjectileID.Twinkle:
                    target.AddBuff(BuffID.Obstructed, 20);
                    target.AddBuff(BuffID.Blackout, 300);
                    break;

                case ProjectileID.Sharknado:
                case ProjectileID.Cthulunado:
                    target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                    target.AddBuff(ModContent.BuffType<OceanicMaul>(), 20 * 60);
                    target.GetModPlayer<FargoSoulsPlayer>().MaxLifeReduction += FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBossEX, NPCID.DukeFishron) ? 100 : 25;
                    break;

                case ProjectileID.FlamingScythe:
                    target.AddBuff(BuffID.OnFire, 900);
                    target.AddBuff(ModContent.BuffType<LivingWasteland>(), 900);
                    break;

                case ProjectileID.FrostWave:
                case ProjectileID.FrostShard:
                    target.AddBuff(ModContent.BuffType<Hypothermia>(), 600);
                    break;

                case ProjectileID.SnowBallHostile:
                    target.GetModPlayer<FargoSoulsPlayer>().AddBuffNoStack(BuffID.Frozen, 45);
                    break;

                case ProjectileID.BulletSnowman:
                    target.AddBuff(ModContent.BuffType<Hypothermia>(), 600);
                    break;

                case ProjectileID.UnholyTridentHostile:
                    target.AddBuff(BuffID.Darkness, 300);
                    target.AddBuff(BuffID.Blackout, 300);
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 600);
                    //target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 180);
                    break;

                case ProjectileID.BombSkeletronPrime:
                    target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                    break;

                case ProjectileID.DeathLaser:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.retiBoss, NPCID.Retinazer))
                        target.AddBuff(BuffID.Ichor, 600);
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
                        target.AddBuff(BuffID.Electrified, 60);
                    break;

                case ProjectileID.BulletDeadeye:
                case ProjectileID.CannonballHostile:
                    target.AddBuff(ModContent.BuffType<Defenseless>(), 600);
                    target.AddBuff(ModContent.BuffType<Midas>(), 900);
                    break;

                case ProjectileID.AncientDoomProjectile:
                    target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 300);
                    target.AddBuff(ModContent.BuffType<Shadowflame>(), 300);
                    break;

                case ProjectileID.SandnadoHostile:
                    if (!target.HasBuff(BuffID.Dazed))
                        target.AddBuff(BuffID.Dazed, 120);
                    break;

                case ProjectileID.DD2OgreSmash:
                    target.AddBuff(BuffID.BrokenArmor, 300);
                    break;

                case ProjectileID.DD2OgreStomp:
                    target.AddBuff(BuffID.Dazed, 120);
                    break;

                case ProjectileID.DD2DarkMageBolt:
                    target.AddBuff(ModContent.BuffType<Hexed>(), 240);
                    break;

                case ProjectileID.IceSpike:
                    //target.AddBuff(BuffID.Slimed, 120);
                    target.AddBuff(ModContent.BuffType<Hypothermia>(), 300);
                    break;

                case ProjectileID.JungleSpike:
                    //target.AddBuff(BuffID.Slimed, 120);
                    target.AddBuff(ModContent.BuffType<Infested>(), 300);
                    break;

                default:
                    break;
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.PreKill(projectile, timeLeft);

            if (projectile.owner == Main.myPlayer && HasKillCooldown)
            {
                if (Main.player[projectile.owner].GetModPlayer<EModePlayer>().MasomodeCrystalTimer > 60)
                    return false;

                Main.player[projectile.owner].GetModPlayer<EModePlayer>().MasomodeCrystalTimer += 12;
                return true;
            }

            return base.PreKill(projectile, timeLeft);
        }

        public override Color? GetAlpha(Projectile projectile, Color lightColor)
        {
            if (!FargoSoulsWorld.EternityMode)
                return base.GetAlpha(projectile, lightColor);

            if ((projectile.type == ProjectileID.PoisonSeedPlantera || projectile.type == ProjectileID.SeedPlantera)
                && counter % 8 < 4)
            {
                return new Color(255, 255, 255, 0) * projectile.Opacity;
            }

            return base.GetAlpha(projectile, lightColor);
        }
    }
}
