using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.NPCs;
using FargowiltasSouls.Buffs.Masomode;

namespace FargowiltasSouls.Projectiles.Champions
{
    public class CosmosBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_462";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cosmic Bolt");
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.PhantasmalBolt;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.scale = 2f;
            Projectile.hostile = true;
            Projectile.extraUpdates = 3;

            Projectile.timeLeft = 75 * 4;
            CooldownSlot = 1;

            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().DeletionImmuneRank = 1;
        }

        public override void AI()
        {
            int index = Dust.NewDust(Projectile.Center, 0, 0, 229, 0.0f, 0.0f, 100, new Color(), 1f);
            Main.dust[index].noLight = true;
            Main.dust[index].noGravity = true;
            Main.dust[index].velocity = Projectile.velocity;
            Main.dust[index].position -= Vector2.One * 4f;
            Main.dust[index].scale = 0.8f;
            /*if (++Projectile.frameCounter >= 12 * 12)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 5)
                    Projectile.frame = 0;
            }*/

            if (FargoSoulsWorld.EternityMode && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.championBoss, ModContent.NPCType<NPCs.Champions.CosmosChampion>()))
            {
                float rotation = Projectile.velocity.ToRotation();
                Vector2 vel = Main.player[Main.npc[EModeGlobalNPC.championBoss].target].Center - Projectile.Center;
                float targetAngle = vel.ToRotation();
                Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0f).RotatedBy(rotation.AngleLerp(targetAngle, 0.001f));
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<CosmosDeathray>(), Projectile.damage, 0f, Main.myPlayer, 1f);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (FargoSoulsWorld.EternityMode)
                target.AddBuff(ModContent.BuffType<CurseoftheMoon>(), 360);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 128) * (1f - Projectile.alpha / 255f);
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