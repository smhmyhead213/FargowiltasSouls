using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Projectiles;
using FargowiltasSouls.Projectiles.Deathrays;
using FargowiltasSouls.Projectiles.DeviBoss;
using FargowiltasSouls.Projectiles.Masomode;
using FargowiltasSouls.Items.Accessories.Masomode;
using FargowiltasSouls.Items.Materials;
using FargowiltasSouls.Items.Placeables;
using FargowiltasSouls.Items.Summons;
using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.ItemDropRules.Conditions;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using FargowiltasSouls.Items.Pets;
using FargowiltasSouls.Items.BossBags;
using FargowiltasSouls.Patreon.Phupperbat;
using Terraria.Localization;

namespace FargowiltasSouls.NPCs.DeviBoss
{
    [AutoloadBossHead]
    public class DeviBoss : ModNPC
    {
        public bool playerInvulTriggered;
        private bool droppedSummon = false;

        public int[] attackQueue = new int[4];
        public int lastStrongAttack;
        public bool ignoreMoney;

        public int ringProj, spriteProj;

        //private bool ContentModLoaded => Fargowiltas.Instance.CalamityLoaded || Fargowiltas.Instance.ThoriumLoaded || Fargowiltas.Instance.SoALoaded || Fargowiltas.Instance.MasomodeEXLoaded;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Deviantt");

            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
            NPCID.Sets.DebuffImmunitySets.Add(NPC.type, new Terraria.DataStructures.NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[]
                {
                    BuffID.Confused,
                    BuffID.Chilled,
                    BuffID.OnFire,
                    BuffID.Suffocation,
                    BuffID.Lovestruck,
                    ModContent.BuffType<Lethargic>(),
                    ModContent.BuffType<ClippedWings>()
                }
            });
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.FargowiltasSouls.Bestiary.DeviBoss")
            });
        }

        /*public override bool Autoload(ref string name)
        {
            return false;
        }*/

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 120;
            NPC.damage = 64;
            NPC.defense = 10;
            NPC.lifeMax = 5000;
            if (FargoSoulsWorld.MasochistModeReal)
                NPC.lifeMax = (int)(NPC.lifeMax * 1.2);
            NPC.HitSound = SoundID.NPCHit9;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.npcSlots = 50f;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            NPC.timeLeft = NPC.activeTime * 30;

            Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/LexusCyanixs") : MusicID.OtherworldlyHallow;
            SceneEffectPriority = SceneEffectPriority.BossMedium;

            //MusicPriority = (MusicPriority)10;

            NPC.value = Item.buyPrice(0, 5);

            //if (ContentModLoaded) NPC.lifeMax = (int)(NPC.lifeMax * 1.5);
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = (int)(NPC.damage * 0.5f);
            NPC.lifeMax = (int)(NPC.lifeMax /** 0.5f*/ * bossLifeScale);
        }

        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            CooldownSlot = 1;
            return NPC.Distance(FargoSoulsUtil.ClosestPointInHitbox(target, NPC.Center)) < Player.defaultHeight;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.type == ModContent.Find<ModNPC>("Fargowiltas", "Deviantt").Type
                || target.type == ModContent.Find<ModNPC>("Fargowiltas", "Abominationn").Type
                || target.type == ModContent.Find<ModNPC>("Fargowiltas", "Mutant").Type)
                return false;

            return base.CanHitNPC(target);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            writer.Write(attackQueue[0]);
            writer.Write(attackQueue[1]);
            writer.Write(attackQueue[2]);
            writer.Write(attackQueue[3]);
            writer.Write(ignoreMoney);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            attackQueue[0] = reader.ReadInt32();
            attackQueue[1] = reader.ReadInt32();
            attackQueue[2] = reader.ReadInt32();
            attackQueue[3] = reader.ReadInt32();
            ignoreMoney = reader.ReadBoolean();
        }

        private bool ProjectileExists(int id, int type)
        {
            return id > -1 && id < Main.maxProjectiles && Main.projectile[id].active && Main.projectile[id].type == type;
        }

        public override void AI()
        {
            EModeGlobalNPC.deviBoss = NPC.whoAmI;

            const int platinumToBribe = 10;

            if (NPC.localAI[3] == 0)
            {
                NPC.TargetClosest();
                if (NPC.timeLeft < 30)
                    NPC.timeLeft = 30;

                if (NPC.Distance(Main.player[NPC.target].Center) < 2000)
                {
                    NPC.localAI[3] = 1;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);

                    do
                    {
                        RefreshAttackQueue();
                    } while (attackQueue[0] == 3 || attackQueue[0] == 5 || attackQueue[0] == 9 || attackQueue[0] == 10);
                    //don't start with wyvern, mage spam, frostballs, baby guardian
                }
            }
            /*else if (NPC.localAI[3] == 1)
            {
                Aura(2000f, ModContent.BuffType<GodEater>(), true, 86);
            }*/
            /*else if (Main.player[Main.myPlayer].active && NPC.Distance(Main.player[Main.myPlayer].Center) < 3000f)
            {
                if (FargoSoulsWorld.MasochistMode)
                    Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<DeviPresence>(), 2);
            }*/

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!ProjectileExists(ringProj, ModContent.ProjectileType<DeviRitual2>()))
                    ringProj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual2>(), 0, 0f, Main.myPlayer, 0f, NPC.whoAmI);

                if (!ProjectileExists(spriteProj, ModContent.ProjectileType<Projectiles.DeviBoss.DeviBoss>()))
                {
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        int number = 0;
                        for (int index = 999; index >= 0; --index)
                        {
                            if (!Main.projectile[index].active)
                            {
                                number = index;
                                break;
                            }
                        }
                        if (number >= 0)
                        {
                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                Projectile projectile = Main.projectile[number];
                                projectile.SetDefaults(ModContent.ProjectileType<Projectiles.DeviBoss.DeviBoss>());
                                projectile.Center = NPC.Center;
                                projectile.owner = Main.myPlayer;
                                projectile.velocity.X = 0;
                                projectile.velocity.Y = 0;
                                projectile.damage = 0;
                                projectile.knockBack = 0f;
                                projectile.identity = number;
                                projectile.gfxOffY = 0f;
                                projectile.stepSpeed = 1f;
                                projectile.ai[1] = NPC.whoAmI;

                                spriteProj = number;
                            }
                        }
                    }
                    else //server
                    {
                        spriteProj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.DeviBoss.DeviBoss>(), 0, 0f, Main.myPlayer, 0, NPC.whoAmI);
                    }
                }
            }

            int projectileDamage = FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, NPC.localAI[3] > 1 ? 1 : 0.8f);

            Player player = Main.player[NPC.target];
            NPC.direction = NPC.spriteDirection = NPC.Center.X < player.Center.X ? 1 : -1;
            Vector2 targetPos;

            void StrongAttackTeleport(Vector2 teleportTarget = default)
            {
                const float range = 450f;
                if (NPC.Distance(player.Center) < range)
                    return;

                TeleportDust();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (teleportTarget != default)
                        NPC.Center = teleportTarget;
                    else if (player.velocity == Vector2.Zero)
                        NPC.Center = player.Center + range * Vector2.UnitX.RotatedByRandom(2 * Math.PI);
                    else
                        NPC.Center = player.Center + range * Vector2.Normalize(player.velocity);
                    NPC.velocity /= 2f;
                    NPC.netUpdate = true;
                }
                TeleportDust();
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
            };

            switch ((int)NPC.ai[0])
            {
                case -2: //ACTUALLY dead
                    if (!AliveCheck(player))
                        break;
                    NPC.velocity *= 0.9f;
                    NPC.dontTakeDamage = true;
                    for (int i = 0; i < 5; i++)
                    {
                        int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 86, 0f, 0f, 0, default(Color), 2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 12f;
                    }
                    if (++NPC.ai[1] > 180)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient && ModContent.TryFind("Fargowiltas", "Deviantt", out ModNPC modNPC) && !NPC.AnyNPCs(modNPC.Type))
                        {
                            int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, modNPC.Type);
                            if (n != Main.maxNPCs)
                            {
                                Main.npc[n].homeless = true;
                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                            }
                        }
                        NPC.life = 0;
                        NPC.dontTakeDamage = false;
                        NPC.checkDead();
                    }
                    break;

                case -1: //phase 2 transition
                    NPC.velocity *= 0.9f;
                    NPC.dontTakeDamage = true;
                    if (NPC.buffType[0] != 0)
                        NPC.DelBuff(0);
                    if (++NPC.ai[1] > 60)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 86, 0f, 0f, 0, default(Color), 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 4f;
                        }
                        NPC.localAI[3] = 2; //npc marks p2
                        if (FargoSoulsWorld.EternityMode)
                        {
                            int heal = (int)(NPC.lifeMax / 90 * Main.rand.NextFloat(1f, 1.5f));
                            NPC.life += heal;
                            if (NPC.life > NPC.lifeMax)
                                NPC.life = NPC.lifeMax;
                            CombatText.NewText(NPC.Hitbox, CombatText.HealLife, heal);
                        }
                        if (NPC.ai[1] > 150)
                        {
                            RefreshAttackQueue();
                            attackQueue[3] = 15; //always do sparkling love
                            NPC.localAI[2] = NPC.localAI[3] > 1 ? 1 : 0;
                            GetNextAttack();
                        }
                    }
                    else if (NPC.ai[1] == 60)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                    }
                    break;

                case 0: //track player, decide which attacks to use
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    NPC.dontTakeDamage = false;

                    targetPos = player.Center;
                    targetPos.X += 500 * (NPC.Center.X < targetPos.X ? -1 : 1);
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, NPC.localAI[3] > 0 ? 0.15f : 2f, NPC.localAI[3] > 0 ? 12f : 1200f);

                    if (NPC.localAI[3] > 0) //in range, fight has begun, choose attacks
                    {
                        NPC.netUpdate = true;
                        GetNextAttack();
                    }
                    break;

                case 1: //teleport marx hammers
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    if (NPC.localAI[1] == 0) //pick random number of teleports to do
                    {
                        NPC.localAI[1] = NPC.localAI[3] > 1 ? Main.rand.Next(3, 10) : Main.rand.Next(3, 6);
                        NPC.netUpdate = true;
                    }

                    NPC.velocity = Vector2.Zero;
                    if (++NPC.ai[1] > (NPC.localAI[3] > 1 ? 10 : 20) && NPC.ai[2] < NPC.localAI[1])
                    {
                        //NPC.localAI[1] = 0;
                        NPC.ai[1] = 0;
                        NPC.ai[2]++;

                        TeleportDust();
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool wasOnLeft = NPC.Center.X < player.Center.X;
                            NPC.Center = player.Center + 200 * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, 2 * (float)Math.PI));
                            if (wasOnLeft ? NPC.Center.X < player.Center.X : NPC.Center.X > player.Center.X)
                            {
                                float x = player.Center.X - NPC.Center.X;
                                NPC.position.X += x * 2;
                            }
                            NPC.netUpdate = true;
                        }
                        TeleportDust();
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);

                        if (NPC.ai[2] == NPC.localAI[1])
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = -1; i <= 1; i += 2)
                                {
                                    for (int j = 0; j < 3; j++)
                                    {
                                        float ai1 = MathHelper.ToRadians(90 + 15) - MathHelper.ToRadians(30) * j;
                                        ai1 *= i;
                                        ai1 = ai1 / 60 * 2;
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -Vector2.UnitY, ModContent.ProjectileType<DeviHammerHeld>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, ai1);
                                    }
                                }
                            }
                        }
                    }

                    if (NPC.ai[1] == 60) //finished all the prior teleports, now attack
                    {
                        NPC.netUpdate = true;
                        
                        FargoSoulsUtil.DustRing(NPC.Center, 36, 246, 9f, default, 3f, true);

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item92, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient) //hammers
                        {
                            const float retiRad = 150;
                            const float spazRad = 100;
                            const int retiTime = 45;
                            const int spazTime = 45;

                            float retiSpeed = 2 * (float)Math.PI * retiRad / retiTime;
                            float spazSpeed = 2 * (float)Math.PI * spazRad / spazTime;
                            float retiAcc = retiSpeed * retiSpeed / retiRad * NPC.direction;
                            float spazAcc = spazSpeed * spazSpeed / spazRad * -NPC.direction;
                            
                            for (int i = 0; i < 4; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(Math.PI / 2 * i) * retiSpeed, ModContent.ProjectileType<DeviHammer>(), projectileDamage, 0f, Main.myPlayer, retiAcc, retiTime);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(Math.PI / 2 * i + Math.PI / 4) * spazSpeed, ModContent.ProjectileType<DeviHammer>(), projectileDamage, 0f, Main.myPlayer, spazAcc, spazTime);
                            }
                        }
                    }
                    else if (NPC.ai[1] > 90)
                    {
                        NPC.netUpdate = true;
                        if (NPC.localAI[3] > 1 && ++NPC.localAI[0] < 3)
                        {
                            NPC.ai[2] = 0; //reset tp counter and attack again
                            NPC.localAI[1] = 0;
                        }
                        else
                        {
                            GetNextAttack();
                        }
                    }
                    break;

                case 2: //heart barrages
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    targetPos = player.Center;
                    targetPos.X += 400 * (NPC.Center.X < targetPos.X ? -1 : 1);
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.2f);

                    if (--NPC.ai[1] < 0)
                    {
                        NPC.netUpdate = true;
                        NPC.ai[1] = 75;
                        if (++NPC.ai[2] > 3)
                        {
                            GetNextAttack();
                        }
                        else
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int damage = (int)(NPC.damage / 3.2); //comes out to 20 raw, fake hearts ignore the usual multipliers

                                Vector2 spawnVel = NPC.DirectionFrom(Main.player[NPC.target].Center) * 10f;
                                for (int i = -3; i < 3; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, spawnVel.RotatedBy(Math.PI / 7 * i),
                                        ModContent.ProjectileType<FakeHeart2>(), damage, 0f, Main.myPlayer, 20, 30);
                                }
                                if (NPC.localAI[3] > 1)
                                {
                                    for (int i = -5; i < 5; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 1.5f * spawnVel.RotatedBy(Math.PI / 10 * i),
                                            ModContent.ProjectileType<FakeHeart2>(), damage, 0f, Main.myPlayer, 20, 40 + 5 * Math.Abs(i));
                                    }
                                }
                            }
                        }
                        break;
                    }
                    break;

                case 3: //slow while shooting wyvern orb spirals
                    if (!AliveCheck(player) || Phase2Check())
                        break;
                    
                    targetPos = player.Center + player.DirectionTo(NPC.Center) * 375;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.15f);

                    if (--NPC.ai[1] < 0)
                    {
                        NPC.netUpdate = true;
                        NPC.ai[1] = 150;

                        if (++NPC.ai[2] > 3)
                        {
                            GetNextAttack();
                        }
                        else
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int max = NPC.localAI[3] > 1 ? 8 : 12;
                                Vector2 vel = Vector2.Normalize(NPC.velocity);
                                for (int i = 0; i < max; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(2 * Math.PI / max * i), ModContent.ProjectileType<DeviLightBall>(), projectileDamage, 0f, Main.myPlayer, 0f, .008f * NPC.direction);
                                    if (NPC.localAI[3] > 1)
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(2 * Math.PI / max * i), ModContent.ProjectileType<DeviLightBall>(), projectileDamage, 0f, Main.myPlayer, 0f, .008f * -NPC.direction);
                                }
                            }
                        }
                    }
                    break;

                case 4: //mimics
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    targetPos = player.Center;
                    targetPos.X += 300 * (NPC.Center.X < targetPos.X ? -1 : 1);
                    targetPos.Y -= 300;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.15f);

                    if (++NPC.ai[1] < 120)
                    {
                        if (++NPC.ai[2] > 20)
                        {
                            NPC.ai[2] = 0;

                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);

                            int delay = NPC.localAI[3] > 1 ? 45 : 60;
                            Vector2 target = player.Center;
                            target.Y -= 400;
                            Vector2 speed = (target - NPC.Center) / delay;

                            for (int i = 0; i < 20; i++) //dust spray
                                Dust.NewDust(NPC.Center, 0, 0, Main.rand.NextBool() ? DustID.GoldFlame : DustID.SilverCoin, speed.X, speed.Y, 0, default(Color), 2f);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int type = ModContent.ProjectileType<DeviMimic>();
                                float ai0 = player.position.Y - 16;

                                if (NPC.localAI[3] > 1)
                                {
                                    type = ModContent.ProjectileType<DeviBigMimic>();
                                    ai0 = player.whoAmI;
                                }

                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed, type, projectileDamage, 0f, Main.myPlayer, ai0, delay);
                            }
                        }
                    }
                    else if (NPC.ai[1] == 180) //big wave of mimics, aimed ahead of you
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);

                        int modifier = 150;
                        if (player.velocity.X != 0)
                            modifier *= Math.Sign(player.velocity.X);
                        else
                            modifier *= Math.Sign(player.Center.X - NPC.Center.X);

                        Vector2 target = player.Center;
                        target.Y -= 400;

                        for (int j = 0; j < 7; j++)
                        {
                            int delay = NPC.localAI[3] > 1 ? 45 : 60;
                            Vector2 speed = (target - NPC.Center) / delay;

                            for (int i = 0; i < 20; i++) //dust spray
                                Dust.NewDust(NPC.Center, 0, 0, Main.rand.NextBool() ? DustID.GoldFlame : DustID.SilverCoin, speed.X, speed.Y, 0, default(Color), 2f);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int type = ModContent.ProjectileType<DeviMimic>();
                                float ai0 = player.position.Y - 16;

                                if (NPC.localAI[3] > 1)
                                {
                                    type = ModContent.ProjectileType<DeviBigMimic>();
                                    ai0 = player.whoAmI;
                                }

                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed, type, projectileDamage, 0f, Main.myPlayer, ai0, delay);
                            }

                            target.X += modifier;
                        }
                    }
                    else if (NPC.ai[1] > 240)
                    {
                        GetNextAttack();
                    }
                    break;

                case 5: //frostballs and nados
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    targetPos = player.Center + player.DirectionTo(NPC.Center) * 350;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.2f);

                    if (++NPC.ai[1] > 360)
                    {
                        GetNextAttack();

                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].hostile && Main.projectile[i].type == ModContent.ProjectileType<FrostfireballHostile>()
                                && Main.projectile[i].timeLeft > 120)
                            {
                                Main.projectile[i].timeLeft = 120;
                            }
                        }
                    }
                    if (++NPC.ai[2] > (NPC.localAI[3] > 1 ? 10 : 20))
                    {
                        NPC.netUpdate = true;
                        NPC.ai[2] = 0;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(4f, 0f).RotatedBy(Main.rand.NextDouble() * Math.PI * 2),
                                ModContent.ProjectileType<FrostfireballHostile>(), projectileDamage, 0f, Main.myPlayer, NPC.target, 15f);
                        }
                    }
                    if (NPC.localAI[3] > 1 && --NPC.ai[3] < 0) //spawn sandnado
                    {
                        NPC.netUpdate = true;
                        NPC.ai[3] = 110;

                        Vector2 target = player.Center;
                        target.X += player.velocity.X * 90;
                        target.Y -= 150;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), target, Vector2.Zero, ProjectileID.SandnadoHostileMark, 0, 0f, Main.myPlayer);

                        int length = (int)NPC.Distance(target) / 10;
                        Vector2 offset = NPC.DirectionTo(target) * 10f;
                        for (int i = 0; i < length; i++) //dust warning line for sandnado
                        {
                            int d = Dust.NewDust(NPC.Center + offset * i, 0, 0, 269, 0f, 0f, 0, new Color());
                            Main.dust[d].noLight = true;
                            Main.dust[d].scale = 1.25f;
                        }
                    }
                    break;

                case 6: //rune wizard
                    if (!AliveCheck(player) || Phase2Check())
                        break;
                    
                    EModeGlobalNPC.Aura(NPC, 450, true, 74, Color.GreenYellow, ModContent.BuffType<Hexed>(), ModContent.BuffType<Crippled>(), BuffID.Dazed, BuffID.OgreSpit);
                    EModeGlobalNPC.Aura(NPC, 150, false, 73, default, ModContent.BuffType<Hexed>(), ModContent.BuffType<Crippled>(), BuffID.Dazed, BuffID.OgreSpit);

                    NPC.velocity = Vector2.Zero;
                    if (++NPC.ai[1] == 1)
                    {
                        TeleportDust();
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool wasOnLeft = NPC.Center.X < player.Center.X;
                            NPC.Center = player.Center + 300 * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, 2 * (float)Math.PI));
                            if (wasOnLeft ? NPC.Center.X < player.Center.X : NPC.Center.X > player.Center.X)
                            {
                                float x = player.Center.X - NPC.Center.X;
                                NPC.position.X += x * 2;
                            }
                            NPC.netUpdate = true;
                        }
                        TeleportDust();
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
                    }
                    else if (NPC.ai[1] == 50)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = -1; i <= 1; i++) //rune blast spread
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center,
                                    12f * NPC.DirectionTo(player.Center).RotatedBy(MathHelper.ToRadians(5) * i),
                                    ProjectileID.RuneBlast, projectileDamage, 0f, Main.myPlayer);

                            if (NPC.localAI[3] > 1) //rune blast ring
                            {
                                Vector2 vel = NPC.DirectionFrom(Main.player[NPC.target].Center) * 8;
                                for (int i = 0; i < 5; i++)
                                {
                                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(2 * Math.PI / 5 * i),
                                        ProjectileID.RuneBlast, projectileDamage, 0f, Main.myPlayer, 1);
                                    if (p != Main.maxProjectiles)
                                        Main.projectile[p].timeLeft = 300;
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] > 100)
                    {
                        if (++NPC.ai[2] > 3)
                        {
                            GetNextAttack();

                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                if (Main.projectile[i].active && Main.projectile[i].hostile && Main.projectile[i].type == ProjectileID.RuneBlast
                                    && Main.projectile[i].timeLeft > 60)
                                {
                                    Main.projectile[i].timeLeft = 60;
                                }
                            }
                        }
                        else
                        {
                            NPC.netUpdate = true;
                            NPC.ai[1] = 0;
                        }
                    }
                    break;

                case 7: //moth dust charges
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    if (NPC.localAI[0] == 0) //teleport behind you
                    {
                        NPC.localAI[0] = 1;
                        NPC.ai[1] = -45;

                        TeleportDust();
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            bool wasOnLeft = NPC.Center.X < player.Center.X;
                            NPC.Center = player.Center;
                            NPC.position.X += wasOnLeft ? 400 : -400;
                            NPC.netUpdate = true;
                        }
                        TeleportDust();

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(NPC.Center.X < player.Center.X ? -1f : 1f, -1f),
                                ModContent.ProjectileType<DeviSparklingLoveSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 0.0001f * Math.Sign(player.Center.X - NPC.Center.X));
                        }
                    }

                    if (++NPC.ai[3] > 2)
                    {
                        NPC.ai[3] = 0;

                        if (Main.netMode != NetmodeID.MultiplayerClient) //make moth dust trail
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Unit() * 2f, ModContent.ProjectileType<MothDust>(), projectileDamage, 0f, Main.myPlayer);
                    }

                    NPC.velocity *= 0.9f;
                    if (++NPC.ai[1] > (NPC.localAI[3] > 1 ? 45 : 60))
                    {
                        NPC.netUpdate = true;
                        if (++NPC.ai[2] > 5)
                        {
                            GetNextAttack();
                        }
                        else
                        {
                            NPC.ai[0]++;
                            NPC.ai[1] = 0;
                            NPC.velocity = NPC.DirectionTo(player.Center + player.velocity) * 20f;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float rotation = MathHelper.Pi * 1.5f * (NPC.ai[2] % 2 == 0 ? 1 : -1);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity).RotatedBy(-rotation / 2),
                                    ModContent.ProjectileType<DeviSparklingLoveSmall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, rotation / 60 * 2);
                            }
                        }
                    }
                    break;

                case 8: //while dashing
                    if (Phase2Check())
                        break;

                    NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);

                    if (++NPC.ai[3] > 2)
                    {
                        NPC.ai[3] = 0;

                        if (Main.netMode != NetmodeID.MultiplayerClient) //make moth dust trail
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Unit() * 2f, ModContent.ProjectileType<MothDust>(), projectileDamage, 0f, Main.myPlayer);
                    }

                    if (++NPC.ai[1] > 30)
                    {
                        NPC.netUpdate = true;
                        NPC.ai[0]--;
                        NPC.ai[1] = 0;
                    }
                    break;

                case 9: //mage skeleton attacks
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    NPC.velocity = NPC.DirectionTo(player.Center + NPC.DirectionFrom(player.Center) * 80) * 2f;

                    if (++NPC.ai[1] == 1)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -1);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 10, NPC.whoAmI);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 10, NPC.whoAmI);
                        }
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);
                    }
                    else if (NPC.ai[1] < 120) //spam shadowbeams after delay
                    {
                        if (NPC.ai[3] <= 0) //store rotation briefly before shooting
                            NPC.localAI[0] = NPC.DirectionTo(player.Center).ToRotation();

                        if (++NPC.ai[2] > 90)
                        {
                            if (++NPC.ai[3] > (NPC.localAI[3] > 1 ? 5 : 8))
                            {
                                NPC.ai[3] = 0;

                                if (Main.netMode != NetmodeID.MultiplayerClient) //shoot a shadowbeam
                                {
                                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 6f * Vector2.UnitX.RotatedBy(NPC.localAI[0]), ProjectileID.ShadowBeamHostile, projectileDamage, 0f, Main.myPlayer);
                                    if (p != Main.maxProjectiles)
                                        Main.projectile[p].timeLeft = 300;
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] < 240)
                    {
                        NPC.ai[3] = 0;
                        NPC.localAI[0] = 0;

                        if (++NPC.ai[2] > (NPC.localAI[3] > 1 ? 20 : 40)) //shoot diabolist bolts
                        {
                            NPC.ai[2] = 0;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float speed = NPC.localAI[3] > 1 ? 16 : 8;
                                Vector2 blastPos = NPC.Center + Main.rand.NextFloat(1, 2) * NPC.Distance(player.Center) * NPC.DirectionTo(player.Center);
                                int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed * NPC.DirectionTo(player.Center), ProjectileID.InfernoHostileBolt, projectileDamage, 0f, Main.myPlayer, blastPos.X, blastPos.Y);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 300;
                            }
                        }
                    }
                    else
                    {
                        NPC.velocity /= 2;

                        if (NPC.ai[1] == 300) //spray ragged caster bolts
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.ForceRoar, (int)NPC.Center.X, (int)NPC.Center.Y, -1, 1f, 0); //eoc roar

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int max = NPC.localAI[3] > 1 ? 30 : 20;
                                for (int i = 0; i < max; i++)
                                {
                                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextFloat(2f, 6f) * Vector2.UnitX.RotatedBy(Main.rand.NextFloat((float)Math.PI * 2)), ModContent.ProjectileType<DeviLostSoul>(), projectileDamage, 0f, Main.myPlayer);
                                    if (p != Main.maxProjectiles)
                                        Main.projectile[p].timeLeft = 300;
                                }
                            }
                        }

                        if (NPC.ai[1] > 360)
                        {
                            GetNextAttack();
                        }
                    }
                    break;

                case 10: //baby guardians
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    targetPos = player.Center;
                    targetPos.Y -= 400;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.3f);

                    if (NPC.ai[1] == 1) //tp above player
                    {
                        TeleportDust();
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.Center = player.Center;
                            NPC.position.X += 500 * (Main.rand.NextBool() ? -1 : 1);
                            NPC.position.Y -= Main.rand.NextFloat(300, 500);
                            NPC.netUpdate = true;
                        }
                        TeleportDust();
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
                    }

                    if (++NPC.ai[1] < 180)
                    {
                        //warning dust
                        for (int i = 0; i < 3; i++)
                        {
                            int d = Dust.NewDust(NPC.Center, 0, 0, DustID.Torch, 0f, 0f, 0, default(Color), 3f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].noLight = true;
                            Main.dust[d].velocity *= 12f;
                        }
                    }
                    else if (NPC.ai[1] == 180)
                    {
                        NPC.netUpdate = true;

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);

                        if (Main.netMode != NetmodeID.MultiplayerClient) //shoot guardians
                        {
                            for (int i = -1; i <= 1; i++) //left and right sides
                            {
                                if (i == 0)
                                    continue;

                                for (int j = -1; j <= 1; j++)
                                {
                                    Vector2 spawnPos = player.Center;
                                    spawnPos.X += 1200 * i;
                                    spawnPos.Y += 50 * j;
                                    Vector2 vel = 14 * Vector2.UnitX * -i;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<DeviGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3), 0f, Main.myPlayer);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (++NPC.ai[2] > 3)
                        {
                            NPC.ai[2] = 0;
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item21, NPC.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 spawnPos = player.Center;
                                spawnPos.X += Main.rand.Next(-200, 201);
                                spawnPos.Y += 700;
                                Vector2 vel = Main.rand.NextFloat(12, 16f) * Vector2.Normalize(player.Center - spawnPos);
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<DeviGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3), 0f, Main.myPlayer);
                            }
                        }

                        if (NPC.ai[1] > 360)
                        {
                            GetNextAttack();
                        }

                        if (NPC.localAI[3] > 1 && NPC.ai[1] == 270) //surprise!
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.ForceRoar, (int)NPC.Center.X, (int)NPC.Center.Y, -1, 1f, 0); //eoc roar

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = -1; i <= 1; i++) //left and right sides
                                {
                                    if (i == 0)
                                        continue;

                                    for (int j = -1; j <= 1; j++)
                                    {
                                        Vector2 spawnPos = player.Center;
                                        spawnPos.X += 1200 * i;
                                        spawnPos.Y += 60 * j;
                                        Vector2 vel = 14 * Vector2.UnitX * -i;
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, vel, ModContent.ProjectileType<DeviGuardian>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3), 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 11: //noah/irisu geyser rain
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    if (NPC.localAI[0] == 0 && NPC.localAI[1] == 0)
                    {
                        StrongAttackTeleport(player.Center + new Vector2(player.Center.X - NPC.Center.X, -Math.Abs(player.Center.Y - NPC.Center.Y)));

                        if (Main.netMode != NetmodeID.MultiplayerClient) //spawn ritual for strong attacks
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);

                        NPC.localAI[0] = NPC.Center.X;
                        NPC.localAI[1] = NPC.Center.Y;
                        NPC.netUpdate = true;
                    }

                    //Main.NewText(NPC.localAI[0].ToString() + ", " + NPC.localAI[1].ToString());

                    targetPos = player.Center;
                    if (NPC.Center.Y > player.Center.Y)
                        targetPos.X += 300 * (NPC.Center.X < targetPos.X ? -1 : 1);
                    targetPos.Y -= 350;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.15f);

                    if (++NPC.ai[1] < 120)
                    {
                        if (++NPC.ai[2] > 2)
                        {
                            NPC.ai[2] = 0;
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item44, NPC.Center);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -24 * Vector2.UnitY.RotatedBy((Main.rand.NextDouble() - 0.5) * Math.PI / 2),
                                    ModContent.ProjectileType<DeviVisualHeart>(), 0, 0f, Main.myPlayer);
                            }
                        }
                    }
                    else if (NPC.ai[1] < 420)
                    {
                        if (--NPC.ai[3] < 0)
                        {
                            NPC.netUpdate = true;
                            NPC.ai[3] = FargoSoulsWorld.MasochistModeReal ? 70 : 85;

                            NPC.ai[2] = NPC.ai[2] == 1 ? -1 : 1;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 speed = 24 * Vector2.UnitY.RotatedBy(MathHelper.ToRadians(10) * NPC.ai[2]);
                                int type = NPC.localAI[3] > 1 ? ModContent.ProjectileType<DeviRainHeart2>() : ModContent.ProjectileType<DeviRainHeart>();
                                int damage = NPC.localAI[3] > 1 ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3 ): FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);
                                int range = NPC.localAI[3] > 1 ? 8 : 10;
                                float spacing = 1200f / range;
                                float offset = Main.rand.NextFloat(-spacing, spacing);
                                for (int i = -range; i <= range; i++)
                                {
                                    Vector2 spawnPos = new Vector2(NPC.localAI[0], NPC.localAI[1]);
                                    spawnPos.X += spacing * i + offset;
                                    spawnPos.Y -= 1200;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, speed, type, damage, 0f, Main.myPlayer, 0f, NPC.whoAmI);
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] > 510)
                    {
                        GetNextAttack();
                    }
                    break;

                case 12: //lilith cross ray hearts
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    targetPos = player.Center + player.DirectionTo(NPC.Center) * 400;
                    if (NPC.Distance(targetPos) > 50)
                        Movement(targetPos, 0.3f);

                    if (NPC.localAI[0] == 0)
                    {
                        StrongAttackTeleport();
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient) //spawn ritual for strong attacks
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);

                        NPC.localAI[0] = 1;
                        NPC.netUpdate = true;
                    }

                    if (++NPC.ai[2] > (NPC.localAI[3] > 1 ? 75 : 100))
                    {
                        if (++NPC.ai[3] > (FargoSoulsWorld.MasochistModeReal ? 3 : 5))
                        {
                            NPC.ai[3] = 0;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 target = player.Center - NPC.Center;
                                target.X += Main.rand.Next(-75, 76);
                                target.Y += Main.rand.Next(-75, 76);

                                Vector2 speed = 2 * target / 90;
                                float acceleration = -speed.Length() / 90;

                                int damage = NPC.localAI[3] > 1 ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3 ): FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);
                                float rotation = FargoSoulsWorld.MasochistModeReal ? MathHelper.ToRadians(Main.rand.NextFloat(-10, 10)) : 0;

                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed, ModContent.ProjectileType<DeviEnergyHeart>(),
                                    damage, 0f, Main.myPlayer, rotation, acceleration);
                            }
                        }

                        if (NPC.ai[2] > 130)
                        {
                            NPC.netUpdate = true;
                            NPC.ai[2] = 0;
                        }
                    }

                    if (++NPC.ai[1] > (NPC.localAI[3] > 1 ? 450 : 480))
                    {
                        GetNextAttack();
                    }
                    break;

                case 13: //that one boss that was a bunch of gems burst rain but with butterflies
                    if (!AliveCheck(player) || Phase2Check())
                        break;

                    NPC.velocity = Vector2.Zero;

                    if (NPC.ai[2] == 0)
                    {
                        StrongAttackTeleport();

                        NPC.ai[2] = 1;
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float offset = Main.rand.NextFloat(600);
                            int damage = NPC.localAI[3] > 1 ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3 ): FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);
                            int max = 8;
                            for (int i = 0; i < max; i++) //make butterflies
                            {
                                Vector2 speed = new Vector2(Main.rand.NextFloat(40f), Main.rand.NextFloat(-20f, 20f));
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed, ModContent.ProjectileType<DeviButterfly>(),
                                   damage, 0f, Main.myPlayer, NPC.whoAmI, 300 / 4 * i + offset);
                            }
                        }
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient) //spawn ritual for strong attacks
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);
                    }

                    if (FargoSoulsWorld.MasochistModeReal && NPC.ai[1] % 240 == 90)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int max = NPC.localAI[3] > 1 ? 8 : 12;
                            Vector2 vel = 0.95f * Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
                            for (int i = 0; i < max; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(2 * Math.PI / max * i), ModContent.ProjectileType<DeviLightBall>(), projectileDamage, 0f, Main.myPlayer, 0f, .008f * NPC.direction);
                                if (NPC.localAI[3] > 1)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel.RotatedBy(2 * Math.PI / max * i), ModContent.ProjectileType<DeviLightBall>(), projectileDamage, 0f, Main.myPlayer, 0f, .008f * -NPC.direction);
                            }
                        }
                    }

                    if (++NPC.ai[1] > 480)
                    {
                        GetNextAttack();
                    }
                    break;

                case 14: //medusa ray
                    if ((NPC.ai[1] < 420 && !AliveCheck(player)) || Phase2Check())
                        break;

                    if (NPC.localAI[0] == 0)
                    {
                        StrongAttackTeleport();

                        NPC.localAI[0] = 1;
                        NPC.velocity = Vector2.Zero;

                        if (Main.netMode != NetmodeID.MultiplayerClient) //spawn ritual for strong attacks
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);
                    }

                    if (NPC.ai[3] < 4 && NPC.Distance(Main.LocalPlayer.Center) < 3000 && Collision.CanHitLine(NPC.Center, 0, 0, Main.LocalPlayer.Center, 0, 0)
                        && Math.Sign(Main.LocalPlayer.direction) == Math.Sign(NPC.Center.X - Main.LocalPlayer.Center.X)
                        && Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
                    {
                        Vector2 target = Main.LocalPlayer.Center - Vector2.UnitY * 12;
                        Vector2 source = NPC.Center - Vector2.UnitY * 6;
                        Vector2 distance = target - source;

                        int length = (int)distance.Length() / 10;
                        Vector2 offset = Vector2.Normalize(distance) * 10f;
                        for (int i = 0; i <= length; i++) //dust indicator
                        {
                            int d = Dust.NewDust(source + offset * i, 0, 0, DustID.GoldFlame, 0f, 0f, 0, new Color());
                            Main.dust[d].noLight = true;
                            Main.dust[d].noGravity = true;
                            Main.dust[d].scale = 1f;
                        }
                    }

                    if (++NPC.ai[2] > (FargoSoulsWorld.MasochistModeReal ? 30 : 60))
                    {
                        NPC.ai[2] = 0;
                        //only make rings in p2 and before firing ray
                        if (NPC.localAI[3] > 1 && NPC.ai[3] < 7 && !Main.player[NPC.target].stoned)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                const int max = 12;
                                int damage = NPC.localAI[3] > 1 ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3 ): FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);
                                for (int i = 0; i < max; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, 6f * NPC.DirectionTo(player.Center).RotatedBy(2 * Math.PI / max * i),
                                        ModContent.ProjectileType<DeviHeart>(), damage, 0f, Main.myPlayer);
                                }
                            }
                        }

                        if (++NPC.ai[3] < 4) //medusa warning
                        {
                            NPC.netUpdate = true;
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.ForceRoar, (int)NPC.Center.X, (int)NPC.Center.Y, -1, 1f, 0); //eoc roar

                            FargoSoulsUtil.DustRing(NPC.Center, 120, 228, 20f, default, 2f);

                            if (NPC.ai[3] == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, -1), ModContent.ProjectileType<DeviMedusa>(), 0, 0, Main.myPlayer);
                        }
                        else if (NPC.ai[3] == 4) //petrify
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCKilled, NPC.Center, 17);

                            if (NPC.Distance(Main.LocalPlayer.Center) < 3000 && Collision.CanHitLine(NPC.Center, 0, 0, Main.LocalPlayer.Center, 0, 0)
                                && Math.Sign(Main.LocalPlayer.direction) == Math.Sign(NPC.Center.X - Main.LocalPlayer.Center.X)
                                && Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
                            {
                                for (int i = 0; i < 40; i++) //petrify dust
                                {
                                    int d = Dust.NewDust(Main.LocalPlayer.Center, 0, 0, DustID.Stone, 0f, 0f, 0, default(Color), 2f);
                                    Main.dust[d].velocity *= 3f;
                                }

                                Main.LocalPlayer.AddBuff(BuffID.Stoned, 300);
                                if (Main.LocalPlayer.HasBuff(BuffID.Stoned))
                                    Main.LocalPlayer.AddBuff(BuffID.Featherfall, 300);

                                Projectile.NewProjectile(NPC.GetSource_FromThis(), Main.LocalPlayer.Center, new Vector2(0, -1), ModContent.ProjectileType<DeviMedusa>(), 0, 0, Main.myPlayer);
                            }
                        }
                        else if (NPC.ai[3] < 7) //ray warning
                        {
                            NPC.netUpdate = true;

                            FargoSoulsUtil.DustRing(NPC.Center, 160, 86, 40f, default, 2.5f);
                            
                            NPC.localAI[1] = NPC.DirectionTo(player.Center).ToRotation(); //store for aiming ray

                            if (NPC.ai[3] == 6 && Main.netMode != NetmodeID.MultiplayerClient) //final warning
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(NPC.localAI[1]), ModContent.ProjectileType<DeviDeathraySmall>(),
                                    0, 0f, Main.myPlayer, 0f, NPC.whoAmI);
                            }
                        }
                        else if (NPC.ai[3] == 7) //fire deathray
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)NPC.Center.X, (int)NPC.Center.Y, 0);

                            NPC.velocity = -3f * Vector2.UnitX.RotatedBy(NPC.localAI[1]);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(NPC.localAI[1]), ModContent.ProjectileType<DeviBigDeathray>(),
                                    FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);
                            }

                            const int ring = 160;
                            for (int i = 0; i < ring; ++i)
                            {
                                Vector2 vector2 = (-Vector2.UnitY.RotatedBy(i * 3.14159274101257 * 2 / ring) * new Vector2(8f, 16f)).RotatedBy(NPC.velocity.ToRotation());
                                int index2 = Dust.NewDust(NPC.Center, 0, 0, 86, 0.0f, 0.0f, 0, new Color(), 1f);
                                Main.dust[index2].scale = 5f;
                                Main.dust[index2].noGravity = true;
                                Main.dust[index2].position = NPC.Center;
                                Main.dust[index2].velocity = vector2 * 3f;
                            }
                        }
                    }

                    if (NPC.ai[3] >= 7) //firing laser dust
                    {
                        /*for (int i = 0; i < 5; i++)
                        {
                            int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 86, -NPC.velocity.X, -NPC.velocity.Y, 0, default(Color), 2.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 12f;
                        }*/
                    }
                    else //charge up dust
                    {
                        float num1 = 0.99f;
                        if (NPC.ai[3] >= 1f)
                            num1 = 0.79f;
                        if (NPC.ai[3] >= 2f)
                            num1 = 0.58f;
                        if (NPC.ai[3] >= 3f)
                            num1 = 0.43f;
                        if (NPC.ai[3] >= 4f)
                            num1 = 0.33f;
                        for (int i = 0; i < 9; ++i)
                        {
                            if (Main.rand.NextFloat() >= num1)
                            {
                                float f = Main.rand.NextFloat() * 6.283185f;
                                float num2 = Main.rand.NextFloat();
                                Dust dust = Dust.NewDustPerfect(NPC.Center + f.ToRotationVector2() * (110 + 600 * num2), 86, (f - 3.141593f).ToRotationVector2() * (14 + 8 * num2), 0, default, 1f);
                                dust.scale = 0.9f;
                                dust.fadeIn = 1.15f + num2 * 0.3f;
                                //dust.color = new Color(1f, 1f, 1f, num1) * (1f - num1);
                                dust.noGravity = true;
                                //dust.noLight = true;
                            }
                        }
                    }

                    if (NPC.localAI[1] != 0)
                        NPC.direction = NPC.spriteDirection = Math.Sign(NPC.localAI[1].ToRotationVector2().X);

                    if (++NPC.ai[1] > 600)//(NPC.localAI[3] > 1 ? 540 : 600))
                    {
                        GetNextAttack();
                    }
                    break;

                case 15: //sparkling love
                    if (NPC.localAI[0] == 0)
                    {
                        StrongAttackTeleport(player.Center + new Vector2(300 * Math.Sign(NPC.Center.X - player.Center.X), -100));

                        NPC.localAI[0] = 1;

                        if (Main.netMode != NetmodeID.MultiplayerClient) //spawn ritual for strong attacks
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DeviRitual>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0f, NPC.whoAmI);
                        }
                    }
                    
                    if (++NPC.ai[1] < 150)
                    {
                        NPC.velocity = Vector2.Zero;

                        if (NPC.ai[2] == 0) //spawn weapon, teleport
                        {
                            double angle = NPC.position.X < player.position.X ? -Math.PI / 4 : Math.PI / 4;
                            NPC.ai[2] = (float)angle * -4f / 30;

                            //spawn axe
                            const int loveOffset = 90;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + -Vector2.UnitY.RotatedBy(angle) * loveOffset, Vector2.Zero, ModContent.ProjectileType<DeviSparklingLove>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 2), 0f, Main.myPlayer, NPC.whoAmI, loveOffset);
                            }

                            //spawn hitboxes
                            const int spacing = 80;
                            Vector2 offset = -Vector2.UnitY.RotatedBy(angle) * spacing;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                void SpawnAxeHitbox(Vector2 spawnPos)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<DeviAxe>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 2), 0f, Main.myPlayer, NPC.whoAmI, NPC.Distance(spawnPos));
                                }

                                for (int i = 0; i < 8; i++)
                                    SpawnAxeHitbox(NPC.Center + offset * i);
                                for (int i = 1; i < 3; i++)
                                {
                                    SpawnAxeHitbox(NPC.Center + offset * 5 + offset.RotatedBy(-angle * 2) * i);
                                    SpawnAxeHitbox(NPC.Center + offset * 6 + offset.RotatedBy(-angle * 2) * i);
                                }
                            }

                            if (FargoSoulsWorld.MasochistModeReal && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Vector2 target = new Vector2(80f, 80f).RotatedBy(MathHelper.Pi / 2 * i);

                                    Vector2 speed = 2 * target / 90;
                                    float acceleration = -speed.Length() / 90;

                                    int damage = NPC.localAI[3] > 1 ? FargoSoulsUtil.ScaledProjectileDamage(NPC.damage, 4f / 3 ): FargoSoulsUtil.ScaledProjectileDamage(NPC.damage);

                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, speed, ModContent.ProjectileType<DeviEnergyHeart>(),
                                        damage, 0f, Main.myPlayer, 0, acceleration);
                                }
                            }
                        }

                        NPC.direction = NPC.spriteDirection = Math.Sign(NPC.ai[2]);
                    }
                    else if (NPC.ai[1] == 150) //start swinging
                    {
                        targetPos = player.Center;
                        targetPos.X -= 360 * Math.Sign(NPC.ai[2]);
                        //targetPos.Y -= 200;
                        NPC.velocity = (targetPos - NPC.Center) / 30;
                        NPC.netUpdate = true;

                        NPC.direction = NPC.spriteDirection = Math.Sign(NPC.ai[2]);

                        if (Math.Sign(targetPos.X - NPC.Center.X) != Math.Sign(NPC.ai[2]))
                            NPC.velocity.X *= 0.5f; //worse movement if you're behind her
                    }
                    else if (NPC.ai[1] < 180)
                    {
                        NPC.ai[3] += NPC.ai[2];
                        NPC.direction = NPC.spriteDirection = Math.Sign(NPC.ai[2]);
                    }
                    else
                    {
                        targetPos = player.Center + player.DirectionTo(NPC.Center) * 400;
                        if (NPC.Distance(targetPos) > 50)
                            Movement(targetPos, 0.2f);

                        if (NPC.ai[1] > 300)
                        {
                            GetNextAttack();
                        }
                    }
                    break;

                case 16: //pause between attacks
                    {
                        if (!AliveCheck(player) || Phase2Check())
                            break;

                        NPC.dontTakeDamage = false;

                        targetPos = player.Center + player.DirectionTo(NPC.Center) * 200;
                        Movement(targetPos, 0.1f);
                        if (NPC.Distance(player.Center) < 100)
                            Movement(targetPos, 0.5f);

                        int delay = 180;
                        if (FargoSoulsWorld.MasochistModeReal)
                        {
                            delay -= 60;
                            ignoreMoney = true;
                        }
                        if (FargoSoulsWorld.EternityMode)
                            delay -= 60;
                        if (NPC.localAI[3] > 1)
                            delay -= 30;
                        if (++NPC.ai[1] > delay)
                        {
                            NPC.netUpdate = true;
                            NPC.ai[0] = 16; //placeholder
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.ai[3] = 0;
                            NPC.localAI[0] = 0;
                            NPC.localAI[1] = 0;

                            if (!ignoreMoney && NPC.extraValue > Item.buyPrice(platinumToBribe))
                            {
                                NPC.ai[0] = 17;
                            }
                            else
                            {
                                NPC.ai[0] = attackQueue[(int)NPC.localAI[2]];

                                int threshold = attackQueue.Length; //only do super attacks in maso
                                if (!FargoSoulsWorld.EternityMode)
                                    threshold -= 1;
                                if (++NPC.localAI[2] >= threshold)
                                {
                                    NPC.localAI[2] = NPC.localAI[3] > 1 ? 1 : 0;
                                    RefreshAttackQueue();
                                }
                            }
                        }
                    }
                    break;

                case 17: //i got money
                    {
                        NPC.dontTakeDamage = true;
                        NPC.velocity *= 0.95f;
                        if (NPC.timeLeft < 600)
                            NPC.timeLeft = 600;

                        if (NPC.buffType[0] != 0)
                            NPC.DelBuff(0);

                        Rectangle displayPoint = new Rectangle(NPC.Hitbox.Center.X, NPC.Hitbox.Center.Y - NPC.height / 4, 2, 2);
                        
                        if (NPC.ai[1] == 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient) //clear my arena
                            {
                                for (int i = 0; i < Main.maxProjectiles; i++)
                                {
                                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<DeviRitual>() && Main.projectile[i].ai[1] == NPC.whoAmI)
                                    {
                                        Main.projectile[i].Kill();
                                    }
                                }
                            }
                        }
                        else if (NPC.ai[1] == 60)
                        {
                            CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Line1"));
                        }
                        else if (NPC.ai[1] == 150)
                        {
                            CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Line2"), true);
                        }
                        else if (NPC.ai[1] == 300)
                        {
                            CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Line3"), true);
                        }
                        else if (NPC.ai[1] == 450)
                        {
                            if (FargoSoulsWorld.downedDevi)
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Accept1"));
                            }
                            else
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Reject1"), true);
                            }
                        }
                        else if (NPC.ai[1] == 600)
                        {
                            if (FargoSoulsWorld.downedDevi)
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Accept2"), true);
                            }
                            else
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Reject2"), true);

                                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item28, player.Center);
                                Vector2 spawnPos = NPC.Center + Vector2.UnitX * NPC.width * 2 * (player.Center.X < NPC.Center.X ? -1 : 1);
                                for (int i = 0; i < 30; i++)
                                {
                                    int d = Dust.NewDust(spawnPos, 0, 0, 66, 0, 0, 0, default, 1f);
                                    Main.dust[d].noGravity = true;
                                    Main.dust[d].velocity *= 6f;
                                }
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, -1, -21);

                                    Item.NewItem(NPC.GetSource_Loot(), spawnPos, ItemID.PlatinumCoin, platinumToBribe);
                                }
                            }

                            NPC.extraValue -= Item.buyPrice(platinumToBribe);
                        }
                        else if (NPC.ai[1] == 900)
                        {
                            if (FargoSoulsWorld.downedDevi)
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Accept3"), true);
                            }
                            else
                            {
                                CombatText.NewText(displayPoint, Color.HotPink, Language.GetTextValue("DeviBribe.Reject3"), true);
                            }
                        }

                        if (++NPC.ai[1] > 1050)
                        {
                            ignoreMoney = true;
                            if (FargoSoulsWorld.downedDevi)
                            {
                                NPC.life = 0;
                                NPC.checkDead();
                            }
                            else
                            {
                                NPC.ai[0] = 16;
                                NPC.ai[1] = 0;
                                NPC.velocity = 20f * NPC.DirectionFrom(player.Center);
                            }
                            NPC.netUpdate = true;
                        }
                    }
                    break;

                default:
                    Main.NewText("UH OH, STINKY");
                    NPC.netUpdate = true;
                    NPC.ai[0] = 0;
                    goto case 0;
            }

            if (player.immune || player.hurtCooldowns[0] != 0 || player.hurtCooldowns[1] != 0)
                playerInvulTriggered = true;

            //drop summon
            if (!FargoSoulsWorld.downedDevi && Main.netMode != NetmodeID.MultiplayerClient && NPC.HasPlayerTarget && !droppedSummon)
            {
                Item.NewItem(NPC.GetSource_Loot(), player.Hitbox, ModContent.ItemType<DevisCurse>());
                droppedSummon = true;
            }
        }

        private void GetNextAttack()
        {
            NPC.TargetClosest();
            NPC.netUpdate = true;
            NPC.ai[0] = 16;// attackQueue[(int)NPC.localAI[2]];
            NPC.ai[1] = 0;
            NPC.ai[2] = 0;
            NPC.ai[3] = 0;
            NPC.localAI[0] = 0;
            NPC.localAI[1] = 0;
        }

        private void RefreshAttackQueue()
        {
            NPC.netUpdate = true;

            int[] newQueue = new int[4];
            for (int i = 0; i < 3; i++)
            {
                newQueue[i] = Main.rand.Next(1, 11);

                bool repeat = false;
                if (newQueue[i] == 8) //npc is the middle of an attack pattern, dont pick it
                    repeat = true;
                for (int j = 0; j < 3; j++) //cant pick attack that's queued in the previous set
                    if (newQueue[i] == attackQueue[j])
                        repeat = true;
                for (int j = i; j >= 0; j--) //can't pick attack that's already queued in npc set
                    if (i != j && newQueue[i] == newQueue[j])
                        repeat = true;

                if (repeat) //retry npc one if needed
                    i--;
            }

            do
            {
                newQueue[3] = Main.rand.Next(11, 16);
            }
            while (newQueue[3] == attackQueue[3] || newQueue[3] == lastStrongAttack || (newQueue[3] == 15 && NPC.localAI[3] <= 1));
            //don't do sparkling love in p1

            lastStrongAttack = attackQueue[3]; //a strong attack can't be used again for the next 2 checks
            attackQueue = newQueue;

            /*Main.NewText("queue: "
                + attackQueue[0].ToString() + " "
                + attackQueue[1].ToString() + " "
                + attackQueue[2].ToString() + " "
                + attackQueue[3].ToString());*/
        }

        private bool AliveCheck(Player player)
        {
            if ((!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 5000f) && NPC.localAI[3] > 0)
            {
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 5000f)
                {
                    if (NPC.timeLeft > 30)
                        NPC.timeLeft = 30;
                    NPC.velocity.Y -= 1f;
                    if (NPC.timeLeft == 1)
                    {
                        if (NPC.position.Y < 0)
                            NPC.position.Y = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient && ModContent.TryFind("Fargowiltas", "Deviantt", out ModNPC modNPC) && !NPC.AnyNPCs(modNPC.Type))
                        {
                            FargoSoulsUtil.ClearHostileProjectiles(2, NPC.whoAmI);
                            int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, modNPC.Type);
                            if (n != Main.maxNPCs)
                            {
                                Main.npc[n].homeless = true;
                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                            }
                        }
                    }
                    return false;
                }
            }
            if (NPC.timeLeft < 600)
                NPC.timeLeft = 600;
            return true;
        }

        private bool Phase2Check()
        {
            if (NPC.localAI[3] > 1)
                return false;

            if (NPC.life < NPC.lifeMax * 0.5 && Main.expertMode)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[0] = -1;
                    NPC.ai[1] = 0;
                    NPC.ai[2] = 0;
                    NPC.ai[3] = 0;
                    NPC.localAI[0] = 0;
                    NPC.localAI[1] = 0;
                    NPC.netUpdate = true;

                    FargoSoulsUtil.ClearHostileProjectiles(2, NPC.whoAmI);
                }
                return true;
            }
            return false;
        }

        private void Movement(Vector2 targetPos, float speedModifier, float cap = 12f)
        {
            if (Math.Abs(NPC.Center.X - targetPos.X) > 10)
            {
                if (NPC.Center.X < targetPos.X)
                {
                    NPC.velocity.X += speedModifier;
                    if (NPC.velocity.X < 0)
                        NPC.velocity.X += speedModifier * 2;
                }
                else
                {
                    NPC.velocity.X -= speedModifier;
                    if (NPC.velocity.X > 0)
                        NPC.velocity.X -= speedModifier * 2;
                }
            }
            if (NPC.Center.Y < targetPos.Y)
            {
                NPC.velocity.Y += speedModifier;
                if (NPC.velocity.Y < 0)
                    NPC.velocity.Y += speedModifier * 2;
            }
            else
            {
                NPC.velocity.Y -= speedModifier;
                if (NPC.velocity.Y > 0)
                    NPC.velocity.Y -= speedModifier * 2;
            }
            if (Math.Abs(NPC.velocity.X) > cap)
                NPC.velocity.X = cap * Math.Sign(NPC.velocity.X);
            if (Math.Abs(NPC.velocity.Y) > cap)
                NPC.velocity.Y = cap * Math.Sign(NPC.velocity.Y);
        }

        private void TeleportDust()
        {
            for (int index1 = 0; index1 < 25; ++index1)
            {
                int index2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, 272, 0f, 0f, 100, new Color(), 2f);
                Main.dust[index2].noGravity = true;
                Main.dust[index2].velocity *= 7f;
                Main.dust[index2].noLight = true;
                int index3 = Dust.NewDust(NPC.position, NPC.width, NPC.height, 272, 0f, 0f, 100, new Color(), 1f);
                Main.dust[index3].velocity *= 4f;
                Main.dust[index3].noGravity = true;
                Main.dust[index3].noLight = true;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<Lovestruck>(), 240);
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            for (int i = 0; i < 3; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, 86, 0f, 0f, 0, default(Color), 1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 3f;
            }
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            //if (Item.melee && !ContentModLoaded) damage = (int)(damage * 1.25);

            ModifyHitByAnything(player, ref damage, ref knockback, ref crit);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            //if ((projectile.melee || projectile.minion) && !ContentModLoaded) damage = (int)(damage * 1.25);

            ModifyHitByAnything(Main.player[projectile.owner], ref damage, ref knockback, ref crit);
        }

        public void ModifyHitByAnything(Player player, ref int damage, ref float knockback, ref bool crit)
        {
            if (player.loveStruck)
            {
                /*npc.life += damage;
                if (npc.life > npc.lifeMax)
                    npc.life = npc.lifeMax;
                CombatText.NewText(npc.Hitbox, CombatText.HealLife, damage);*/

                Vector2 speed = Main.rand.NextFloat(1, 2) * Vector2.UnitX.RotatedByRandom(Math.PI * 2);
                float ai1 = 30 + Main.rand.Next(30);
                Projectile.NewProjectile(NPC.GetSource_FromThis(), player.Center, speed, ModContent.ProjectileType<HostileHealingHeart>(), damage, 0f, Main.myPlayer, NPC.whoAmI, ai1);

                damage = 0;
                crit = false;
            }
        }

        public override bool CheckDead()
        {
            if (NPC.ai[0] == -2 && NPC.ai[1] >= 180)
                return true;

            NPC.life = 1;
            NPC.active = true;

            if (NPC.localAI[3] < 2)
            {
                NPC.localAI[3] = 2;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[0] > -2)
            {
                NPC.ai[0] = -2;
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                NPC.localAI[0] = 0;
                NPC.localAI[1] = 0;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                FargoSoulsUtil.ClearHostileProjectiles(2, NPC.whoAmI);
            }
            return false;
        }

        public override void OnKill()
        {
            base.OnKill();

            if (!playerInvulTriggered && FargoSoulsWorld.EternityMode)
            {
                Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ModContent.ItemType<BrokenBlade>());
                Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ModContent.ItemType<ChibiHat>());
                if (Main.bloodMoon)
                    Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ModContent.ItemType<VermillionTopHat>());
            }

            NPC.SetEventFlagCleared(ref FargoSoulsWorld.downedDevi, -1);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npcLoot);

            npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<DeviatingEnergy>(), 1, 15, 30));
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DeviBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeviTrophy>(), 10));

            LeadingConditionRule emodeRule = new LeadingConditionRule(new EModeDropCondition());
            emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<SparklingAdoration>()));
            npcLoot.Add(emodeRule);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }

        public override void FindFrame(int frameHeight)
        {
            if (++NPC.frameCounter > 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= 4 * frameHeight)
                    NPC.frame.Y = 0;
            }
        }

        public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
        {
            //spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Rectangle rectangle = NPC.frame;
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects effects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture2D13, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), NPC.GetAlpha(drawColor), NPC.rotation, origin2, NPC.scale, effects, 0);
            return false;
        }
    }
}