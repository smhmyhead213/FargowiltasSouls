﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.AbomBoss
{
    public class AbomSword : Deathrays.BaseDeathray
    {
        public AbomSword() : base(300, "AbomDeathray") { }

        public int counter;
        public bool spawnedHandle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Styx Gazer Blade");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().DeletionImmuneRank = 2;
            Projectile.extraUpdates = 1;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Vector2? vector78 = null;
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            NPC abom = FargoSoulsUtil.NPCExists(Projectile.ai[1], ModContent.NPCType<NPCs.AbomBoss.AbomBoss>());
            if (abom == null)
            {
                Projectile.Kill();
                return;
            }
            else
            {
                Projectile.Center = abom.Center;
            }
            if (Projectile.velocity.HasNaNs() || Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity = -Vector2.UnitY;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Zombie, (int)Projectile.position.X, (int)Projectile.position.Y, 104, 1f, 0);
            }
            float num801 = 1f;
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            Projectile.scale = (float)Math.Sin(Projectile.localAI[0] * 3.14159274f / maxTime) * num801 * 6f;
            if (Projectile.scale > num801)
            {
                Projectile.scale = num801;
            }
            float num804 = Projectile.velocity.ToRotation();
            if ((abom.velocity != Vector2.Zero || abom.ai[0] == 19) && abom.ai[0] != 20)
                num804 += Projectile.ai[0] / Projectile.MaxUpdates;
            Projectile.rotation = num804 - 1.57079637f;
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
            if (Projectile.localAI[0] % 2 == 0)
            {
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

                if (abom.velocity != Vector2.Zero && --counter < 0)
                {
                    counter = 5;
                    if (Main.netMode != NetmodeID.MultiplayerClient) //spawn bonus projs
                    {
                        Vector2 spawnPos = Projectile.Center;
                        Vector2 vel = Projectile.velocity.RotatedBy(Math.PI / 2 * Math.Sign(Projectile.ai[0]));
                        const int max = 15;
                        for (int i = 1; i <= max; i++)
                        {
                            spawnPos += Projectile.velocity * 3000f / max;
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), spawnPos, vel, ModContent.ProjectileType<AbomSickle2>(), Projectile.damage, 0f, Projectile.owner);
                        }
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    int d = Dust.NewDust(Projectile.position + Projectile.velocity * Main.rand.NextFloat(3000), Projectile.width, Projectile.height, 87, 0f, 0f, 0, Color.White, 6f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 4f;
                }

                if (++Projectile.frameCounter > 3)
                {
                    Projectile.frameCounter = 0;
                    if (++Projectile.frame > 10)
                        Projectile.frame = 0;
                }
            }

            if (!spawnedHandle)
            {
                spawnedHandle = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<AbomSwordHandle>(), Projectile.damage, Projectile.knockBack, Projectile.owner, (float)Math.PI / 2, Projectile.identity);
                    Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<AbomSwordHandle>(), Projectile.damage, Projectile.knockBack, Projectile.owner, -(float)Math.PI / 2, Projectile.identity);
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.velocity.X = target.Center.X < Main.npc[(int)Projectile.ai[1]].Center.X ? -15f : 15f;
            target.velocity.Y = -10f;

            Projectile.NewProjectile(Projectile.InheritSource(Projectile), target.Center + Main.rand.NextVector2Circular(100, 100), Vector2.Zero, ModContent.ProjectileType<AbomBlast>(), 0, 0f, Projectile.owner);

            if (FargoSoulsWorld.EternityMode)
            {
                target.AddBuff(ModContent.BuffType<Buffs.Boss.AbomFang>(), 300);
                target.AddBuff(BuffID.Burning, 180);
            }
            target.AddBuff(BuffID.WitheredArmor, 600);
            target.AddBuff(BuffID.WitheredWeapon, 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
            {
                return false;
            }
            Texture2D texture2D19 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/Abom/AbomDeathray_" + Projectile.frame.ToString(), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D20 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/Abom/AbomDeathray2_" + Projectile.frame.ToString(), ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture2D21 = FargowiltasSouls.Instance.Assets.Request<Texture2D>("Projectiles/Deathrays/" + texture + "3", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float num223 = Projectile.localAI[1];
            Color color44 = new Color(255, 255, 255, 0) * 0.95f;
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

                    Main.EntitySpriteDraw(texture2D20, value20 - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle7), color44, Projectile.rotation, new Vector2(rectangle7.Width / 2, 0f), Projectile.scale, SpriteEffects.None, 0);
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
            arg_AE2D_0.Draw(arg_AE2D_1, arg_AE2D_2, sourceRectangle2, color44, Projectile.rotation, texture2D21.Frame(1, 1, 0, 0).Top(), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}