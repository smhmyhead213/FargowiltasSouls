﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.Champions
{
    public class CosmosFireball2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_467";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cosmic Fireball");
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1;

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 14);
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }

            if (--Projectile.ai[0] == 0)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 0.1f;
                Projectile.netUpdate = true;
            }

            if (--Projectile.ai[1] == 0)
            {
                Projectile.Kill();
                return;
            }

            Lighting.AddLight(Projectile.Center, 1.1f, 0.9f, 0.4f);

            ++Projectile.localAI[0];
            if ((double)Projectile.localAI[0] == 12.0) //loads of vanilla dust :echprime:
            {
                Projectile.localAI[0] = 0.0f;
                for (int index1 = 0; index1 < 12; ++index1)
                {
                    Vector2 vector2 = (Vector2.UnitX * (float)-Projectile.width / 2f + -Vector2.UnitY.RotatedBy((double)index1 * 3.14159274101257 / 6.0, new Vector2()) * new Vector2(8f, 16f)).RotatedBy((double)Projectile.rotation - 1.57079637050629, new Vector2());
                    int index2 = Dust.NewDust(Projectile.Center, 0, 0, 6, 0.0f, 0.0f, 160, new Color(), 1f);
                    Main.dust[index2].scale = 1.1f;
                    Main.dust[index2].noGravity = true;
                    Main.dust[index2].position = Projectile.Center + vector2;
                    Main.dust[index2].velocity = Projectile.velocity * 0.1f;
                    Main.dust[index2].velocity = Vector2.Normalize(Projectile.Center - Projectile.velocity * 3f - Main.dust[index2].position) * 1.25f;
                }
            }
            if (Main.rand.NextBool(4))
            {
                for (int index1 = 0; index1 < 1; ++index1)
                {
                    Vector2 vector2 = -Vector2.UnitX.RotatedByRandom(0.196349546313286).RotatedBy((double)Projectile.velocity.ToRotation(), new Vector2());
                    int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 31, 0.0f, 0.0f, 100, new Color(), 1f);
                    Main.dust[index2].velocity *= 0.1f;
                    Main.dust[index2].position = Projectile.Center + vector2 * (float)Projectile.width / 2f;
                    Main.dust[index2].fadeIn = 0.9f;
                }
            }
            if (Main.rand.NextBool(32))
            {
                for (int index1 = 0; index1 < 1; ++index1)
                {
                    Vector2 vector2 = -Vector2.UnitX.RotatedByRandom(0.392699092626572).RotatedBy((double)Projectile.velocity.ToRotation(), new Vector2());
                    int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 31, 0.0f, 0.0f, 155, new Color(), 0.8f);
                    Main.dust[index2].velocity *= 0.3f;
                    Main.dust[index2].position = Projectile.Center + vector2 * (float)Projectile.width / 2f;
                    if (Main.rand.NextBool())
                        Main.dust[index2].fadeIn = 1.4f;
                }
            }
            if (Main.rand.NextBool())
            {
                for (int index1 = 0; index1 < 2; ++index1)
                {
                    Vector2 vector2 = -Vector2.UnitX.RotatedByRandom(0.785398185253143).RotatedBy((double)Projectile.velocity.ToRotation(), new Vector2());
                    int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 6, 0.0f, 0.0f, 0, new Color(), 1.2f);
                    Main.dust[index2].velocity *= 0.3f;
                    Main.dust[index2].noGravity = true;
                    Main.dust[index2].position = Projectile.Center + vector2 * (float)Projectile.width / 2f;
                    if (Main.rand.NextBool())
                        Main.dust[index2].fadeIn = 1.4f;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int p = Player.FindClosest(Projectile.Center, 0, 0);
                if (p != -1)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 12f * Projectile.DirectionTo(Main.player[p].Center),
                        ProjectileID.CultistBossFireBall, Projectile.damage, 0f, Main.myPlayer);
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (FargoSoulsWorld.EternityMode)
                target.AddBuff(BuffID.Burning, 120);
            target.AddBuff(BuffID.OnFire, 300);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}