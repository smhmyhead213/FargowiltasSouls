﻿using FargowiltasSouls.Items.Accessories.Masomode;
using FargowiltasSouls.Items.Armor;
using FargowiltasSouls.Items.Materials;
using Terraria.ModLoader;

namespace FargowiltasSouls.Toggler.Content
{
    public class MasoToggles : ToggleCollection
    {
        public override string Mod => "Terraria";
        public override string SortCatagory => "Maso";
        public override int Priority => 2;

        public int MasoHeader2 = ModContent.ItemType<DeviatingEnergy>();
        public string MasoAeolus;
        public string MasoAeolusFlower;
        public string MasoIcon;
        public string MasoIconDrops;
        public string MasoGraze;
        public string MasoGrazeRing;
        public string MasoDevianttHearts;
        public string DreadShellParry;
        public string PrecisionSealHurtbox;
        public string MasoFishron;

        public int SupremeFairyHeader = ModContent.ItemType<SupremeDeathbringerFairy>();
        public string MasoSlime;
        public string SlimeFalling;
        public string MasoEye;
        public string MasoHoney;
        public string MasoSkele;

        public int BionomicHeader = ModContent.ItemType<BionomicCluster>();
        public string MasoConcoction;
        public string MasoCarrot;
        public string MasoRainbow;
        public string MasoFrigid;
        public string MasoNymph;
        public string MasoSqueak;
        public string MasoPouch;
        public string MasoClipped;
        public string TribalCharm;

        public int DubiousHeader = ModContent.ItemType<DubiousCircuitry>();
        public string MasoLightning;
        public string MasoProbe;

        public int PureHeartHeader = ModContent.ItemType<PureHeart>();
        public string MasoEater;
        public string MasoBrain;

        public int LumpofFleshHeader = ModContent.ItemType<LumpOfFlesh>();
        public string MasoPugent;

        public int ChaliceHeader = ModContent.ItemType<ChaliceoftheMoon>();
        public string MasoCultist;
        public string MasoPlant;
        public string MasoGolem;
        public string MasoBoulder;
        public string MasoCelest;
        public string MasoVision;

        public int HeartHeader = ModContent.ItemType<HeartoftheMasochist>();
        public string MasoPump;
        public string IceQueensCrown;
        public string MasoFlocko;
        public string MasoUfo;
        public string MasoGrav;
        public string MasoGrav2;
        public string MasoTrueEye;

        public int MutantArmorHeader = ModContent.ItemType<MutantBody>();
        public string MasoAbom;
        public string MasoRing;
        public string MasoReviveDeathray;
    }
}
