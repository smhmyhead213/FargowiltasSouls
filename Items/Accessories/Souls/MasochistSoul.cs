﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.Toggler;
using FargowiltasSouls.Buffs.Minions;
using FargowiltasSouls.Items.Accessories.Masomode;
using FargowiltasSouls.Items.Materials;

namespace FargowiltasSouls.Items.Accessories.Souls
{
    [AutoloadEquip(/*EquipType.Head, */EquipType.Front, EquipType.Back, EquipType.Shield)]
    public class MasochistSoul : BaseSoul
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            DisplayName.SetDefault("Soul of the Siblings");
            Tooltip.SetDefault(
@"Increases wing time by 200%, armor penetration by 50, and movement speed by 20%
Increases max life by 100%, damage by 50%, and damage reduction by 10%
Increases life regen drastically, increases max number of minions and sentries by 2
Grants gravity control, fastfall, and immunity to knockback, almost all Eternity Mode debuffs, and more
Grants autofire to all weapons and you automatically use mana potions when needed
Your attacks create additional attacks, hearts, and inflict a cocktail of Eternity Mode debuffs
Press the Fireball Dash key to perform a short invincible dash, zoom with right click
Certain enemies will drop potions when defeated, 50% discount on reforges, you respawn with more life
You respawn twice as fast, attacks spawn honey, have improved night vision, and erupt into various attacks when injured
Prevents boss spawns, increases spawn rate, increases loot, and attacks may squeak and deal 1 damage to you
Reduces hurtbox size, hold the Precision Seal key to disable dashes and double jumps
Right Click to parry attacks with extremely tight timing
Use to teleport to your last death point
Summons the aid of all Eternity Mode bosses to your side
'Embrace eternity'");

            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "受虐之魂");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, 
@"延长200%飞行时间，增加50点护甲穿透和20%移动速度
增加100%最大生命值、50%伤害和10%伤害减免
大幅增加生命恢复速度，+10最大召唤和哨兵栏
允许你控制重力，使你免疫击退、摔落伤害和近乎所有的永恒模式减益
允许所有武器自动挥舞且在需要时自动使用魔力药水
在地牢外减少武装和魔法骷髅对你的敌意，右键缩放视域
你的攻击会额外生成心, 并造成混合的永恒模式减益
按下'火球冲刺'键后会进行短距离无敌冲刺
大多敌人在死亡时会掉落随机的药水，减少50%重铸价格，你在重生时以更多生命重生
重生速度翻倍，攻击时会释放蜜蜂，增强夜视效果，受到伤害时释放各种攻击
阻止Boss自然生成，增加刷怪率，敌人死亡后会掉落更多战利品，你在受到伤害时有几率发出吱吱声，并使这次受到的伤害降至1点
召唤所有永恒模式Boss的援助到你身边
'拥抱永恒'");
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.value = 5000000;
            Item.defense = 30;
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item6;
        }

        protected override Color? nameColor => new Color(255, 51, 153, 0);

        public override bool CanUseItem(Player player) => player.lastDeathPostion != Vector2.Zero;

        public override bool? UseItem(Player player)
        {
            for (int index = 0; index < 70; ++index)
            {
                int d = Dust.NewDust(player.position, player.width, player.height, 87, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, new Color(), 1.5f);
                Main.dust[d].velocity *= 4f;
                Main.dust[d].noGravity = true;
            }

            player.grappling[0] = -1;
            player.grapCount = 0;
            for (int index = 0; index < Main.maxProjectiles; ++index)
            {
                if (Main.projectile[index].active && Main.projectile[index].owner == player.whoAmI && Main.projectile[index].aiStyle == 7)
                    Main.projectile[index].Kill();
            }

            if (player.whoAmI == Main.myPlayer)
            {
                player.Teleport(player.lastDeathPostion, 1);
                player.velocity = Vector2.Zero;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.lastDeathPostion.X, player.lastDeathPostion.Y, 1);
            }

            for (int index = 0; index < 70; ++index)
            {
                int d = Dust.NewDust(player.position, player.width, player.height, 87, 0.0f, 0.0f, 150, new Color(), 1.5f);
                Main.dust[d].velocity *= 4f;
                Main.dust[d].noGravity = true;
            }

            return true;
        }

        public override void UpdateInventory(Player player)
        {
            player.GetModPlayer<FargoSoulsPlayer>().BionomicPassiveEffect();
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FargoSoulsPlayer fargoPlayer = player.GetModPlayer<FargoSoulsPlayer>();
            fargoPlayer.MasochistSoul = true;

            player.AddBuff(ModContent.BuffType<SouloftheMasochist>(), 2);

            //stat modifiers
            player.GetDamage(DamageClass.Generic) += 0.5f;
            player.endurance += 0.1f;
            player.maxMinions += 2;
            player.maxTurrets += 2;
            player.GetArmorPenetration(DamageClass.Generic) += 50;
            player.statLifeMax2 += player.statLifeMax;
            if (!fargoPlayer.MutantPresence)
            {
                player.lifeRegen += 7;
                player.lifeRegenTime += 7;
                player.lifeRegenCount += 7;
            }
            fargoPlayer.WingTimeModifier += 2f;
            player.moveSpeed += 0.2f;

            //slimy shield
            player.buffImmune[BuffID.Slimed] = true;

            if (player.GetToggleValue("SlimeFalling"))
            {
                player.maxFallSpeed *= 1.5f;
            }

            if (player.GetToggleValue("MasoSlime"))
            {
                fargoPlayer.SlimyShieldItem = Item;
            }

            //agitating lens
            fargoPlayer.AgitatingLensItem = Item;

            //queen stinger
            //player.honey = true;
            player.npcTypeNoAggro[210] = true;
            player.npcTypeNoAggro[211] = true;
            player.npcTypeNoAggro[42] = true;
            player.npcTypeNoAggro[176] = true;
            player.npcTypeNoAggro[231] = true;
            player.npcTypeNoAggro[232] = true;
            player.npcTypeNoAggro[233] = true;
            player.npcTypeNoAggro[234] = true;
            player.npcTypeNoAggro[235] = true;
            fargoPlayer.QueenStingerItem = Item;

            //necromantic brew
            fargoPlayer.NecromanticBrewItem = Item;

            //supreme deathbringer fairy
            fargoPlayer.SupremeDeathbringerFairy = true;

            //pure heart
            fargoPlayer.PureHeart = true;

            //corrupt heart
            fargoPlayer.CorruptHeartItem = Item;
            player.hasMagiluminescence = true;
            if (fargoPlayer.CorruptHeartCD > 0)
                fargoPlayer.CorruptHeartCD -= 2;

            //gutted heart
            fargoPlayer.GuttedHeart = true;
            fargoPlayer.GuttedHeartCD -= 2; //faster spawns

            //mutant antibodies
            player.buffImmune[BuffID.Wet] = true;
            player.buffImmune[BuffID.Rabies] = true;
            fargoPlayer.MutantAntibodies = true;
            if (player.mount.Active && player.mount.Type == MountID.CuteFishron)
                player.dripping = true;

            //lump of flesh
            player.buffImmune[BuffID.Blackout] = true;
            player.buffImmune[BuffID.Obstructed] = true;
            player.buffImmune[BuffID.Dazed] = true;
            fargoPlayer.SkullCharm = true;
            player.buffImmune[ModContent.BuffType<CrystalSkull>()] = true;
            /*if (!player.ZoneDungeon)
            {
                player.npcTypeNoAggro[NPCID.SkeletonSniper] = true;
                player.npcTypeNoAggro[NPCID.SkeletonCommando] = true;
                player.npcTypeNoAggro[NPCID.TacticalSkeleton] = true;
                player.npcTypeNoAggro[NPCID.DiabolistRed] = true;
                player.npcTypeNoAggro[NPCID.DiabolistWhite] = true;
                player.npcTypeNoAggro[NPCID.Necromancer] = true;
                player.npcTypeNoAggro[NPCID.NecromancerArmored] = true;
                player.npcTypeNoAggro[NPCID.RaggedCaster] = true;
                player.npcTypeNoAggro[NPCID.RaggedCasterOpenCoat] = true;
            }*/

            //sinister icon
            if (player.GetToggleValue("MasoIcon"))
                fargoPlayer.SinisterIcon = true;
            if (player.GetToggleValue("MasoIconDrops"))
                fargoPlayer.SinisterIconDrops = true;

            //sparkling adoration
            /*if (SoulConfig.Instance.GetValue(SoulConfig.Instance.Graze, false))
                player.GetModPlayer<FargoSoulsPlayer>().Graze = true;

            if (SoulConfig.Instance.GetValue(SoulConfig.Instance.DevianttHearts))
                player.GetModPlayer<FargoSoulsPlayer>().DevianttHearts = true;*/

            //dragon fang
            if (player.GetToggleValue("MasoClipped"))
                fargoPlayer.DragonFang = true;

            //frigid gemstone
            player.buffImmune[BuffID.Frostburn] = true;
            if (player.GetToggleValue("MasoFrigid"))
            {
                fargoPlayer.FrigidGemstoneItem = Item;
                if (fargoPlayer.FrigidGemstoneCD > 0)
                    fargoPlayer.FrigidGemstoneCD -= 5;
            }

            //wretched pouch
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[ModContent.BuffType<Shadowflame>()] = true;
            player.GetModPlayer<FargoSoulsPlayer>().WretchedPouchItem = Item;

            //sands of time
            player.buffImmune[BuffID.WindPushed] = true;
            fargoPlayer.SandsofTime = true;

            //mystic skull
            player.buffImmune[BuffID.Suffocation] = true;
            player.manaFlower = true;

            //security wallet
            fargoPlayer.SecurityWallet = true;

            //carrot
            player.nightVision = true;
            if (player.GetToggleValue("MasoCarrot", false))
                player.scope = true;

            //squeaky toy
            fargoPlayer.SqueakyAcc = true;

            //tribal charm
            player.buffImmune[BuffID.Webbed] = true;
            fargoPlayer.TribalCharm = true;

            //nymph's perfume
            player.buffImmune[BuffID.Lovestruck] = true;
            player.buffImmune[BuffID.Stinky] = true;
            fargoPlayer.NymphsPerfumeRespawn = true;
            if (player.GetToggleValue("MasoNymph"))
            {
                fargoPlayer.NymphsPerfume = true;
                if (fargoPlayer.NymphsPerfumeCD > 0)
                    fargoPlayer.NymphsPerfumeCD -= 10;
            }

            //tim's concoction
            if (player.GetToggleValue("MasoConcoction"))
                player.GetModPlayer<FargoSoulsPlayer>().TimsConcoction = true;

            //dubious circuitry
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.Ichor] = true;
            fargoPlayer.FusedLens = true;
            fargoPlayer.GroundStick = true;
            player.noKnockback = true;

            //magical bulb
            player.buffImmune[BuffID.Venom] = true;

            //ice queen's crown
            player.buffImmune[BuffID.Frozen] = true;
            fargoPlayer.IceQueensCrown = true;

            //lihzahrd treasure
            player.buffImmune[BuffID.Burning] = true;
            fargoPlayer.LihzahrdTreasureBoxItem = Item;

            //saucer control console
            player.buffImmune[BuffID.Electrified] = true;

            //betsy's heart
            player.buffImmune[BuffID.OgreSpit] = true;
            player.buffImmune[BuffID.WitheredWeapon] = true;
            player.buffImmune[BuffID.WitheredArmor] = true;
            fargoPlayer.BetsysHeartItem = Item;

            //celestial rune/pumpking's cape
            fargoPlayer.CelestialRuneItem = Item;
            fargoPlayer.PumpkingsCapeItem = Item;
            fargoPlayer.AdditionalAttacks = true;
            if (fargoPlayer.AdditionalAttacksTimer > 0)
                fargoPlayer.AdditionalAttacksTimer -= 2;

            //chalice
            fargoPlayer.MoonChalice = true;

            //galactic globe
            player.buffImmune[BuffID.VortexDebuff] = true;
            //player.buffImmune[BuffID.ChaosState] = true;
            fargoPlayer.GravityGlobeEXItem = Item;
            if (player.GetToggleValue("MasoGrav"))
                player.gravControl = true;

            //heart of maso
            fargoPlayer.MasochistHeart = true;
            player.buffImmune[BuffID.MoonLeech] = true;

            //precision seal
            fargoPlayer.PrecisionSeal = true;
            if (player.GetToggleValue("PrecisionSealHurtbox", false))
                fargoPlayer.PrecisionSealHurtbox = true;

            //dread shell
            if (player.GetToggleValue("DreadShellParry"))
                player.GetModPlayer<FargoSoulsPlayer>().DreadShellItem = Item;

            //sadism
            player.buffImmune[ModContent.BuffType<Anticoagulation>()] = true;
            player.buffImmune[ModContent.BuffType<Antisocial>()] = true;
            player.buffImmune[ModContent.BuffType<Atrophied>()] = true;
            player.buffImmune[ModContent.BuffType<Berserked>()] = true;
            player.buffImmune[ModContent.BuffType<Bloodthirsty>()] = true;
            player.buffImmune[ModContent.BuffType<ClippedWings>()] = true;
            player.buffImmune[ModContent.BuffType<Crippled>()] = true;
            player.buffImmune[ModContent.BuffType<CurseoftheMoon>()] = true;
            player.buffImmune[ModContent.BuffType<Defenseless>()] = true;
            player.buffImmune[ModContent.BuffType<FlamesoftheUniverse>()] = true;
            player.buffImmune[ModContent.BuffType<Flipped>()] = true;
            player.buffImmune[ModContent.BuffType<FlippedHallow>()] = true;
            player.buffImmune[ModContent.BuffType<Fused>()] = true;
            //player.buffImmune[ModContent.BuffType<GodEater>()] = true;
            player.buffImmune[ModContent.BuffType<Guilty>()] = true;
            player.buffImmune[ModContent.BuffType<Hexed>()] = true;
            player.buffImmune[ModContent.BuffType<Hypothermia>()] = true;
            player.buffImmune[ModContent.BuffType<Infested>()] = true;
            player.buffImmune[ModContent.BuffType<IvyVenom>()] = true;
            player.buffImmune[ModContent.BuffType<Jammed>()] = true;
            player.buffImmune[ModContent.BuffType<Lethargic>()] = true;
            player.buffImmune[ModContent.BuffType<LihzahrdCurse>()] = true;
            player.buffImmune[ModContent.BuffType<LightningRod>()] = true;
            player.buffImmune[ModContent.BuffType<LivingWasteland>()] = true;
            player.buffImmune[ModContent.BuffType<Lovestruck>()] = true;
            player.buffImmune[ModContent.BuffType<LowGround>()] = true;
            player.buffImmune[ModContent.BuffType<MarkedforDeath>()] = true;
            player.buffImmune[ModContent.BuffType<Midas>()] = true;
            player.buffImmune[ModContent.BuffType<MutantNibble>()] = true;
            player.buffImmune[ModContent.BuffType<NanoInjection>()] = true;
            player.buffImmune[ModContent.BuffType<NullificationCurse>()] = true;
            player.buffImmune[ModContent.BuffType<Oiled>()] = true;
            player.buffImmune[ModContent.BuffType<OceanicMaul>()] = true;
            player.buffImmune[ModContent.BuffType<Purified>()] = true;
            player.buffImmune[ModContent.BuffType<ReverseManaFlow>()] = true;
            player.buffImmune[ModContent.BuffType<Rotting>()] = true;
            player.buffImmune[ModContent.BuffType<Shadowflame>()] = true;
            player.buffImmune[ModContent.BuffType<Smite>()] = true;
            player.buffImmune[ModContent.BuffType<Buffs.Masomode.SqueakyToy>()] = true;
            player.buffImmune[ModContent.BuffType<Swarming>()] = true;
            player.buffImmune[ModContent.BuffType<Stunned>()] = true;
            player.buffImmune[ModContent.BuffType<Unstable>()] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ModContent.ItemType<SinisterIcon>())
            .AddIngredient(ModContent.ItemType<SupremeDeathbringerFairy>())
            .AddIngredient(ModContent.ItemType<BionomicCluster>())
            .AddIngredient(ModContent.ItemType<DubiousCircuitry>())
            .AddIngredient(ModContent.ItemType<PureHeart>())
            .AddIngredient(ModContent.ItemType<LumpOfFlesh>())
            .AddIngredient(ModContent.ItemType<ChaliceoftheMoon>())
            .AddIngredient(ModContent.ItemType<HeartoftheMasochist>())
            .AddIngredient(ModContent.ItemType<PrecisionSeal>())
            .AddIngredient(ModContent.ItemType<AbomEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<DeviatingEnergy>(), 15)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))

            
            .Register();
        }
    }
}
