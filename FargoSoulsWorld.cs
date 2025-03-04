using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace FargowiltasSouls
{
    public class FargoSoulsWorld : ModSystem
    {
        internal enum Downed //for champions specifically to keep them organized and synced, DO NOT rearrange
        {
            TimberChampion,
            TerraChampion,
            EarthChampion,
            NatureChampion,
            LifeChampion,
            ShadowChampion,
            SpiritChampion,
            WillChampion,
            CosmosChampion
        }

        public static bool SwarmActive => ModLoader.TryGetMod("Fargowiltas", out Mod fargo) && (bool)fargo.Call("SwarmActive");

        public static bool downedBetsy;

        //masomode
        public const int MaxCountPreHM = 560;
        public const int MaxCountHM = 240;

        public static bool ShouldBeEternityMode;
        public static bool EternityMode { get; private set; }
        public static bool MasochistModeReal;
        public static bool CanPlayMaso;
        public static bool downedFishronEX;
        public static bool downedDevi;
        public static bool downedAbom;
        public static bool downedMutant;
        public static bool AngryMutant;

        public static bool haveForcedAbomFromGoblins;
        public static int skipMutantP1;

        public static bool ReceivedTerraStorage;
        public static bool spawnedDevi;

        public static bool[] downedChampions = new bool[9];

        public override void OnWorldLoad()
        {
            downedBetsy = false;

            ShouldBeEternityMode = false;
            EternityMode = false;
            CanPlayMaso = false;
            MasochistModeReal = false;
            downedFishronEX = false;
            downedDevi = false;
            downedAbom = false;
            downedMutant = false;
            AngryMutant = false;

            haveForcedAbomFromGoblins = false;
            skipMutantP1 = 0;

            ReceivedTerraStorage = false;
            spawnedDevi = false;

            for (int i = 0; i < downedChampions.Length; i++)
                downedChampions[i] = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {

            List<string> downed = new List<string>();
            if (downedBetsy) downed.Add("betsy");
            if (ShouldBeEternityMode) downed.Add("shouldBeEternityMode");
            if (EternityMode) downed.Add("eternity");
            if (CanPlayMaso) downed.Add("CanPlayMaso");
            if (MasochistModeReal) downed.Add("getReal");
            if (downedFishronEX) downed.Add("downedFishronEX");
            if (downedDevi) downed.Add("downedDevi");
            if (downedAbom) downed.Add("downedAbom");
            if (downedMutant) downed.Add("downedMutant");
            if (AngryMutant) downed.Add("AngryMutant");
            if (haveForcedAbomFromGoblins) downed.Add("haveForcedAbomFromGoblins");
            if (ReceivedTerraStorage) downed.Add("ReceivedTerraStorage");
            if (spawnedDevi) downed.Add("spawnedDevi");

            for (int i = 0; i < downedChampions.Length; i++)
            {
                if (downedChampions[i])
                    downed.Add("downedChampion" + i.ToString());
            }

            tag.Add("downed", downed);
            tag.Add("mutantP1", skipMutantP1);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> downed = tag.GetList<string>("downed");
            downedBetsy = downed.Contains("betsy");
            ShouldBeEternityMode = downed.Contains("shouldBeEternityMode");
            EternityMode = downed.Contains("eternity") || downed.Contains("masochist");
            CanPlayMaso = downed.Contains("CanPlayMaso");
            MasochistModeReal = downed.Contains("getReal");
            downedFishronEX = downed.Contains("downedFishronEX");
            downedDevi = downed.Contains("downedDevi");
            downedAbom = downed.Contains("downedAbom");
            downedMutant = downed.Contains("downedMutant");
            AngryMutant = downed.Contains("AngryMutant");
            haveForcedAbomFromGoblins = downed.Contains("haveForcedAbomFromGoblins");
            ReceivedTerraStorage = downed.Contains("ReceivedTerraStorage");
            spawnedDevi = downed.Contains("spawnedDevi");

            for (int i = 0; i < downedChampions.Length; i++)
                downedChampions[i] = downed.Contains($"downedChampion{i}");

            if (tag.ContainsKey("mutantP1"))
                skipMutantP1 = tag.GetAsInt("mutantP1");
        }

        public override void NetReceive(BinaryReader reader)
        {
            skipMutantP1 = reader.ReadInt32();

            BitsByte flags = reader.ReadByte();
            downedBetsy = flags[0];
            EternityMode = flags[1];
            downedFishronEX = flags[2];
            downedDevi = flags[3];
            downedAbom = flags[4];
            downedMutant = flags[5];
            AngryMutant = flags[6];
            haveForcedAbomFromGoblins = flags[7];

            flags = reader.ReadByte();
            ReceivedTerraStorage = flags[0];
            spawnedDevi = flags[1];
            MasochistModeReal = flags[2];
            CanPlayMaso = flags[3];
            ShouldBeEternityMode = flags[4];

            flags = reader.ReadByte();
            downedChampions[0] = flags[0];
            downedChampions[1] = flags[1];
            downedChampions[2] = flags[2];
            downedChampions[3] = flags[3];
            downedChampions[4] = flags[4];
            downedChampions[5] = flags[5];
            downedChampions[6] = flags[6];
            downedChampions[7] = flags[7];

            flags = reader.ReadByte();
            downedChampions[8] = flags[0];
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(skipMutantP1);

            writer.Write(new BitsByte
            {
                [0] = downedBetsy,
                [1] = EternityMode,
                [2] = downedFishronEX,
                [3] = downedDevi,
                [4] = downedAbom,
                [5] = downedMutant,
                [6] = AngryMutant,
                [7] = haveForcedAbomFromGoblins
            });

            writer.Write(new BitsByte
            {
                [0] = ReceivedTerraStorage,
                [1] = spawnedDevi,
                [2] = MasochistModeReal,
                [3] = CanPlayMaso,
                [4] = ShouldBeEternityMode
            });

            writer.Write(new BitsByte
            {
                [0] = downedChampions[0],
                [1] = downedChampions[1],
                [2] = downedChampions[2],
                [3] = downedChampions[3],
                [4] = downedChampions[4],
                [5] = downedChampions[5],
                [6] = downedChampions[6],
                [7] = downedChampions[7]
            });

            writer.Write(new BitsByte
            {
                [0] = downedChampions[8]
            });
        }

        public override void PostUpdateWorld()
        {
            NPC.LunarShieldPowerExpert = 150;

            if (ShouldBeEternityMode)
            {
                if (EternityMode && !FargoSoulsUtil.WorldIsExpertOrHarder())
                {
                    EternityMode = false;
                    FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.EternityWrongDifficulty", new Color(175, 75, 255));
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                    if (!Main.dedServ)
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center, 0);
                }
                else if (!EternityMode && FargoSoulsUtil.WorldIsExpertOrHarder())
                {
                    EternityMode = true;
                    FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.EternityOn", new Color(175, 75, 255));
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                    if (!Main.dedServ)
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center, 0);
                }
            }
            else if (EternityMode)
            {
                EternityMode = false;
                FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.EternityOff", new Color(175, 75, 255));
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
                if (!Main.dedServ)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center, 0);
            }

            if (EternityMode)
            {
                NPC.LunarShieldPowerExpert = 50;

                if (!NPC.downedSlimeKing && !NPC.downedBoss1 && !Main.hardMode //pre boss, disable some events
                    && ModContent.TryFind("Fargowiltas", "Abominationn", out ModNPC abom) && !NPC.AnyNPCs(abom.Type))
                {
                    if (Main.raining || Sandstorm.Happening || Main.bloodMoon)
                    {
                        Main.raining = false;
                        Main.rainTime = 0;
                        Main.maxRaining = 0;
                        Sandstorm.Happening = false;
                        Sandstorm.TimeLeft = 0;
                        if (Main.bloodMoon)
                            FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.BloodMoonCancel", new Color(175, 75, 255));
                        Main.bloodMoon = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.WorldData);
                    }
                }

                if (!MasochistModeReal && EternityMode && FargoSoulsUtil.WorldIsMaster() && CanPlayMaso && !FargoSoulsUtil.AnyBossAlive())
                {
                    MasochistModeReal = true;
                    FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.MasochistOn", new Color(51, 255, 191, 0));
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.WorldData);
                    if (!Main.dedServ)
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center, 0);
                }
            }

            if (MasochistModeReal && !(EternityMode && FargoSoulsUtil.WorldIsMaster() && CanPlayMaso))
            {
                MasochistModeReal = false;
                FargoSoulsUtil.PrintText($"Mods.{Mod.Name}.Message.{Name}.MasochistOff", new Color(51, 255, 191, 0));
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
                if (!Main.dedServ)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center, 0);
            }

            //Main.NewText(BuilderMode);

            #region commented

            //right when day starts
            /*if(/*Main.time == 0 && Main.dayTime && !Main.eclipse && FargoSoulsWorld.masochistMode)
            {
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)player.position.X, (int)player.position.Y, 0, 1f, 0);

                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.eclipse = true;
                        //Main.NewText(Lang.misc[20], 50, 255, 130, false);
                    }
                    else
                    {
                        //NetMessage.SendData(61, -1, -1, "", player.whoAmI, -6f, 0f, 0f, 0, 0, 0);
                    }


            }*/

            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 361 && Main.CanStartInvasion(1, true))
            // {
            // this.itemTime = item.useTime;
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            // if (Main.invasionType == 0)
            // {
            // Main.invasionDelay = 0;
            // Main.StartInvasion(1);
            // }
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -1f, 0f, 0f, 0, 0, 0);
            // }
            // }
            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 602 && Main.CanStartInvasion(2, true))
            // {
            // this.itemTime = item.useTime;
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            // if (Main.invasionType == 0)
            // {
            // Main.invasionDelay = 0;
            // Main.StartInvasion(2);
            // }
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -2f, 0f, 0f, 0, 0, 0);
            // }
            // }
            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 1315 && Main.CanStartInvasion(3, true))
            // {
            // this.itemTime = item.useTime;
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            // if (Main.invasionType == 0)
            // {
            // Main.invasionDelay = 0;
            // Main.StartInvasion(3);
            // }
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -3f, 0f, 0f, 0, 0, 0);
            // }
            // }
            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 1844 && !Main.dayTime && !Main.pumpkinMoon && !Main.snowMoon && !DD2Event.Ongoing)
            // {
            // this.itemTime = item.useTime;
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            // Main.NewText(Lang.misc[31], 50, 255, 130, false);
            // Main.startPumpkinMoon();
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -4f, 0f, 0f, 0, 0, 0);
            // }
            // }

            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 3601 && NPC.downedGolemBoss && Main.hardMode && !NPC.AnyDanger() && !NPC.AnyoneNearCultists())
            // {
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // this.itemTime = item.useTime;
            // if (Main.netMode == NetmodeID.SinglePlayer)
            // {
            // WorldGen.StartImpendingDoom();
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -8f, 0f, 0f, 0, 0, 0);
            // }
            // }
            // if (this.itemTime == 0 && this.itemAnimation > 0 && item.type == 1958 && !Main.dayTime && !Main.pumpkinMoon && !Main.snowMoon && !DD2Event.Ongoing)
            // {
            // this.itemTime = item.useTime;
            // Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, (int)this.position.X, (int)this.position.Y, 0, 1f, 0);
            // if (Main.netMode != NetmodeID.MultiplayerClient)
            // {
            // Main.NewText(Lang.misc[34], 50, 255, 130, false);
            // Main.startSnowMoon();
            // }
            // else
            // {
            // NetMessage.SendData(61, -1, -1, "", this.whoAmI, -5f, 0f, 0f, 0, 0, 0);
            // }
            // }

            #endregion
        }

        public override void PostWorldGen()
        {
            /*WorldGen.PlaceTile(Main.spawnTileX - 1, Main.spawnTileY, TileID.GrayBrick, false, true);
            WorldGen.PlaceTile(Main.spawnTileX, Main.spawnTileY, TileID.GrayBrick, false, true);
            WorldGen.PlaceTile(Main.spawnTileX + 1, Main.spawnTileY, TileID.GrayBrick, false, true);
            Main.tile[Main.spawnTileX - 1, Main.spawnTileY].slope(0);
            Main.tile[Main.spawnTileX, Main.spawnTileY].slope(0);
            Main.tile[Main.spawnTileX + 1, Main.spawnTileY].slope(0);
            WorldGen.PlaceTile(Main.spawnTileX, Main.spawnTileY - 1, ModContent.Find<ModTile>("Fargowiltas", "RegalStatueSheet"), false, true);*/

            bool TryPlacingStatue(int baseCheckX, int baseCheckY)
            {
                List<int> legalBlocks = new List<int> { 
                    TileID.Stone, 
                    TileID.Grass,
                    TileID.Dirt,
                    TileID.SnowBlock,
                    TileID.IceBlock, 
                    TileID.ClayBlock,
                    TileID.Mud,
                    TileID.JungleGrass,
                    TileID.Sand
                };

                bool canPlaceStatueHere = true;
                for (int i = 0; i < 3; i++) //check no obstructing blocks
                    for (int j = 0; j < 4; j++)
                    {
                        Tile tile = Framing.GetTileSafely(baseCheckX + i, baseCheckY + j);
                        if (WorldGen.SolidOrSlopedTile(tile))
                        {
                            canPlaceStatueHere = false;
                            break;
                        }
                    }
                for (int i = 0; i < 3; i++) //check for solid foundation
                {
                    Tile tile = Framing.GetTileSafely(baseCheckX + i, baseCheckY + 4);
                    if (!WorldGen.SolidTile(tile) || !legalBlocks.Contains(tile.TileType))
                    {
                        canPlaceStatueHere = false;
                        break;
                    }
                }

                if (canPlaceStatueHere)
                {
                    for (int i = 0; i < 3; i++) //MAKE SURE nothing in the way
                        for (int j = 0; j < 4; j++)
                            WorldGen.KillTile(baseCheckX + i, baseCheckY + j);

                    WorldGen.PlaceTile(baseCheckX, baseCheckY + 4, TileID.GrayBrick, false, true);
                    WorldGen.PlaceTile(baseCheckX + 1, baseCheckY + 4, TileID.GrayBrick, false, true);
                    WorldGen.PlaceTile(baseCheckX + 2, baseCheckY + 4, TileID.GrayBrick, false, true);
                    Tile tile = Main.tile[baseCheckX, baseCheckY + 4]; tile.Slope = 0;
                    tile = Main.tile[baseCheckX + 1, baseCheckY + 4]; tile.Slope = 0;
                    tile = Main.tile[baseCheckX + 2, baseCheckY + 4]; tile.Slope = 0;
                    WorldGen.PlaceTile(baseCheckX + 1, baseCheckY + 3, ModContent.TileType<Tiles.MutantStatueGift>(), false, true);

                    return true;
                }

                return false;
            }

            int positionX = Main.spawnTileX - 1; //offset by dimensions of statue
            int positionY = Main.spawnTileY - 4;
            bool placed = false;
            for (int offsetX = -50; offsetX <= 50; offsetX++)
            {
                for (int offsetY = -30; offsetY <= 10; offsetY++)
                {
                    if (TryPlacingStatue(positionX + offsetX, positionY + offsetY))
                    {
                        placed = true;
                        break;
                    }
                }

                if (placed)
                    break;
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            base.UpdateUI(gameTime);

            FargowiltasSouls.UserInterfaceManager.UpdateUI(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            base.ModifyInterfaceLayers(layers);

            FargowiltasSouls.UserInterfaceManager.ModifyInterfaceLayers(layers);
        }
    }
}
