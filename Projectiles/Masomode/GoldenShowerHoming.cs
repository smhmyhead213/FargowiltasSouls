﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.Masomode
{
    public class GoldenShowerHoming : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_288";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Golden Shower");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 900;
            Projectile.hostile = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
            }

            if (Projectile.ai[1] == 0)
            {
                Player player = FargoSoulsUtil.PlayerExists(Projectile.ai[0]);
                if (player == null)
                {
                    Projectile.ai[1] = 1;
                    Projectile.netUpdate = true;
                    Projectile.timeLeft = 180;
                    Projectile.velocity.Normalize();
                }
                else
                {
                    float rotation = Projectile.velocity.ToRotation();
                    Vector2 vel = player.Center - Projectile.Center;
                    float targetAngle = vel.ToRotation();
                    Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0f).RotatedBy(rotation.AngleLerp(targetAngle, Projectile.localAI[0]));

                    if (Projectile.localAI[0] < 0.5f)
                        Projectile.localAI[0] += 1f / 3000f;

                    if (vel.Length() < 250 || !player.active || player.dead || player.ghost)
                    {
                        Projectile.ai[1] = 1;
                        Projectile.netUpdate = true;
                        Projectile.timeLeft = 180;
                        Projectile.velocity.Normalize();
                    }
                }
            }
            else if (Projectile.ai[1] > 0)
            {
                if (++Projectile.ai[1] < 120)
                {
                    Projectile.velocity *= 1.03f;

                    float rotation = Projectile.velocity.ToRotation();
                    Vector2 vel = Main.player[(int)Projectile.ai[0]].Center - Projectile.Center;
                    float targetAngle = vel.ToRotation();
                    Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0f).RotatedBy(rotation.AngleLerp(targetAngle, 0.025f));
                }
            }
            else //ai1 below 0 rn
            {
                Projectile.ai[1]++;
            }

            for (int i = 0; i < 3; i++) //vanilla dusts
            {
                for (int j = 0; j < 3; ++j)
                {
                    int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 170, 0.0f, 0.0f, 100, default, 1f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.1f;
                    Main.dust[d].velocity += Projectile.velocity * 0.5f;
                    Main.dust[d].position -= Projectile.velocity / 3 * j;
                }
                if (Main.rand.NextBool(8))
                {
                    int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 170, 0.0f, 0.0f, 100, default, 0.5f);
                    Main.dust[d].velocity *= 0.25f;
                    Main.dust[d].velocity += Projectile.velocity * 0.5f;
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 900);
        }
    }
}