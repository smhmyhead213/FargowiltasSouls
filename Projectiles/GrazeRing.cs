using Microsoft.Xna.Framework;
using Terraria;
using FargowiltasSouls.Toggler;

namespace FargowiltasSouls.Projectiles
{
    public class GrazeRing : GlowRingHollow
    {
        public override string Texture => "FargowiltasSouls/Projectiles/GlowRingHollow";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glow Ring");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.alpha = 0;
            Projectile.hostile = false;
            Projectile.friendly = true;
            color = Color.HotPink;

            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TimeFreezeImmune = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FargoSoulsPlayer fargoPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (!player.active || player.dead || player.ghost || (Projectile.owner == Main.myPlayer && (!fargoPlayer.Graze || !player.GetToggleValue("MasoGrazeRing", false))))
            {
                Projectile.Kill();
                return;
            }

            float radius = Player.defaultHeight + fargoPlayer.GrazeRadius;

            Projectile.timeLeft = 2;
            Projectile.Center = player.Center;

            Projectile.alpha = 0;

            Projectile.scale = radius * 2f / 1000f;

            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = (int)(1000 * Projectile.scale);
            Projectile.Center = Projectile.position;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return base.GetAlpha(lightColor) * 0.8f;
        }
    }
}