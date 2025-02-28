using System;
using System.Collections.Generic;
using System.Linq;
using FargowiltasSouls.Items.Accessories.Enchantments;
using FargowiltasSouls.Toggler;
using FargowiltasSouls.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Items.Weapons.SwarmDrops;
using FargowiltasSouls.Projectiles.BossWeapons;
using FargowiltasSouls.Projectiles.Souls;
using FargowiltasSouls.Projectiles.Minions;
using FargowiltasSouls.Buffs.Souls;
using FargowiltasSouls.Buffs.Masomode;
using Terraria.DataStructures;

namespace FargowiltasSouls.Projectiles
{
    public class FargoSoulsGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        //        private bool townNPCProj;
        public int counter;
        public bool Rainbow = false;
        public int GrazeCD;

        //enchants
        public bool CanSplit = true;
        private int numSplits = 1;
        public int stormTimer;
        public float TungstenScale = 1;
        public bool tikiMinion;
        private int tikiTimer = 300;
        public int shroomiteMushroomCD ;
        private int spookyCD;
        public bool FrostFreeze;
        //        public bool SuperBee;
        public bool ChilledProj;
        public int ChilledTimer;
        public bool SilverMinion;

        public Func<Projectile, bool> GrazeCheck = projectile =>
            projectile.Distance(Main.LocalPlayer.Center) < Math.Min(projectile.width, projectile.height) / 2 + Player.defaultHeight + Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().GrazeRadius
            && (projectile.ModProjectile == null ? true : projectile.ModProjectile.CanDamage() != false) 
            && Collision.CanHit(projectile.Center, 0, 0, Main.LocalPlayer.Center, 0, 0);

        private bool firstTick = true;
        private bool squeakyToy = false;
        public const int TimeFreezeMoveDuration = 10;
        public int TimeFrozen = 0;
        public bool TimeFreezeImmune;
        public bool TimeFreezeCheck;
        public int DeletionImmuneRank;

        //        public int ModProjID;

        public bool canHurt = true;

        public bool noInteractionWithNPCImmunityFrames;
        private int tempIframe;

