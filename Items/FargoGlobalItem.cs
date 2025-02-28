using System;
using System.Collections.Generic;
using FargowiltasSouls.Buffs.Souls;
//using FargowiltasSouls.Buffs.Souls;
//using FargowiltasSouls.Projectiles.Critters;
using FargowiltasSouls.Toggler;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Items
{
    public class FargoGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Acorn || item.type == ItemID.Bone)
            {
                item.ammo = item.type;
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (player.manaCost <= 0f) player.manaCost = 0f;
        }

        public override void GrabRange(Item item, Player player, ref int grabRange)
        {
            FargoSoulsPlayer p = player.GetModPlayer<FargoSoulsPlayer>();
            //ignore money, hearts, mana stars
            if (player.GetToggleValue("IronM", false) && player.whoAmI == Main.myPlayer && p.IronEnchantActive && item.type != ItemID.CopperCoin && item.type != ItemID.SilverCoin && item.type != ItemID.GoldCoin && item.type != ItemID.PlatinumCoin && item.type != ItemID.HermesBoots && item.type != ItemID.CandyApple && item.type != ItemID.SoulCake &&
                item.type != ItemID.Star && item.type != ItemID.CandyCane && item.type != ItemID.SugarPlum && item.type != ItemID.Heart)
            {
                grabRange += (p.TerraForce || p.WizardEnchantActive) ? 1000 : 250;

                //half as effective on nebula bois
                if (!p.TerrariaSoul && (item.type == ItemID.NebulaPickup1 || item.type == ItemID.NebulaPickup2 || item.type == ItemID.NebulaPickup3))
                {
                    grabRange -= (p.TerraForce || p.WizardEnchantActive) ? 500 : 125;
                }
            }
        }

        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.Jammed && weapon.DamageType == DamageClass.Ranged)
                type = ProjectileID.ConfettiGun;
        }

        //        public override bool ConsumeItem(Item item, Player player)
        //        {
        //            FargoSoulsPlayer p = player.GetModPlayer<FargoSoulsPlayer>();

        //            if (item.makeNPC > 0 && (p.WoodForce || p.WizardEnchant) && Main.rand.NextBool())
        //            {
        //                return false;
        //            }

        //            if (p.BuilderMode && (item.createTile != -1 || item.createWall != -1) && item.type != ItemID.PlatinumCoin && item.type != ItemID.GoldCoin)
        //                return false;
        //            return true;
        //        }

        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.UniverseSoul || modPlayer.Eternity)
                knockback *= 2;
        }

        public override bool? CanAutoReuseItem(Item item, Player player)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();
            
            if (modPlayer.Berserked)
                return true;

            if (modPlayer.TribalCharm && item.type != ItemID.RodofDiscord && item.fishingPole == 0)
                return true;

            return base.CanAutoReuseItem(item, player);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.NoUsingItems)
                return false;

            //if (modPlayer.AdamantiteEnchantActive && modPlayer.AdamantiteCD == 0)
            //{
            //// ??? tm
            //}

            //dont use hotkeys in stasis
            if (player.HasBuff(ModContent.BuffType<GoldenStasis>()))
            {
                if (item.type == ItemID.RodofDiscord)
                    player.ClearBuff(ModContent.BuffType<GoldenStasis>());
                else
                    return false;
            }

            if (item.DamageType == DamageClass.Magic && player.GetModPlayer<FargoSoulsPlayer>().ReverseManaFlow)
            {
                int damage = (int)(item.mana / (1f - player.endurance) + player.statDefense);
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was destroyed by their own magic."), damage, 0);
                player.immune = false;
                player.immuneTime = 0;
            }

            if (modPlayer.BuilderMode && (item.createTile != -1 || item.createWall != -1) && item.type != ItemID.PlatinumCoin && item.type != ItemID.GoldCoin)
            {
                item.useTime = 1;
                item.useAnimation = 1;
            }

            if (item.damage > 0 && player.HasAmmo(item, true) && !(item.mana > 0 && player.statMana < item.mana) //non weapons and weapons with no ammo begone
                && item.type != ItemID.ExplosiveBunny && item.type != ItemID.Cannonball
                && item.useTime > 0 && item.createTile == -1 && item.createWall == -1 && item.ammo == AmmoID.None && item.hammer == 0 && item.pick == 0 && item.axe == 0)
            {
                modPlayer.TryAdditionalAttacks(item.damage, item.DamageType);
            }

            //            //critter attack timer
            //            if (modPlayer.WoodEnchant && player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed && item.makeNPC > 0)
            //            {
            //                if (modPlayer.CritterAttackTimer == 0)
            //                {
            //                    Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.Center);
            //                    float damageMultiplier = player.GetDamage(DamageClass.Summon);

            //                    int type = -1;
            //                    int damage = 0;
            //                    int attackCooldown = 0;

            //                    switch (item.type)
            //                    {
            //                        //case ItemID.Bunny:
            //                        //    type = ProjectileID.ExplosiveBunny;
            //                        //    damage = 10;
            //                        //    attackCooldown = 10;
            //                        //    break;

            //                        case ItemID.Bird:
            //                            type = ModContent.ProjectileType<BirdProj>();
            //                            damage = 15;
            //                            attackCooldown = 15;
            //                            break;

            //                        case ItemID.BlueJay:
            //                            type = ModContent.ProjectileType<BlueJayProj>();
            //                            damage = 10;
            //                            attackCooldown = 10;
            //                            break;

            //                        case ItemID.Cardinal:
            //                            type = ModContent.ProjectileType<CardinalProj>();
            //                            damage = 20;
            //                            attackCooldown = 20;
            //                            break;
            //                    }

            //                    if (type != -1)
            //                    {
            //                        Projectile.NewProjectile(player.Center, vel * 2f, type, damage, 2, player.whoAmI);
            //                        modPlayer.CritterAttackTimer = attackCooldown;
            //                    }


            //                }





            //                return false;
            //            }

            if (item.type == ItemID.RodofDiscord && player.chaosState)
            {
                player.GetModPlayer<FargoSoulsPlayer>().WasHurtBySomething = true; //with abom rebirth, die to chaos state
            }

            return true;
        }

        public override bool? UseItem(Item item, Player player)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (item.type == ItemID.RodofDiscord)
                player.ClearBuff(ModContent.BuffType<GoldenStasis>());

            return base.UseItem(item, player);
        }

        //        public override bool AltFunctionUse(Item item, Player player)
        //        {
        //            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

        //            if (modPlayer.WoodEnchant)
        //            {
        //                switch (item.type)
        //                {
        //                    case ItemID.Bunny:
        //                    case ItemID.Bird:
        //                    case ItemID.BlueJay:
        //                    case ItemID.Cardinal:
        //                        return true;

        //                }
        //            }



        //            return base.AltFunctionUse(item, player);
        //        }

        //        public override bool NewPreReforge(Item item)
        //        {
        //            /*if (Main.player[item.owner].GetModPlayer<FargoSoulsPlayer>().SecurityWallet)
        //            {
        //                switch(item.prefix)
        //                {
        //                    case PrefixID.Warding:  if (SoulConfig.Instance.walletToggles.Warding)  return false; break;
        //                    case PrefixID.Violent:  if (SoulConfig.Instance.walletToggles.Violent)  return false; break;
        //                    case PrefixID.Quick:    if (SoulConfig.Instance.walletToggles.Quick)    return false; break;
        //                    case PrefixID.Lucky:    if (SoulConfig.Instance.walletToggles.Lucky)    return false; break;
        //                    case PrefixID.Menacing: if (SoulConfig.Instance.walletToggles.Menacing) return false; break;
        //                    case PrefixID.Legendary:if (SoulConfig.Instance.walletToggles.Legendary)return false; break;
        //                    case PrefixID.Unreal:   if (SoulConfig.Instance.walletToggles.Unreal)   return false; break;
        //                    case PrefixID.Mythical: if (SoulConfig.Instance.walletToggles.Mythical) return false; break;
        //                    case PrefixID.Godly:    if (SoulConfig.Instance.walletToggles.Godly)    return false; break;
        //                    case PrefixID.Demonic:  if (SoulConfig.Instance.walletToggles.Demonic)  return false; break;
        //                    case PrefixID.Ruthless: if (SoulConfig.Instance.walletToggles.Ruthless) return false; break;
        //                    case PrefixID.Light:    if (SoulConfig.Instance.walletToggles.Light)    return false; break;
        //                    case PrefixID.Deadly:   if (SoulConfig.Instance.walletToggles.Deadly)   return false; break;
        //                    case PrefixID.Rapid:    if (SoulConfig.Instance.walletToggles.Rapid)    return false; break;
        //                    default: break;
        //                }
        //            }*/
        //            return true;
        //        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.Eternity)
                velocity *= 2;
            else if (modPlayer.UniverseSoul)
                velocity *= 1.5f;
        }

        //        public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
        //        {
        //            if (Main.LocalPlayer.GetModPlayer<FargoSoulsPlayer>().SecurityWallet)
        //                reforgePrice /= 2;
        //            return true;
        //        }

        //        //summon variants
        //        private static readonly int[] Summon = { ItemID.NimbusRod, ItemID.CrimsonRod, ItemID.BeeGun, ItemID.WaspGun, ItemID.PiranhaGun, ItemID.BatScepter };

        //        public override bool CanRightClick(Item item)
        //        {
        //            if (Array.IndexOf(Summon, item.type) > -1)
        //            {
        //                return true;
        //            }

        //            return base.CanRightClick(item);
        //        }

        //        public override void RightClick(Item item, Player player)
        //        {
        //            int newType = -1;

        //            if (Array.IndexOf(Summon, item.type) > -1)
        //            {
        //                newType = mod.ItemType(ItemID.GetUniqueKey(item.type).Replace("Terraria ", string.Empty) + "Summon");
        //            }

        //            if (newType != -1)
        //            {
        //                int num = Item.NewItem(player.getRect(), newType, prefixGiven: item.prefix);

        //                if (Main.netMode == NetmodeID.MultiplayerClient)
        //                {
        //                    NetMessage.SendData(MessageID.SyncItem, number: num, number2: 1f);
        //                }
        //            }
        //        }

        public override bool WingUpdate(int wings, Player player, bool inUse)
        {
            FargoSoulsPlayer modPlayer = player.GetModPlayer<FargoSoulsPlayer>();

            if (modPlayer.ChloroEnchantActive && player.GetToggleValue("Jungle") && inUse)
            {
                modPlayer.CanJungleJump = false;

                //spwn cloud
                if (modPlayer.JungleCD == 0)
                {
                    int dmg = (modPlayer.NatureForce || modPlayer.WizardEnchantActive) ? 150 : 75;
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)player.position.X, (int)player.position.Y, 62, 0.5f);
                    FargoSoulsUtil.XWay(10, player.GetSource_Accessory(modPlayer.ChloroEnchantItem), new Vector2(player.Center.X, player.Center.Y + (player.height / 2)), ProjectileID.SporeCloud, 3f, FargoSoulsUtil.HighestDamageTypeScaling(player, dmg), 0);

                    modPlayer.JungleCD = 8;
                }
            }

            return base.WingUpdate(wings, player, inUse);
        }

        public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            base.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);

            //Main.NewText($"vertical: {ascentWhenFalling} {ascentWhenRising} {maxCanAscendMultiplier} {maxAscentMultiplier} {constantAscend}");
        }

        public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
        {
            base.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);

            //Main.NewText($"horiz: {speed} {acceleration}");
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            /*if (Array.IndexOf(Summon, item.type) > -1)
            {
                TooltipLine helperLine = new TooltipLine(mod, "help", "Right click to convert");
                tooltips.Add(helperLine);
            }*/
        }
    }
}