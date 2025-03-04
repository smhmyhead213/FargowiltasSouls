﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.EternityMode;
using FargowiltasSouls.EternityMode.Content.Boss.HM;

namespace FargowiltasSouls.Projectiles.Masomode
{
    public class PrimeTrail : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Trail");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.8f;
        }

        public override void AI()
        {
            bool fade = false;

            NPC npc = FargoSoulsUtil.NPCExists(Projectile.ai[0]);
            if (npc != null)
            {
                Projectile.Center = npc.Center;
                if (Projectile.ai[1] == 0) //swipe limb
                {
                    if (!npc.GetEModeNPCMod<PrimeLimb>().IsSwipeLimb || npc.ai[2] < 140)
                        fade = true;
                }
                else if (Projectile.ai[1] == 1)
                {
                    if (npc.GetEModeNPCMod<PrimeLimb>().IsSwipeLimb || (Main.npc[(int)npc.ai[1]].ai[1] != 1 && Main.npc[(int)npc.ai[1]].ai[1] != 2))
                        fade = true;
                }
            }
            else
            {
                fade = true;
            }

            if (fade)
            {
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                {
                    Projectile.alpha = 255;
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.alpha -= Projectile.ai[1] == 0 ? 16 : 8;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float increment = 0.25f;
            if (Projectile.ai[1] == 1f)
                increment = 0.1f;

            for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += increment)
            {
                int max0 = (int)i - 1;
                if (max0 < 0)
                    continue;
                Player player = Main.player[Projectile.owner];
                Texture2D glow = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Color color27 = (Projectile.ai[1] == 0f ? new Color(255, 0, 0, 210) : new Color(191, 51, 255, 210)) * 0.25f * Projectile.Opacity;
                if (Projectile.ai[1] == 0f)
                    color27 *= 0.5f;
                color27 *= ((float)ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                float scale = Projectile.scale;
                scale *= ((float)ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 center = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], (1 - i % 1));
                float smoothtrail = i % 1 * (float)Math.PI / 6.85f;

                center += Projectile.Size / 2;
                
                Main.EntitySpriteDraw(
                    glow,
                    center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY),
                    null,
                    color27,
                    Projectile.rotation,
                    glow.Size() / 2,
                    scale,
                    SpriteEffects.None,
                    0);
            }

            return false;
        }
    }
}