        public override void SetDefaults(Projectile projectile)
        {
            switch (projectile.type)
            {
                case ProjectileID.FinalFractal:
                    DeletionImmuneRank = 2;
                    break;

                case ProjectileID.StardustGuardian:
                case ProjectileID.StardustGuardianExplosion:
                    TimeFreezeImmune = true;
                    break;

                case ProjectileID.Sharknado:
                case ProjectileID.Cthulunado:
                    DeletionImmuneRank = 1;
                    break;

                case ProjectileID.MoonlordTurretLaser:
                    projectile.DamageType = DamageClass.Summon;
                    DeletionImmuneRank = 1;
                    break;

                case ProjectileID.LastPrism:
                case ProjectileID.LastPrismLaser:
                case ProjectileID.ChargedBlasterCannon:
                case ProjectileID.ChargedBlasterLaser:
                    DeletionImmuneRank = 1;
                    TimeFreezeImmune = true;
                    break;

                case ProjectileID.SandnadoFriendly:
                    DeletionImmuneRank = 1;
                    break;

                case ProjectileID.PhantasmalDeathray:
                case ProjectileID.FairyQueenSunDance:
                case ProjectileID.SaucerDeathray:
                case ProjectileID.SandnadoHostile:
                case ProjectileID.SandnadoHostileMark:
                case ProjectileID.StardustSoldierLaser:
                    DeletionImmuneRank = 1;
                    break;

                case ProjectileID.DD2BetsyFlameBreath:
                    DeletionImmuneRank = 1;
                    break;

                case ProjectileID.StardustCellMinionShot:
                case ProjectileID.MiniSharkron:
                case ProjectileID.UFOLaser:
                    ProjectileID.Sets.MinionShot[projectile.type] = true;
                    break;

                case ProjectileID.SpiderEgg:
                case ProjectileID.BabySpider:
                case ProjectileID.FrostBlastFriendly:
                case ProjectileID.RainbowCrystalExplosion:
                case ProjectileID.DD2FlameBurstTowerT1Shot:
                case ProjectileID.DD2FlameBurstTowerT2Shot:
                case ProjectileID.DD2FlameBurstTowerT3Shot:
                case ProjectileID.DD2BallistraProj:
                case ProjectileID.DD2ExplosiveTrapT1Explosion:
                case ProjectileID.DD2ExplosiveTrapT2Explosion:
                case ProjectileID.DD2ExplosiveTrapT3Explosion:
                case ProjectileID.MonkStaffT1Explosion:
                case ProjectileID.DD2LightningAuraT1:
                case ProjectileID.DD2LightningAuraT2:
                case ProjectileID.DD2LightningAuraT3:
                    projectile.DamageType = DamageClass.Summon;
                    break;

                default:
                    break;
            }

            //            Fargowiltas.ModProjDict.TryGetValue(projectile.type, out ModProjID);
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            switch (projectile.type)
            {
                case ProjectileID.DesertDjinnCurse:
                    {
                        if (projectile.damage > 0 && source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.active && npc.type == ModContent.NPCType<NPCs.Champions.ShadowChampion>())
                            projectile.damage = FargoSoulsUtil.ScaledProjectileDamage(npc.damage);
                    }
                    break;

                case ProjectileID.SandnadoHostile:
                    {
                        if (projectile.damage > 0 && source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.active)
                        {
                            if (npc.type == ModContent.NPCType<NPCs.DeviBoss.DeviBoss>())
                            {
                                projectile.damage = FargoSoulsUtil.ScaledProjectileDamage(npc.damage);
                                if (npc.ai[0] == 5)
                                    projectile.timeLeft = Math.Min(projectile.timeLeft, 360 + 90 - (int)npc.ai[1]);
                                else
                                    projectile.timeLeft = 90;
                            }
                            else if (npc.type == ModContent.NPCType<NPCs.Champions.ShadowChampion>())
                            {
                                projectile.damage = FargoSoulsUtil.ScaledProjectileDamage(npc.damage);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        public static int[] noSplit => new int[] {
            ProjectileID.CrystalShard,
            ProjectileID.SandnadoFriendly,
            ProjectileID.LastPrism,
            ProjectileID.LastPrismLaser,
            ProjectileID.FlowerPetal,
            ProjectileID.BabySpider,
            ProjectileID.CrystalLeafShot,
            ProjectileID.Phantasm,
            ProjectileID.VortexBeater,
            ProjectileID.ChargedBlasterCannon,
            ProjectileID.MedusaHead,
            ProjectileID.WireKite,
            ProjectileID.DD2PhoenixBow,
            ProjectileID.LaserMachinegun,
            ProjectileID.PiercingStarlight
        };

        public override bool PreAI(Projectile projectile)
        {
            bool retVal = true;
            Player player = Main.player[Main.myPlayer];
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();
            counter++;
             
            if (spookyCD > 0)
            {
                spookyCD--;
            }

            if (projectile.owner == Main.myPlayer)
            {
                if (firstTick)
                {
                    //townNPCProj = projectile.friendly && !projectile.hostile
                    //    && !projectile.melee && !projectile.ranged && !projectile.magic && !projectile.minion && !projectile.thrown
                    //    && !projectile.sentry && !ProjectileID.Sets.MinionShot[projectile.type] && !ProjectileID.Sets.SentryShot[projectile.type];
                    /*for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];

                        if (npc.active && npc.townNPC && projectile.Hitbox.Intersects(npc.Hitbox))
                        {
                            townNPCProj = true;
                        }
                    }*/

                    if (modPlayer.SilverEnchantActive && FargoSoulsUtil.IsSummonDamage(projectile, true, false) && player.GetToggleValue("SilverSpeed"))
                    {
                        SilverMinion = true;
                        projectile.extraUpdates++;
                    }

                    if (modPlayer.TungstenEnchantActive && player.GetToggleValue("TungstenProj"))
                    {
                        TungstenEnchant.TungstenIncreaseProjSize(projectile, modPlayer);
                    }

                    if (modPlayer.TikiEnchantActive)
                    {
                        if (FargoSoulsUtil.IsSummonDamage(projectile) && (projectile.sentry ? modPlayer.TikiSentry : modPlayer.TikiMinion))
                        {
                            tikiMinion = true;

                            if (projectile.type != ModContent.ProjectileType<EaterBody>() && projectile.type != ProjectileID.StardustDragon2 && projectile.type != ProjectileID.StardustDragon3)
                            {
                                tikiMinion = true;
                                tikiTimer = 300;

                                if (modPlayer.SpiritForce || modPlayer.WizardEnchantActive)
                                {
                                    tikiTimer = 480;
                                }
                            }
                        }
                    }

                    //if (modPlayer.StardustEnchant && projectile.type == ProjectileID.StardustGuardianExplosion)
                    //{
                    //    projectile.damage *= 5;
                    //}

                    if (modPlayer.AdamantiteEnchantActive && player.GetToggleValue("Adamantite") /*&& modPlayer.AdamantiteCD == 0*/ && CanSplit
                        && projectile.friendly && !projectile.hostile && !projectile.npcProj && projectile.damage > 0 
                        && Array.IndexOf(noSplit, projectile.type) <= -1
                        && !projectile.minion && !projectile.sentry && !ProjectileID.Sets.IsAWhip[projectile.type]
                        && projectile.minionSlots == 0 && projectile.aiStyle != 19 && projectile.aiStyle != 99
                        && !(projectile.type == ProjectileID.DD2BetsyArrow && projectile.ai[1] == -1))
                    {
                        modPlayer.AdamantiteCanSplit = !modPlayer.AdamantiteCanSplit;
                        if (modPlayer.AdamantiteCanSplit)
                            AdamantiteEnchant.AdamantiteSplit(projectile);
                        else //cut damage anyway
                            projectile.damage = (int)(projectile.damage * AdamantiteEnchant.ProjectileDamageRatio);
                    }

                    if (projectile.bobber && CanSplit)
                    {
                        /*if (modPlayer.FishSoul1)
                        {
                            SplitProj(projectile, 5);
                        }*/
                        if (player.whoAmI == Main.myPlayer && modPlayer.FishSoul2)
                        {
                            SplitProj(projectile, 11, MathHelper.Pi / 3, 1);
                        }
                    }

                    /*if (modPlayer.BeeEnchant && (projectile.type == ProjectileID.GiantBee || projectile.type == ProjectileID.Bee || projectile.type == ProjectileID.Wasp))
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = 5;
                        projectile.penetrate *= 2;
                        projectile.timeLeft *= 2;
                        projectile.scale *= 2.5f;
                        //projectile.damage = (int)(projectile.damage * 1.5);
                        SuperBee = true;
                    }*/
                }

                //reset tungsten size
                if (TungstenScale != 1 && (!modPlayer.TungstenEnchantActive || !player.GetToggleValue("TungstenProj")))
                {
                    projectile.position = projectile.Center;
                    projectile.scale /= TungstenScale;
                    projectile.width = (int)(projectile.width / TungstenScale);
                    projectile.height = (int)(projectile.height / TungstenScale);
                    projectile.Center = projectile.position;

                    TungstenScale = 1;
                }

                switch (projectile.type)
                {
                    case ProjectileID.RedCounterweight:
                    case ProjectileID.BlackCounterweight:
                    case ProjectileID.BlueCounterweight:
                    case ProjectileID.GreenCounterweight:
                    case ProjectileID.PurpleCounterweight:
                    case ProjectileID.YellowCounterweight:
                        {
                            if (player.HeldItem.type == ModContent.ItemType<Blender>())
                            {
                                projectile.localAI[0]++;
                                if (projectile.localAI[0] > 60)
                                {
                                    projectile.Kill();
                                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCKilled, (int)projectile.Center.X, (int)projectile.Center.Y, 11, 0.5f);
                                    int proj2 = ModContent.ProjectileType<BlenderProj3>();
                                    Projectile.NewProjectile(projectile.GetSource_FromThis(), new Vector2(projectile.Center.X, projectile.Center.Y), projectile.DirectionFrom(player.Center) * 8, proj2, projectile.damage, projectile.knockBack, projectile.owner);
                                }
                            }
                        }
                        break;
                }

                if (tikiMinion)
                {
                    projectile.alpha = 120;

                    //dust
                    if (Main.rand.Next(4) < 2)
                    {
                        int dust = Dust.NewDust(new Vector2(projectile.position.X - 2f, projectile.position.Y - 2f), projectile.width + 4, projectile.height + 4, 44, projectile.velocity.X * 0.4f, projectile.velocity.Y * 0.4f, 100, Color.LimeGreen, .8f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 1.8f;
                        Dust expr_1CCF_cp_0 = Main.dust[dust];
                        expr_1CCF_cp_0.velocity.Y = expr_1CCF_cp_0.velocity.Y - 0.5f;
                        if (Main.rand.NextBool(4))
                        {
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].scale *= 0.5f;
                        }
                    }

                    tikiTimer--;

                    if (tikiTimer <= 0)
                    {
                        for (int num468 = 0; num468 < 20; num468++)
                        {
                            int num469 = Dust.NewDust(new Vector2(projectile.Center.X, projectile.Center.Y), projectile.width, projectile.height, 44, -projectile.velocity.X * 0.2f,
                                -projectile.velocity.Y * 0.2f, 100, Color.LimeGreen, 1f);
                            Main.dust[num469].noGravity = true;
                            Main.dust[num469].velocity *= 2f;
                            num469 = Dust.NewDust(new Vector2(projectile.Center.X, projectile.Center.Y), projectile.width, projectile.height, 44, -projectile.velocity.X * 0.2f,
                                -projectile.velocity.Y * 0.2f, 100, Color.LimeGreen, .5f);
                            Main.dust[num469].velocity *= 2f;
                        }

                        //stardust dragon fix
                        if (projectile.type == ProjectileID.StardustDragon2)
                        {
                            int tailIndex = -1;
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile p = Main.projectile[i];

                                if (p.active && p.type == ProjectileID.StardustDragon4)
                                {
                                    tailIndex = i;
                                    break;
                                }
                            }

                            Projectile prev = Main.projectile[tailIndex];
                            List<int> list = new List<int>();
                            list.Add(prev.whoAmI);

                            while (prev.type != ProjectileID.StardustDragon1)
                            {
                                list.Add((int)prev.ai[0]);
                                prev = Main.projectile[(int)prev.ai[0]];
                            }

                            int listIndex = list.IndexOf(projectile.whoAmI);
                            Main.projectile[list[listIndex - 2]].ai[0] = list[listIndex + 1];
                        }

                        projectile.Kill();
                    }
                }

                //if (SuperBee && (modPlayer.LifeForce || modPlayer.WizardEnchant))
                //{
                //    projectile.position += projectile.velocity;
                //}

                ////prob change in 1.4
                //if (modPlayer.StardustEnchant && projectile.type == ProjectileID.StardustGuardian)
                //{
                //    projectile.localAI[0] = 0f;
                //}

                //hook ai
                if (modPlayer.MahoganyEnchantActive && player.GetToggleValue("Mahogany", false) && projectile.aiStyle == 7)
                {
                    RichMahoganyEnchant.MahoganyHookAI(projectile, player);
                }

                if (projectile.friendly && !projectile.hostile)
                {
                    if (modPlayer.Jammed && projectile.DamageType == DamageClass.Ranged && projectile.type != ProjectileID.ConfettiGun)
                    {
                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity, ProjectileID.ConfettiGun, 0, 0f, projectile.owner);
                        projectile.active = false;
                    }

                    if (modPlayer.Atrophied && projectile.DamageType == DamageClass.Throwing)
                    {
                        projectile.damage = 0;
                        projectile.position = new Vector2(Main.maxTilesX);
                        projectile.Kill();
                    }

                    if (modPlayer.ShroomEnchantActive && player.GetToggleValue("ShroomiteShroom") && projectile.damage > 0 /*&& !townNPCProj*/ && projectile.velocity.Length() > 1 && projectile.minionSlots == 0 && projectile.type != ModContent.ProjectileType<ShroomiteShroom>() && player.ownedProjectileCounts[ModContent.ProjectileType<ShroomiteShroom>()] < 50)
                    {
                        if (shroomiteMushroomCD >= 15)
                        {
                            shroomiteMushroomCD = 0;

                            if (player.stealth == 0 || modPlayer.NatureForce || modPlayer.WizardEnchantActive)
                            {
                                shroomiteMushroomCD = 10;
                            }

                            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity, ModContent.ProjectileType<ShroomiteShroom>(), projectile.damage / 2, projectile.knockBack / 2, projectile.owner);
                        }
                        shroomiteMushroomCD++;
                    }

                    if (modPlayer.SpookyEnchantActive && player.GetToggleValue("Spooky")
                        && projectile.minionSlots > 0 && spookyCD == 0)
                    {
                        float minDistance = 500f;
                        int npcIndex = -1;
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC target = Main.npc[i];

                            if (target.active && Vector2.Distance(projectile.Center, target.Center) < minDistance && Main.npc[i].CanBeChasedBy(projectile, false))
                            {
                                npcIndex = i;
                                minDistance = Vector2.Distance(projectile.Center, target.Center);
                            }
                        }

                        if (npcIndex != -1)
                        {
                            NPC target = Main.npc[npcIndex];

                            if (Collision.CanHit(projectile.position, projectile.width, projectile.height, target.position, target.width, target.height))
                            {
                                Vector2 velocity = Vector2.Normalize(target.Center - projectile.Center) * 20;

                                int p = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, velocity, ModContent.ProjectileType<SpookyScythe>(), projectile.damage, 2, projectile.owner);

                                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)projectile.position.X, (int)projectile.position.Y, 62, 0.5f);

                                spookyCD = 30 + Main.rand.Next(player.maxMinions * 5);

                                if (modPlayer.ShadowForce || modPlayer.WizardEnchantActive)
                                {
                                    spookyCD -= 10;
                                }
                            }
                        }

                    }
                }

                if (modPlayer.Asocial && FargoSoulsUtil.IsSummonDamage(projectile, true, false))
                {
                    projectile.Kill();
                    retVal = false;
                }
            }

            if (ChilledTimer > 0)
            {
                ChilledTimer--;

                if (retVal && ChilledTimer % 2 == 1)
                {
                    retVal = false;
                    projectile.timeLeft++;
                }

                if (ChilledTimer <= 0)
                    ChilledProj = false;
            }

            if (modPlayer.SnowEnchantActive && player.GetToggleValue("Snow") && projectile.hostile && !ChilledProj)
            {
                ChilledProj = true;
                projectile.timeLeft *= 2;
                projectile.netUpdate = true;
            }

            if (TimeFrozen > 0 && !firstTick && !TimeFreezeImmune)
            {
                if (counter % projectile.MaxUpdates == 0) //only decrement once per tick
                    TimeFrozen--;
                if (counter > TimeFreezeMoveDuration * projectile.MaxUpdates)
                {
                    projectile.position = projectile.oldPosition;

                    if (projectile.frameCounter > 0)
                        projectile.frameCounter--;

                    if (retVal)
                    {
                        retVal = false;
                        projectile.timeLeft++;
                    }
                }
            }

            ////masomode unicorn meme and pearlwood meme
            if (Rainbow)
            {
                projectile.tileCollide = false;

                if (counter >= 5)
                    projectile.velocity = Vector2.Zero;

                int deathTimer = 15;

                if (projectile.hostile)
                    deathTimer = 60;

                if (counter >= deathTimer)
                    projectile.Kill();
            }

            if (firstTick)
            {
                firstTick = false;
            }

            return retVal;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            switch (projectile.type)
            {
                case ProjectileID.RedCounterweight:
                case ProjectileID.BlackCounterweight:
                case ProjectileID.BlueCounterweight:
                case ProjectileID.GreenCounterweight:
                case ProjectileID.PurpleCounterweight:
                case ProjectileID.YellowCounterweight:
                    {
                        Player player = Main.player[projectile.owner];
                        if (player.HeldItem.type == ModContent.ItemType<Blender>())
                        {
                            Texture2D texture2D13 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/PlanteraTentacle", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                            Rectangle rectangle = new Rectangle(0, 0, texture2D13.Width, texture2D13.Height);
                            Vector2 origin2 = rectangle.Size() / 2f;

                            SpriteEffects spriteEffects = projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                            Vector2 toPlayer = projectile.Center - player.Center;
                            float drawRotation = toPlayer.ToRotation() + MathHelper.Pi;
                            if (projectile.spriteDirection < 0)
                                drawRotation += (float)Math.PI;
                            Main.EntitySpriteDraw(texture2D13, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), projectile.GetAlpha(lightColor),
                                drawRotation, origin2, projectile.scale * 0.8f, spriteEffects, 0);
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }
            return base.PreDraw(projectile, ref lightColor);
        }

        public static void SplitProj(Projectile projectile, int number, float maxSpread, float damageRatio)
        {
            if (ModContent.TryFind("Fargowiltas", "SpawnProj", out ModProjectile spawnProj) && projectile.type == spawnProj.Type)
            {
                return;
            }

            //if its odd, we just keep the original 
            if (number % 2 != 0)
            {
                number--;
            }

            Projectile split;

            double spread = maxSpread / number;

            for (int i = 0; i < number / 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int factor = (j == 0) ? 1 : -1;
                    split = FargoSoulsUtil.NewProjectileDirectSafe(projectile.GetSource_FromThis(), projectile.Center, projectile.velocity.RotatedBy(factor * spread * (i + 1)), projectile.type, (int)(projectile.damage * damageRatio), projectile.knockBack, projectile.owner, projectile.ai[0], projectile.ai[1]);
                    if (split != null)
                    {
                        split.localAI[0] = projectile.localAI[0];
                        split.localAI[1] = projectile.localAI[1];

                        split.friendly = projectile.friendly;
                        split.hostile = projectile.hostile;
                        split.timeLeft = projectile.timeLeft;
                        split.DamageType = projectile.DamageType;

                        split.GetGlobalProjectile<FargoSoulsGlobalProjectile>().numSplits = projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().numSplits;
                        split.GetGlobalProjectile<FargoSoulsGlobalProjectile>().CanSplit = false;
                        split.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TungstenScale = projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TungstenScale;
                    }
                }
            }
        }

        private void KillPet(Projectile projectile, Player player, int buff, bool toggle, bool minion = false)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (player.FindBuffIndex(buff) == -1)
            {
                if (player.dead || !toggle || (minion ? !modPlayer.StardustEnchantActive : !modPlayer.VoidSoul) || (!modPlayer.PetsActive && !minion))
                {
                    projectile.Kill();
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            switch (projectile.type)
            {
                #region pets

                case ProjectileID.BabyHornet:
                    KillPet(projectile, player, BuffID.BabyHornet, player.GetToggleValue("PetHornet"));
                    break;

                case ProjectileID.Sapling:
                    KillPet(projectile, player, BuffID.PetSapling, player.GetToggleValue("PetSeed"));
                    break;

                case ProjectileID.BabyFaceMonster:
                    KillPet(projectile, player, BuffID.BabyFaceMonster, player.GetToggleValue("PetFaceMonster"));
                    break;

                case ProjectileID.CrimsonHeart:
                    KillPet(projectile, player, BuffID.CrimsonHeart, player.GetToggleValue("PetHeart"));
                    break;

                case ProjectileID.MagicLantern:
                    KillPet(projectile, player, BuffID.MagicLantern, player.GetToggleValue("PetLantern"));
                    break;

                case ProjectileID.MiniMinotaur:
                    KillPet(projectile, player, BuffID.MiniMinotaur, player.GetToggleValue("PetMinitaur"));
                    break;

                case ProjectileID.BlackCat:
                    KillPet(projectile, player, BuffID.BlackCat, player.GetToggleValue("PetBlackCat"));
                    break;

                case ProjectileID.Wisp:
                    KillPet(projectile, player, BuffID.Wisp, player.GetToggleValue("PetWisp"));
                    break;

                case ProjectileID.CursedSapling:
                    KillPet(projectile, player, BuffID.CursedSapling, player.GetToggleValue("PetCursedSapling"));
                    break;

                case ProjectileID.EyeSpring:
                    KillPet(projectile, player, BuffID.EyeballSpring, player.GetToggleValue("PetEyeSpring"));
                    break;

                case ProjectileID.Turtle:
                    KillPet(projectile, player, BuffID.PetTurtle, player.GetToggleValue("PetTurtle"));
                    break;

                case ProjectileID.PetLizard:
                    KillPet(projectile, player, BuffID.PetLizard, player.GetToggleValue("PetLizard"));
                    break;

                case ProjectileID.Truffle:
                    KillPet(projectile, player, BuffID.BabyTruffle, player.GetToggleValue("PetShroom"));
                    break;

                case ProjectileID.Spider:
                    KillPet(projectile, player, BuffID.PetSpider, player.GetToggleValue("PetSpider"));
                    break;

                case ProjectileID.Squashling:
                    KillPet(projectile, player, BuffID.Squashling, player.GetToggleValue("PetSquash"));
                    break;

                case ProjectileID.BlueFairy:
                    KillPet(projectile, player, BuffID.FairyBlue, player.GetToggleValue("PetNavi"));
                    break;

                case ProjectileID.StardustGuardian:
                    KillPet(projectile, player, BuffID.StardustGuardianMinion, player.GetToggleValue("Stardust"), true);
                    if (modPlayer.FreezeTime && modPlayer.freezeLength > 60) //throw knives in stopped time
                    {
                        if (projectile.owner == Main.myPlayer && counter % 20 == 0)
                        {
                            int target = -1;

                            NPC minionAttackTargetNpc = projectile.OwnerMinionAttackTargetNPC;
                            if (minionAttackTargetNpc != null && minionAttackTargetNpc.CanBeChasedBy())
                            {
                                target = minionAttackTargetNpc.whoAmI;
                            }
                            else
                            {
                                const float homingMaximumRangeInPixels = 1000;
                                for (int i = 0; i < Main.maxNPCs; i++)
                                {
                                    NPC n = Main.npc[i];
                                    if (n.CanBeChasedBy(projectile))
                                    {
                                        float distance = projectile.Distance(n.Center);
                                        if (distance <= homingMaximumRangeInPixels &&
                                            (target == -1 || //there is no selected target
                                            projectile.Distance(Main.npc[target].Center) > distance)) //or we are closer to this target than the already selected target
                                        {
                                            target = i;
                                        }
                                    }
                                }
                            }

                            if (target != -1)
                            {
                                const int totalUpdates = 2 + 1;
                                const int travelTime = TimeFreezeMoveDuration * totalUpdates;

                                Vector2 spawnPos = projectile.Center + 16f * projectile.DirectionTo(Main.npc[target].Center);

                                //adjust speed so it always lands just short of touching the enemy
                                Vector2 vel = Main.npc[target].Center - spawnPos;
                                float length = (vel.Length() - 0.6f * Math.Max(Main.npc[target].width, Main.npc[target].height)) / travelTime;
                                if (length < 0.1f)
                                    length = 0.1f;

                                float offset = 1f - (modPlayer.freezeLength - 60f) / 540f; //change how far they stop as time decreases
                                if (offset < 0.1f)
                                    offset = 0.1f;
                                if (offset > 1f)
                                    offset = 1f;
                                length *= offset;

                                const int max = 3;
                                int damage = 100; //at time of writing, raw hellzone does 190 damage, 7.5 times per second, 1425 dps
                                if (modPlayer.CosmoForce)
                                    damage = 150;
                                if (modPlayer.TerrariaSoul)
                                    damage = 300;
                                damage = (int)(damage * player.ActualClassDamage(DamageClass.Summon));
                                float rotation = MathHelper.ToRadians(60) * Main.rand.NextFloat(0.2f, 1f);
                                float rotationOffset = MathHelper.ToRadians(5) * Main.rand.NextFloat(-1f, 1f);
                                for (int i = -max; i <= max; i++)
                                {
                                    Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, length * Vector2.Normalize(vel).RotatedBy(rotation / max * i + rotationOffset),
                                        ModContent.ProjectileType<StardustKnife>(), damage, 4f, Main.myPlayer);
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.TikiSpirit:
                    KillPet(projectile, player, BuffID.TikiSpirit, player.GetToggleValue("PetTiki"));
                    break;

                case ProjectileID.Penguin:
                    KillPet(projectile, player, BuffID.BabyPenguin, player.GetToggleValue("PetPenguin"));
                    break;

                case ProjectileID.BabySnowman:
                    KillPet(projectile, player, BuffID.BabySnowman, player.GetToggleValue("PetSnowman"));
                    break;

                case ProjectileID.BabyGrinch:
                    KillPet(projectile, player, BuffID.BabyGrinch, player.GetToggleValue("PetGrinch"));
                    break;

                case ProjectileID.DD2PetGato:
                    KillPet(projectile, player, BuffID.PetDD2Gato, player.GetToggleValue("PetGato"));
                    break;

                case ProjectileID.Parrot:
                    KillPet(projectile, player, BuffID.PetParrot, player.GetToggleValue("PetParrot"));
                    break;

                case ProjectileID.Puppy:
                    KillPet(projectile, player, BuffID.Puppy, player.GetToggleValue("PetPup"));
                    break;

                case ProjectileID.CompanionCube:
                    KillPet(projectile, player, BuffID.CompanionCube, player.GetToggleValue("PetCompanionCube"));
                    break;

                case ProjectileID.DD2PetDragon:
                    KillPet(projectile, player, BuffID.PetDD2Dragon, player.GetToggleValue("PetDragon"));
                    break;

                case ProjectileID.BabySkeletronHead:
                    KillPet(projectile, player, BuffID.BabySkeletronHead, player.GetToggleValue("PetDG"));
                    break;

                case ProjectileID.BabyDino:
                    KillPet(projectile, player, BuffID.BabyDinosaur, player.GetToggleValue("PetDino"));
                    break;

                case ProjectileID.BabyEater:
                    KillPet(projectile, player, BuffID.BabyEater, player.GetToggleValue("PetEater"));
                    break;

                case ProjectileID.ShadowOrb:
                    KillPet(projectile, player, BuffID.ShadowOrb, player.GetToggleValue("PetOrb"));
                    break;

                case ProjectileID.SuspiciousTentacle:
                    KillPet(projectile, player, BuffID.SuspiciousTentacle, player.GetToggleValue("PetSuspEye"));
                    break;

                case ProjectileID.DD2PetGhost:
                    KillPet(projectile, player, BuffID.PetDD2Ghost, player.GetToggleValue("PetFlicker"));
                    break;

                case ProjectileID.ZephyrFish:
                    KillPet(projectile, player, BuffID.ZephyrFish, player.GetToggleValue("PetZephyr"));
                    break;

                #endregion

                case ProjectileID.Flamelash:
                case ProjectileID.MagicMissile:
                case ProjectileID.RainbowRodBullet:
                    if (Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().TribalCharm)
                    {
                        if (projectile.timeLeft > 300)
                            projectile.timeLeft = 300;
                    }
                    break;

                case ProjectileID.RuneBlast:
                    if (projectile.ai[0] == 1f)
                    {
                        if (projectile.localAI[0] == 0f)
                        {
                            projectile.localAI[0] = projectile.Center.X;
                            projectile.localAI[1] = projectile.Center.Y;
                        }
                        Vector2 distance = projectile.Center - new Vector2(projectile.localAI[0], projectile.localAI[1]);
                        if (distance != Vector2.Zero && distance.Length() >= 300f)
                        {
                            projectile.velocity = distance.RotatedBy(Math.PI / 2);
                            projectile.velocity.Normalize();
                            projectile.velocity *= 8f;
                        }
                    }
                    break;

                default:
                    break;
            }

            if (stormTimer > 0)
            {
                stormTimer--;

                int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.GoldFlame, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 1.5f);
                Main.dust[dustId].noGravity = true;
            }

            if (ChilledProj)
            {
                int dustId = Dust.NewDust(projectile.position, projectile.width, projectile.height, 76, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 1f);
                Main.dust[dustId].noGravity = true;

                projectile.position -= projectile.velocity * 0.5f;
            }

            if (SilverMinion && projectile.owner == Main.myPlayer)
            {
                /*if (counter == 60) //i hate netcode, proj array desyncs between clients
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        var netMessage = mod.GetPacket();
                        netMessage.Write((byte)19);
                        netMessage.Write(projectile.whoAmI);
                        netMessage.Write(projectile.type);
                        netMessage.Write(projectile.extraUpdates);
                        netMessage.Send();
                    }
                }*/

                if (!(modPlayer.SilverEnchantActive && player.GetToggleValue("SilverSpeed")))
                    projectile.Kill();
            }

            if (projectile.bobber && modPlayer.FishSoul1)
            {
                //ai0 = in water, localai1 = counter up to catching an item
                if (projectile.wet && projectile.ai[0] == 0 && projectile.localAI[1] < 655 /*&& Main.player[projectile.owner].FishingLevel() != -1*/) //fishron check
                    projectile.localAI[1] = 655; //quick catch. not 660 and up, may break things
            }

            if (modPlayer.HuntressEnchantActive && projectile.arrow)
            {
                //stuck in enemy
                if (projectile.localAI[0] == 1)
                {
                    projectile.aiStyle = -1;

                    projectile.ignoreWater = true;
                    projectile.tileCollide = false;

                    bool kill = false;
                    int npcIndex = (int)projectile.localAI[1];

                    if (npcIndex < 0 || npcIndex >= 200 || projectile.timeLeft <= 5)
                    {
                        kill = true;
                    }
                    else if (Main.npc[npcIndex].active && !Main.npc[npcIndex].dontTakeDamage)
                    {
                        projectile.Center = Main.npc[npcIndex].Center - projectile.velocity * 2f;
                        projectile.gfxOffY = Main.npc[npcIndex].gfxOffY;
                    }
                    else
                    {
                        kill = true;
                    }

                    if (kill)
                    {
                        //remove stack of bleed

                        projectile.Kill();
                    }
                }
            }
        }

        public override void PostAI(Projectile projectile)
        {
            FargoSoulsPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>();

            if (!TimeFreezeCheck)
            {
                TimeFreezeCheck = true;
                if (projectile.whoAmI == Main.player[projectile.owner].heldProj)
                    TimeFreezeImmune = true;
            }

            if (projectile.whoAmI == Main.player[projectile.owner].heldProj)
            {
                DeletionImmuneRank = 2;

                if (!FargoSoulsUtil.IsSummonDamage(projectile))
                    Main.player[projectile.owner].GetModPlayer<EModePlayer>().MasomodeWeaponUseTimer = 30;

                modPlayer.TryAdditionalAttacks(projectile.damage, projectile.DamageType);
            }

            if (projectile.hostile && projectile.damage > 0 && canHurt && projectile.GetGlobalProjectile<EModeGlobalProjectile>().EModeCanHurt && Main.LocalPlayer.active && !Main.LocalPlayer.dead) //graze
            {
                FargoSoulsPlayer fargoPlayer = Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>();
                if (fargoPlayer.Graze && --GrazeCD < 0 && !Main.LocalPlayer.immune && Main.LocalPlayer.hurtCooldowns[0] <= 0 && Main.LocalPlayer.hurtCooldowns[1] <= 0)
                {
                    if (GrazeCheck(projectile))
                    {
                        double grazeCap = 0.25;
                        if (fargoPlayer.MutantEyeItem != null)
                            grazeCap += 0.25;

                        double grazeGain = 0.0125;
                        if (fargoPlayer.AbomWandItem != null)
                            grazeGain *= 2;

                        GrazeCD = 30 * projectile.MaxUpdates;
                        fargoPlayer.GrazeBonus += grazeGain;
                        if (fargoPlayer.GrazeBonus > grazeCap)
                        {
                            fargoPlayer.GrazeBonus = grazeCap;
                            if (fargoPlayer.StyxSet)
                                fargoPlayer.StyxMeter += FargoSoulsUtil.HighestDamageTypeScaling(Main.LocalPlayer, projectile.damage * 4) * 5; //as if gaining the projectile's damage, times SOU crit
                        }
                        fargoPlayer.GrazeCounter = -1; //reset counter whenever successful graze

                        if (!Main.dedServ)
                        {
                            Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(FargowiltasSouls.Instance, "Sounds/Graze").WithVolume(0.5f), Main.LocalPlayer.Center);
                        }

                        Vector2 baseVel = Vector2.UnitX.RotatedByRandom(2 * Math.PI);
                        const int max = 64; //make some indicator dusts
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 vector6 = baseVel * 3f;
                            vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Main.LocalPlayer.Center;
                            Vector2 vector7 = vector6 - Main.LocalPlayer.Center;
                            //changes color when bonus is maxed
                            int d = Dust.NewDust(vector6 + vector7, 0, 0, fargoPlayer.GrazeBonus >= grazeCap ? 86 : 228, 0f, 0f, 0, default(Color));
                            Main.dust[d].scale = fargoPlayer.GrazeBonus >= grazeCap ? 1f : 0.75f;
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity = vector7;
                        }
                    }
                    else
                    {
                        GrazeCD = 6; //don't check per tick ech
                    }
                }
            }
        }


        public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (projectile.type == ProjectileID.SmokeBomb)
            {
                fallThrough = false;
            }

            if (TungstenScale != 1)
            {
                width = (int)(width / TungstenScale);
                height = (int)(height / TungstenScale);
            }

            return base.TileCollideStyle(projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (!canHurt)
                return false;
            if (TimeFrozen > 0 && counter > TimeFreezeMoveDuration * projectile.MaxUpdates) 
                return false;
            if (target.GetModPlayer<FargoSoulsPlayer>().PrecisionSealHurtbox && !projectile.Colliding(projectile.Hitbox, target.GetModPlayer<FargoSoulsPlayer>().GetPrecisionHurtbox()))
                return false;
            return true;
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (!canHurt)
                return false;
            if (TimeFrozen > 0 && counter > TimeFreezeMoveDuration * projectile.MaxUpdates) 
                return false;
            return null;
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (stormTimer > 0)
                damage = (int)(damage * (Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().SpiritForce ? 1.6 : 1.3));

            if (noInteractionWithNPCImmunityFrames)
                tempIframe = target.immune[projectile.owner];

            if (projectile.type >= ProjectileID.StardustDragon1 && projectile.type <= ProjectileID.StardustDragon4
                && Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().TikiMinion
                && Main.player[projectile.owner].ownedProjectileCounts[ProjectileID.StardustDragon2] > Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().actualMinions)
            {
                int newDamage = (int)(projectile.damage * (1.69 + 0.46 * Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>().actualMinions));
                if (damage > newDamage)
                    damage = newDamage;
            }

            if (SilverMinion)
            {
                if (projectile.maxPenetrate == 1 || projectile.usesLocalNPCImmunity || projectile.type == ProjectileID.StardustCellMinionShot)
                {
                    //reduce final damage but ignore some defense to compensate
                    damage /= 2;

                    FargoSoulsPlayer modPlayer = Main.player[projectile.owner].GetModPlayer<FargoSoulsPlayer>();
                    int armorPen = 20;
                    if (modPlayer.SpiritForce)
                        armorPen *= 2;
                    if (modPlayer.TerrariaSoul)
                        armorPen *= 3;

                    damage += Math.Min(target.defense / 4, armorPen / 2);
                }
            }

            if (projectile.type == ProjectileID.SharpTears && !projectile.usesLocalNPCImmunity && projectile.usesIDStaticNPCImmunity && noInteractionWithNPCImmunityFrames)
            {
                crit = true;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (noInteractionWithNPCImmunityFrames)
                target.immune[projectile.owner] = tempIframe;

            if (projectile.type == ProjectileID.SharpTears && !projectile.usesLocalNPCImmunity && projectile.usesIDStaticNPCImmunity && noInteractionWithNPCImmunityFrames)
            {
                target.AddBuff(ModContent.BuffType<Anticoagulation>(), 360);

                if (FargoSoulsUtil.NPCExists(target.realLife) != null)
                {
                    foreach (NPC n in Main.npc.Where(n => n.active && (n.realLife == target.realLife || n.whoAmI == target.realLife) && n.whoAmI != target.whoAmI))
                    {
                        Projectile.perIDStaticNPCImmunity[projectile.type][n.whoAmI] = Main.GameUpdateCount + (uint)projectile.idStaticNPCHitCooldown;
                    }
                }
            }

            if (FrostFreeze)
            {
                FargoSoulsGlobalNPC globalNPC = target.GetGlobalNPC<FargoSoulsGlobalNPC>();

                int debuff = ModContent.BuffType<Frozen>();
                int duration = target.HasBuff(debuff) ? 5 : 15;

                NPC head = FargoSoulsUtil.NPCExists(target.realLife);
                if (head != null)
                {
                    head.AddBuff(debuff, duration);

                    foreach (NPC n in Main.npc.Where(n => n.active && n.realLife == head.whoAmI && n.whoAmI != head.whoAmI))
                        n.AddBuff(debuff, duration);
                }
                else
                {
                    target.AddBuff(debuff, duration);
                }
            }

            Player player = Main.player[projectile.owner];
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (projectile.owner == player.whoAmI && modPlayer.HuntressEnchantActive && projectile.arrow)
            {
                //start sticking in enemy
                projectile.localAI[0] = 1;
                projectile.localAI[1] = (float)target.whoAmI;
                projectile.velocity = (Main.npc[target.whoAmI].Center - projectile.Center) * 0.5f; //distance it sticks out
                projectile.aiStyle = -1;
                projectile.damage = 0;
                projectile.timeLeft = 305;
                projectile.netUpdate = true;

                //add stack of bleed
            }
        }

        public override void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
        {
            if (squeakyToy)
            {
                damage = 1;
                target.GetModPlayer<FargoSoulsPlayer>().Squeak(target.Center);
            }
        }

        public override void Kill(Projectile projectile, int timeLeft)
        {
            Player player = Main.player[projectile.owner];
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (!projectile.npcProj && CanSplit && projectile.friendly && projectile.damage > 0 && !projectile.minion && projectile.aiStyle != 19)
            {
                if (modPlayer.CobaltEnchantActive)
                {
                    if (player.GetToggleValue("Cobalt") && player.whoAmI == Main.myPlayer && modPlayer.CobaltCD == 0 && Main.rand.NextBool(4))
                    {
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)player.position.X, (int)player.position.Y, 27);

                        int damage = (int)(25 * player.GetDamage(DamageClass.Ranged).Additive);

                        if (modPlayer.TerrariaSoul)
                        {
                            damage *= 5;
                            modPlayer.CobaltCD = 10;
                        }
                        else if (modPlayer.EarthForce || modPlayer.WizardEnchantActive)
                        {
                            damage *= 2;
                            modPlayer.CobaltCD = 20;
                        }
                        else
                        {
                            modPlayer.CobaltCD = 30;
                        }

                        for (int i = 0; i < 5; i++)
                        {
                            float velX = -projectile.velocity.X * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                            float velY = -projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                            int p = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + velX, projectile.position.Y + velY, velX, velY, ProjectileID.CrystalShard, damage, 0f, projectile.owner);
                            if (p != Main.maxProjectiles)
                                Main.projectile[p].GetGlobalProjectile<FargoSoulsGlobalProjectile>().CanSplit = false;
                        }
                    }
                }
                else if (modPlayer.AncientCobaltEnchantActive && !modPlayer.CobaltEnchantActive && player.GetToggleValue("AncientCobalt") && player.whoAmI == Main.myPlayer && modPlayer.CobaltCD == 0 && Main.rand.NextBool(5))
                {
                    Projectile[] projs = FargoSoulsUtil.XWay(3, projectile.GetSource_FromThis(), projectile.Center, ProjectileID.HornetStinger, 5f, projectile.damage / 2, 0);

                    for (int i = 0; i < projs.Length; i++)
                    {
                        projs[i].penetrate = 3;
                        projs[i].timeLeft /= 2;
                    }

                    modPlayer.CobaltCD = 60;
                }
            }
        }

        //        public override void UseGrapple(Player player, ref int type)
        //        {
        //            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

        //            if (modPlayer.JungleEnchant)
        //            {
        //                modPlayer.CanJungleJump = true;
        //            }
        //        }

        public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.MahoganyEnchantActive && player.GetToggleValue("Mahogany", false))
            {
                float multiplier = 1.5f;

                if (modPlayer.WoodForce || modPlayer.WizardEnchantActive)
                {
                    multiplier = 2.5f;
                }

                speed *= multiplier;
            }
        }

        public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.MahoganyEnchantActive && player.GetToggleValue("Mahogany", false))
            {
                float multiplier = 3f;

                if (modPlayer.WoodForce || modPlayer.WizardEnchantActive)
                {
                    multiplier = 2.5f;
                }

                speed *= multiplier;
            }
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (projectile.type == ProjectileID.RuneBlast)
            {
                Texture2D texture2D13 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/RuneBlast", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                int num156 = texture2D13.Height / Main.projFrames[projectile.type]; //ypos of lower right corner of sprite to draw
                int y3 = num156 * projectile.frame; //ypos of upper left corner of sprite to draw
                Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
                Vector2 origin2 = rectangle.Size() / 2f;
                SpriteEffects effects = SpriteEffects.None;
                Main.EntitySpriteDraw(texture2D13, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), new Color(255, 255, 255), projectile.rotation, origin2, projectile.scale, effects, 0);
                Main.EntitySpriteDraw(texture2D13, projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), new Color(255, 255, 255, 0), projectile.rotation, origin2, projectile.scale, effects, 0);
            }
        }
    }
}
