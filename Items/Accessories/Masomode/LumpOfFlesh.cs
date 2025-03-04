using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Toggler;
using FargowiltasSouls.Items.Materials;
using FargowiltasSouls.Buffs.Masomode;

namespace FargowiltasSouls.Items.Accessories.Masomode
{
    //[AutoloadEquip(EquipType.Shield)]
    public class LumpOfFlesh : SoulsItem
    {
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lump of Flesh");
            Tooltip.SetDefault(@"Grants immunity to knockback, Anticoagulation, Blackout, Obstructed, Dazed, and Stunned
Increases minion damage by 16%
Increases your max number of minions by 2
Increases your max number of sentries by 2
Right Click to parry attacks with extremely tight timing
Defense and damage reduction drastically decreased while and shortly after guarding
The pungent eyeball charges energy to fire a laser as you attack
Enemies are less likely to target you
'It's growing'");
            DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "肉团");
            Tooltip.AddTranslation((int)GameCulture.CultureName.Chinese, @"'它在增长'
免疫致盲,阻塞和眩晕
增加16%召唤伤害,但略微减少防御
+2最大召唤栏
+2最大哨兵栏
当你攻击时,尖刻眼球会充能来发射激光
敌人不太可能以你为目标
地牢外的装甲和魔法骷髅敌意减小");

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Blackout] = true;
            player.buffImmune[BuffID.Obstructed] = true;
            player.buffImmune[BuffID.Dazed] = true;
            player.buffImmune[ModContent.BuffType<Stunned>()] = true;
            player.GetDamage(DamageClass.Summon) += 0.16f;
            player.aggro -= 400;
            player.GetModPlayer<FargoSoulsPlayer>().SkullCharm = true;
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
            player.maxMinions += 2;
            player.maxTurrets += 2;
            if (player.GetToggleValue("MasoPugent"))
            {
                player.buffImmune[ModContent.BuffType<Buffs.Minions.CrystalSkull>()] = true;
                player.AddBuff(ModContent.BuffType<Buffs.Minions.PungentEyeball>(), 5);
            }

            player.buffImmune[ModContent.BuffType<Anticoagulation>()] = true;
            player.noKnockback = true;
            if (player.GetToggleValue("DreadShellParry"))
                player.GetModPlayer<FargoSoulsPlayer>().DreadShellItem = Item;
        }

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ModContent.ItemType<PungentEyeball>())
            .AddIngredient(ModContent.ItemType<SkullCharm>())
            .AddIngredient(ModContent.ItemType<DreadShell>())
            .AddIngredient(ItemID.SpectreBar, 10)
            .AddIngredient(ModContent.ItemType<DeviatingEnergy>(), 10)

            .AddTile(TileID.MythrilAnvil)
            
            .Register();
        }
    }
}