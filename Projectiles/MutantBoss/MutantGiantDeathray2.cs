﻿using FargowiltasSouls.Buffs.Boss;
using FargowiltasSouls.Buffs.Masomode;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Buffs.Souls;
using System.IO;

namespace FargowiltasSouls.Projectiles.MutantBoss
{
    public class MutantGiantDeathray2 : Deathrays.BaseDeathray
    {
        public MutantGiantDeathray2() : base(FargoSoulsWorld.MasochistModeReal ? 600 + 180 : 600, "PhantasmalDeathrayML") { }

        public int dustTimer;
        public bool stall;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Phantasmal Deathray");
            ProjectileID.Sets.DismountsPlayersOnHit[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.netImportant = true;

            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TimeFreezeImmune = true;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().DeletionImmuneRank = 2;
        }

        public override bool? CanDamage()
        {
            return Projectile.scale >= 5f;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            if (!Main.dedServ && Main.LocalPlayer.active)
                Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().Screenshake = 2;

            Projectile.timeLeft = 2;

            Vector2? vector78 = null;
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            NPC npc = FargoSoulsUtil.NPCExists(Projectile.ai[1], ModContent.NPCType<NPCs.MutantBoss.MutantBoss>());
            if (npc != null)
            {
                Projectile.Center = npc.Center + Main.rand.NextVector2Circular(5, 5) + Vector2.UnitX.RotatedBy(npc.ai[3]) * (npc.ai[0] == -7 ? 100 : 175) * Projectile.scale / 10f;
                if (npc.ai[0] == -7) //death animation, not actual attack
                {
                    maxTime = 255;
                }
                else if (npc.ai[0] == -5) //final spark
                {
                    if (npc.localAI[2] > 30) //mutant is forcing a despawn
                    {
                        //so this should disappear too
                        if (Projectile.localAI[0] < maxTime - 90)
                            Projectile.localAI[0] = maxTime - 90;
                    }
                    else if (stall)
                    {
                        stall = false;

                        Projectile.localAI[0] -= 1;
                        Projectile.netUpdate = true;

                        npc.ai[2] -= 1;
                        npc.netUpdate = true;
                    }
                }
            }
            else
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Zombie, (int)Main.player[Main.myPlayer].Center.X, (int)Main.player[Main.myPlayer].Center.Y, 104, 1f, 0);
            }
            float num801 = 10f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            
            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * 3.14159274f / maxTime) * 5f * num801;
            if (FargoSoulsWorld.MasochistModeReal)
                Projectile.scale *= 5f;
            
            if (Projectile.scale > num801)
                Projectile.scale = num801;
            //float num804 = Projectile.velocity.ToRotation();
            //num804 += Projectile.ai[0];
            //Projectile.rotation = num804 - 1.57079637f;
            float num804 = npc.ai[3] - 1.57079637f;
            //if (Projectile.ai[0] != 0f) num804 -= (float)Math.PI;
            float oldRot = Projectile.rotation;
            Projectile.rotation = num804;
            num804 += 1.57079637f;
            Projectile.velocity = num804.ToRotationVector2();
            float num805 = 3f;
            float num806 = (float)Projectile.width;
            Vector2 samplingPoint = Projectile.Center;
            if (vector78.HasValue)
            {
                samplingPoint = vector78.Value;
            }
            float[] array3 = new float[(int)num805];
            //Collision.LaserScan(samplingPoint, Projectile.velocity, num806 * Projectile.scale, 3000f, array3);
            for (int i = 0; i < array3.Length; i++)
                array3[i] = 3000f;
            float num807 = 0f;
            int num3;
            for (int num808 = 0; num808 < array3.Length; num808 = num3 + 1)
            {
                num807 += array3[num808];
                num3 = num808;
            }
            num807 /= num805;
            float amount = 0.5f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], num807, amount);
            /*Vector2 vector79 = Projectile.Center + Projectile.velocity * (Projectile.localAI[1] - 14f);
            for (int num809 = 0; num809 < 2; num809 = num3 + 1)
            {
                float num810 = Projectile.velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? -1f : 1f) * 1.57079637f;
                float num811 = (float)Main.rand.NextDouble() * 2f + 2f;
                Vector2 vector80 = new Vector2((float)Math.Cos((double)num810) * num811, (float)Math.Sin((double)num810) * num811);
                int num812 = Dust.NewDust(vector79, 0, 0, 244, vector80.X, vector80.Y, 0, default(Color), 1f);
                Main.dust[num812].noGravity = true;
                Main.dust[num812].scale = 1.7f;
                num3 = num809;
            }
            if (Main.rand.NextBool(5))
            {
                Vector2 value29 = Projectile.velocity.RotatedBy(1.5707963705062866, default(Vector2)) * ((float)Main.rand.NextDouble() - 0.5f) * (float)Projectile.width;
                int num813 = Dust.NewDust(vector79 + value29 - Vector2.One * 4f, 8, 8, 244, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust = Main.dust[num813];
                dust.velocity *= 0.5f;
                Main.dust[num813].velocity.Y = -Math.Abs(Main.dust[num813].velocity.Y);
            }*/
            //DelegateMethods.v3_1 = new Vector3(0.3f, 0.65f, 0.7f);
            //Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], (float)Projectile.width * Projectile.scale, new Utils.PerLinePoint(DelegateMethods.CastLight));

            //if (--dustTimer < -1 && npc.ai[0] != -7)
            //{
            //    dustTimer = 50;

            //    float diff = MathHelper.WrapAngle(Projectile.rotation - oldRot);
            //    //if (npc.HasPlayerTarget && Math.Abs(MathHelper.WrapAngle(npc.DirectionTo(Main.player[npc.target].Center).ToRotation() - Projectile.velocity.ToRotation())) < Math.Abs(diff)) diff = 0;
            //    diff *= 15f;

            //    const int ring = 220; //LAUGH
            //    for (int i = 0; i < ring; ++i)
            //    {
            //        Vector2 speed = Projectile.velocity.RotatedBy(diff) * 24f;

            //        Vector2 vector2 = (-Vector2.UnitY.RotatedBy(i * 3.14159274101257 * 2 / ring) * new Vector2(8f, 16f)).RotatedBy(Projectile.velocity.ToRotation() + diff);
            //        int index2 = Dust.NewDust(npc.Center, 0, 0, 111, 0.0f, 0.0f, 0, new Color(), 1f);
            //        Main.dust[index2].scale = 2.5f;
            //        Main.dust[index2].noGravity = true;
            //        Main.dust[index2].position = npc.Center;
            //        Main.dust[index2].velocity = vector2 * 2.5f + speed;

            //        index2 = Dust.NewDust(npc.Center, 0, 0, 111, 0.0f, 0.0f, 0, new Color(), 1f);
            //        Main.dust[index2].scale = 2.5f;
            //        Main.dust[index2].noGravity = true;
            //        Main.dust[index2].position = npc.Center;
            //        Main.dust[index2].velocity = vector2 * 1.75f + speed * 2;
            //    }
            //}


            Projectile.frameCounter += Main.rand.Next(3);
            if (++Projectile.frameCounter > 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame > 15)
                    Projectile.frame = 0;
            }

            /*if (++Projectile.frameCounter > 3)
            {
                if (++Projectile.frame > 15)
                    Projectile.frame = 0;

                switch (Projectile.frame)
                {
                    case 1:
                    case 3:
                    case 9:
                    case 11: Projectile.frameCounter = 2; break;
                    default: Projectile.frameCounter = 0; break;
                }
            }*/

            if (Main.rand.NextBool(10))
                Projectile.spriteDirection *= -1;

            if (Main.LocalPlayer.active && Projectile.Colliding(Projectile.Hitbox, Main.LocalPlayer.Hitbox))
            {
                Main.LocalPlayer.immune = false;
                Main.LocalPlayer.immuneTime = 0;
                Main.LocalPlayer.hurtCooldowns[0] = 0;
                Main.LocalPlayer.hurtCooldowns[1] = 0;
            }
        }

        private int hits;

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            base.ModifyHitPlayer(target, ref damage, ref crit);
            DamageRampup(ref damage);
            if (hits > 180)
                target.endurance = 0;
        }

        private void DamageRampup(ref int damage)
        {
            int tempHits = hits - 90;
            if (tempHits > 0)
            {
                double modifier = Math.Min(1.0 + tempHits / 6.0, 100.0);
                damage = (int)(damage * modifier);
            }
            else
            {
                damage = (int)(hits / 90.0 * damage);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            hits++;

            stall = true;

            if (FargoSoulsWorld.EternityMode)
            {
                target.GetModPlayer<FargoSoulsPlayer>().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaul>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFang>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 600);
            target.ClearBuff(ModContent.BuffType<GoldenStasis>());
            
            //if (FargowiltasSouls.Instance.MasomodeEXLoaded) target.AddBuff(ModLoader.GetMod("MasomodeEX").BuffType("MutantJudgement"), 3600);

            target.immune = false;
            target.immuneTime = 0;
            target.hurtCooldowns[0] = 0;
            target.hurtCooldowns[1] = 0;

            target.velocity = -0.4f * Vector2.UnitY;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
            {
                return false;
            }

            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D texture2D19 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/Mutant/MutantDeathray_" + Projectile.frame.ToString(), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D20 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/Mutant/MutantDeathray2_" + Projectile.frame.ToString(), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D21 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/" + texture + "3", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float num223 = Projectile.localAI[1];
            Color color44 = new Color(255, 255, 255, 100) * 0.9f;
            SpriteBatch arg_ABD8_0 = Main.spriteBatch;
            Texture2D arg_ABD8_1 = texture2D19;
            Vector2 arg_ABD8_2 = Projectile.Center - Main.screenPosition;
            Rectangle? sourceRectangle2 = null;
            arg_ABD8_0.Draw(arg_ABD8_1, arg_ABD8_2, sourceRectangle2, color44, Projectile.rotation, texture2D19.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            num223 -= (texture2D19.Height / 2 + texture2D21.Height) * Projectile.scale;
            Vector2 value20 = Projectile.Center;
            value20 += Projectile.velocity * Projectile.scale * texture2D19.Height / 2f;
            if (num223 > 0f)
            {
                float num224 = 0f;
                Rectangle rectangle7 = new Rectangle(0, 0, texture2D20.Width, 30);
                while (num224 + 1f < num223)
                {
                    if (num223 - num224 < rectangle7.Height)
                    {
                        rectangle7.Height = (int)(num223 - num224);
                    }

                    Main.EntitySpriteDraw(texture2D20, value20 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle7), color44, Projectile.rotation, new Vector2(rectangle7.Width / 2, 0f), Projectile.scale, spriteEffects, 0);
                    num224 += rectangle7.Height * Projectile.scale;
                    value20 += Projectile.velocity * rectangle7.Height * Projectile.scale;
                    rectangle7.Y += 30;
                    if (rectangle7.Y + rectangle7.Height > texture2D20.Height)
                    {
                        rectangle7.Y = 0;
                    }
                }
            }
            SpriteBatch arg_AE2D_0 = Main.spriteBatch;
            Texture2D arg_AE2D_1 = texture2D21;
            Vector2 arg_AE2D_2 = value20 - Main.screenPosition;
            sourceRectangle2 = null;
            arg_AE2D_0.Draw(arg_AE2D_1, arg_AE2D_2, sourceRectangle2, color44, Projectile.rotation, texture2D21.Frame(1, 1, 0, 0).Top(), Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}