using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles.Champions
{
    public class TimberSnowball : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_109";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snowball");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.timeLeft = 600;

            Projectile.tileCollide = false;
            Projectile.coldDamage = true;
            Projectile.scale = 2f;
            CooldownSlot = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.Center, 1);
                for (int index1 = 0; index1 < 5; ++index1)
                {
                    int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 51);
                    Dust dust = Main.dust[index2];
                    dust.velocity = dust.velocity * 0.6f;
                }
            }
            
            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity.Y += 0.3f;
                if (Projectile.velocity.Y > 0)
                {
                    int p = Player.FindClosest(Projectile.Center, 0, 0);
                    if (p != -1)
                    {
                        Projectile.velocity = Projectile.DirectionTo(Main.player[p].Center) * 30;
                        Projectile.ai[0] = 1f;
                        Projectile.netUpdate = true;
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (FargoSoulsWorld.EternityMode)
                target.AddBuff(BuffID.Chilled, 180);
            target.AddBuff(BuffID.Frostburn, 180);
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.Center, 1);
            for (int index1 = 0; index1 < 5; ++index1)
            {
                int index2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 51);
                Dust dust = Main.dust[index2];
                dust.velocity = dust.velocity * 0.6f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }
    }
}