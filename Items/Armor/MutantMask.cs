﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using FargowiltasSouls.Buffs.Minions;

namespace FargowiltasSouls.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class MutantMask : SoulsItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("True Mutant Head");
            Tooltip.SetDefault(@"50% increased damage and 20% increased critical strike chance
Increases max number of minions and sentries by 10
25% reduced mana usage
25% chance not to consume ammo");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "真·突变之颅");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, @"增加50%伤害和20%暴击率
增加10最大召唤栏和哨兵栏
减少25%法力消耗
25%概率不消耗弹药");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 50);
            Item.defense = 50;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.50f;
            player.GetCritChance(DamageClass.Generic) += 20;

            player.maxMinions += 10;
            player.maxTurrets += 10;

            player.manaCost -= 0.25f;
            player.ammoCost75 = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<MutantBody>() && legs.type == ModContent.ItemType<MutantPants>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = @"Phantasmal Spheres shoot deathrays at nearby enemies
Abominationn's Visage fights alongside you
Your attacks inflict God Eater and Hellfire
You erupt into a massive deathray whenever revived
20% increased weapon use speed";

            player.AddBuff(ModContent.BuffType<MutantPower>(), 2);

            player.GetModPlayer<FargoSoulsPlayer>().MutantSetBonusItem = Item;
            player.GetModPlayer<FargoSoulsPlayer>().GodEaterImbue = true;
            player.GetModPlayer<FargoSoulsPlayer>().AttackSpeed += .2f;
        }

        public override void SafeModifyTooltips(List<TooltipLine> list)
        {
            foreach (TooltipLine line2 in list)
            {
                if (line2.Mod == "Terraria" && line2.Name == "ItemName")
                {
                    line2.OverrideColor = new Color(Main.DiscoR, 51, 255 - (int)(Main.DiscoR * 0.4));
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.Find<ModItem>("Fargowiltas", "MutantMask"))
            .AddIngredient(null, "AbomEnergy", 10)
            .AddIngredient(null, "EternalEnergy", 10)
            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))
            
            .Register();
        }
    }
}