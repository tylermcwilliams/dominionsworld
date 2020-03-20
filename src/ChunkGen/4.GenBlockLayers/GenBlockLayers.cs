﻿using System;
using System.Collections.Generic;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods.NoObf;
using Vintagestory.ServerMods;

namespace dominions.world
{
    public class GenBlockLayers : ModStdWorldGen
    {
        private ICoreServerAPI api;

        List<int> BlockLayersIds = new List<int>();
        int[] layersUnderWater = new int[0];

        LCGRandom rnd;
        int mapheight;
        ClampedSimplexNoise grassDensity;
        ClampedSimplexNoise grassHeight;


        RockStrataVariant dummyRock;
        public BlockLayerConfig blockLayerConfig;

        SimplexNoise distort2dx;
        SimplexNoise distort2dz;


        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override double ExecuteOrder()
        {
            return 0.4;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            if (DoDecorationPass)
            {
                this.api.Event.InitWorldGenerator(InitWorldGen, "standard");
                this.api.Event.InitWorldGenerator(InitWorldGen, "superflat"); // Just the Init so that BlockSoil can grow grass
                this.api.Event.MapRegionGeneration(OnMapRegionGen, "standard");
                this.api.Event.ChunkColumnGeneration(this.OnChunkColumnGeneration, EnumWorldGenPass.Terrain, "standard");
            }

            dummyRock = new RockStrataVariant() { SoilpH = 6.5f, WeatheringFactor = 1f };

            distort2dx = new SimplexNoise(new double[] { 14, 9, 6, 3 }, new double[] { 1 / 100.0, 1 / 50.0, 1 / 25.0, 1 / 12.5 }, api.World.SeaLevel + 20980);
            distort2dz = new SimplexNoise(new double[] { 14, 9, 6, 3 }, new double[] { 1 / 100.0, 1 / 50.0, 1 / 25.0, 1 / 12.5 }, api.World.SeaLevel + 20981);
        }

        private void OnMapRegionGen(IMapRegion mapRegion, int regionX, int regionZ)
        {
            /*int noiseSize = api.WorldManager.RegionSize / TerraGenConfig.rockStrataScale;
            int pad = 2;

            mapRegion.RockStrata = new IntMap[strata.Variants.Length];
            for (int i = 0; i < strata.Variants.Length; i++)
            {
                IntMap intmap = new IntMap();
                mapRegion.RockStrata[i] = intmap;
                intmap.Data = strataNoises[i].GenLayerMax0(
                    regionX * noiseSize - pad,
                    regionZ * noiseSize - pad,
                    noiseSize + 2 * pad,
                    noiseSize + 2 * pad
                );

                intmap.Size = noiseSize + 2 * pad;
                intmap.TopLeftPadding = intmap.BottomRightPadding = pad;
            }*/
        }





        public void InitWorldGen()
        {
            LoadGlobalConfig(api);

            IAsset asset = api.Assets.Get("worldgen/rockstrata.json");
            RockStrataConfig rockstrata = asset.ToObject<RockStrataConfig>();

            asset = api.Assets.Get("worldgen/blocklayers.json");
            blockLayerConfig = asset.ToObject<BlockLayerConfig>();
            blockLayerConfig.ResolveBlockIds(api, rockstrata);

            rnd = new LCGRandom(api.WorldManager.Seed);
            grassDensity = new ClampedSimplexNoise(new double[] { 4 }, new double[] { 0.5 }, rnd.NextInt());
            grassHeight = new ClampedSimplexNoise(new double[] { 1.5 }, new double[] { 0.5 }, rnd.NextInt());

            mapheight = api.WorldManager.MapSizeY;
        }



        private void OnChunkColumnGeneration(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams = null)
        {
            rnd.InitPositionSeed(chunkX, chunkZ);

            IntMap forestMap = chunks[0].MapChunk.MapRegion.ForestMap;
            IntMap climateMap = chunks[0].MapChunk.MapRegion.ClimateMap;
            IntMap beachMap = chunks[0].MapChunk.MapRegion.BeachMap;

            ushort[] heightMap = chunks[0].MapChunk.RainHeightMap;

            int regionChunkSize = api.WorldManager.RegionSize / chunksize;

            //fetch the region coordinates
            int rdx = chunkX % regionChunkSize;
            int rdz = chunkZ % regionChunkSize;

            // Amount of data points per chunk
            float climateStep = (float)climateMap.InnerSize / regionChunkSize;
            float forestStep = (float)forestMap.InnerSize / regionChunkSize;
            float beachStep = (float)beachMap.InnerSize / regionChunkSize;

            // Retrieves the map data on the chunk edges
            int forestUpLeft = forestMap.GetUnpaddedInt((int)(rdx * forestStep), (int)(rdz * forestStep));
            int forestUpRight = forestMap.GetUnpaddedInt((int)(rdx * forestStep + forestStep), (int)(rdz * forestStep));
            int forestBotLeft = forestMap.GetUnpaddedInt((int)(rdx * forestStep), (int)(rdz * forestStep + forestStep));
            int forestBotRight = forestMap.GetUnpaddedInt((int)(rdx * forestStep + forestStep), (int)(rdz * forestStep + forestStep));

            int beachUpLeft = beachMap.GetUnpaddedInt((int)(rdx * beachStep), (int)(rdz * beachStep));
            int beachUpRight = beachMap.GetUnpaddedInt((int)(rdx * beachStep + beachStep), (int)(rdz * beachStep));
            int beachBotLeft = beachMap.GetUnpaddedInt((int)(rdx * beachStep), (int)(rdz * beachStep + beachStep));
            int beachBotRight = beachMap.GetUnpaddedInt((int)(rdx * beachStep + beachStep), (int)(rdz * beachStep + beachStep));


            // increasing x -> left to right  
            // increasing z -> top to bottom

            float transitionSize = blockLayerConfig.blockLayerTransitionSize;

            for (int x = 0; x < chunksize; x++)
            {
                for (int z = 0; z < chunksize; z++)
                {
                    // Some weird randomnes stuff to hide fundamental bugs in the climate transition system :D T_T   (maybe not bugs but just fundamental shortcomings of using lerp on a very low resolution map)
                    float distx = (float)distort2dx.Noise(chunkX * chunksize + x, chunkZ * chunksize + z);
                    float distz = (float)distort2dz.Noise(chunkX * chunksize + x, chunkZ * chunksize + z);

                    double posRand = (double)GameMath.MurmurHash3(x + chunkX * chunksize, 1, z + chunkZ * chunksize) / int.MaxValue;
                    double transitionRand = (posRand + 1) * transitionSize;

                    int posY = heightMap[z * chunksize + x];

                    int climate = climateMap.GetUnpaddedColorLerped(
                        Math.Abs(rdx * climateStep + climateStep * (float)(x + distx) / chunksize),
                        Math.Abs(rdz * climateStep + climateStep * (float)(z + distz) / chunksize)
                    );

                    int tempUnscaled = (climate >> 16) & 0xff;

                    // ===
                    float pz = (rdz * climateStep + climateStep * (float)(z + distz) / chunksize);
                    float px = (rdx * climateStep + climateStep * (float)(x + distx) / chunksize);
                    // ===
                    int iz = (int)pz;
                    int ix = (int)px;

                    if (tempUnscaled < 200)
                    {
                        int colorTopLeft = climateMap.Data[(iz + climateMap.TopLeftPadding) * climateMap.Size + ix + climateMap.TopLeftPadding];
                        int colorTopRight = climateMap.Data[(iz + climateMap.TopLeftPadding) * climateMap.Size + ix + 1 + climateMap.TopLeftPadding];
                        int colorBottomLeft = climateMap.Data[(iz + 1 + climateMap.TopLeftPadding) * climateMap.Size + ix + climateMap.TopLeftPadding];
                        int colorBottomRight = climateMap.Data[(iz + 1 + climateMap.TopLeftPadding) * climateMap.Size + ix + 1 + climateMap.TopLeftPadding];


                        float lx = px - ix;
                        float lz = pz - iz;

                        int top = GameMath.LerpRgbColor(lx, colorTopLeft, colorTopRight);
                        int bottom = GameMath.LerpRgbColor(lx, colorBottomLeft, colorBottomRight);

                        int both = GameMath.LerpRgbColor(lz, top, bottom);



                        api.World.Logger.Notification("inner size " + climateMap.InnerSize.ToString());

                        api.World.Logger.Notification("topleft pad " + climateMap.TopLeftPadding.ToString());
                        api.World.Logger.Notification("botright" + climateMap.BottomRightPadding.ToString());

                        api.World.Logger.Notification("iz" + iz.ToString());
                        api.World.Logger.Notification("ix" + ix.ToString());

                        api.World.Logger.Notification("lz" + lz.ToString());
                        api.World.Logger.Notification("lx" + lx.ToString());



                        api.World.Logger.Notification("colorTopLeft " + colorTopLeft.ToString() + " _index: " + ((iz + climateMap.TopLeftPadding) * climateMap.Size + ix + climateMap.TopLeftPadding).ToString());
                        api.World.Logger.Notification("colorTopRight " + colorTopRight.ToString() + " _index: " + ((iz + climateMap.TopLeftPadding) * climateMap.Size + ix + 1 + climateMap.TopLeftPadding).ToString());
                        api.World.Logger.Notification("colorBottomLeft " + colorBottomLeft.ToString() + " _index: " + ((iz + 1 + climateMap.TopLeftPadding) * climateMap.Size + ix + climateMap.TopLeftPadding).ToString());
                        api.World.Logger.Notification("colorBottomRight " + colorBottomRight.ToString() + " _index: " + ((iz + 1 + climateMap.TopLeftPadding) * climateMap.Size + ix + 1 + climateMap.TopLeftPadding).ToString());

                        api.World.Logger.Notification("top " + top.ToString());
                        api.World.Logger.Notification("bottom " + bottom.ToString());

                        api.World.Logger.Notification("both " + both.ToString());

                    }

                    int rnd = (int)(distx / 5);
                    float temp = TerraGenConfig.GetScaledAdjustedTemperatureFloat(tempUnscaled, posY - TerraGenConfig.seaLevel + rnd);
                    float rainRel = TerraGenConfig.GetRainFall((climate >> 8) & 0xff, posY + rnd) / 255f;
                    float forestRel = GameMath.BiLerp(forestUpLeft, forestUpRight, forestBotLeft, forestBotRight, (float)x / chunksize, (float)z / chunksize) / 255f;
                    float beachRel = GameMath.BiLerp(beachUpLeft, beachUpRight, beachBotLeft, beachBotRight, (float)x / chunksize, (float)z / chunksize) / 255f;

                    //api.World.Logger.Notification(x.ToString() + " " + z.ToString() + " temp: " + tempUnscaled.ToString() + " distx" + distx.ToString() + " distz" + distz.ToString());

                    int prevY = posY;

                    posY = PutLayers(transitionRand, x, posY, z, chunks, rainRel, temp, tempUnscaled, heightMap);
                    int blockID = chunks[0].MapChunk.TopRockIdMap[z * chunksize + x];

                    GenBeach(x, prevY, z, chunks, rainRel, temp, beachRel, blockID);
                    PlaceTallGrass(x, prevY, z, chunks, rainRel, temp, forestRel);


                    // Try again to put layers if above sealevel and we found over 10 air blocks
                    int foundAir = 0;
                    while (posY >= TerraGenConfig.seaLevel - 1)
                    {
                        int chunkY = posY / chunksize;
                        int lY = posY % chunksize;
                        int index3d = (chunksize * lY + z) * chunksize + x;
                        int blockId = chunks[chunkY].Blocks[index3d];

                        if (blockId == 0)
                        {
                            foundAir++;
                        }
                        else
                        {
                            if (foundAir >= 8)
                            {
                                temp = TerraGenConfig.GetScaledAdjustedTemperatureFloat(tempUnscaled, posY - TerraGenConfig.seaLevel);
                                rainRel = TerraGenConfig.GetRainFall((climate >> 8) & 0xff, posY) / 255f;

                                //PutLayers(transitionRand, x, posY, z, chunks, rainRel, temp, tempUnscaled, null);
                                break;
                            }
                            else
                            {
                                foundAir = 0;
                            }

                        }

                        posY--;
                    }



                }
            }
        }


        private int PutLayers(double posRand, int x, int posY, int z, IServerChunk[] chunks, float rainRel, float temp, int unscaledTemp, ushort[] heightMap)
        {
            int i = 0;
            int j = 0;

            bool underWater = false;
            bool first = true;
            int startPosY = posY;

            while (posY > 0)
            {
                int chunkY = posY / chunksize;
                int lY = posY % chunksize;
                int index3d = (chunksize * lY + z) * chunksize + x;
                int blockId = chunks[chunkY].Blocks[index3d];

                posY--;

                if (blockId == GlobalConfig.waterBlockId)
                {
                    underWater = true;
                    continue;
                }

                if (blockId != 0)
                {
                    if (heightMap != null && first)
                    {
                        chunks[0].MapChunk.TopRockIdMap[z * chunksize + x] = blockId;

                        LoadBlockLayers(posRand, rainRel, temp, unscaledTemp, startPosY, blockId);
                        first = false;

                        if (!underWater) heightMap[z * chunksize + x] = (ushort)(posY + 1);
                    }

                    if (i >= BlockLayersIds.Count || (underWater && j >= layersUnderWater.Length))
                    {
                        return posY;
                    }


                    chunks[chunkY].Blocks[index3d] = underWater ? layersUnderWater[j++] : BlockLayersIds[i++];

                }
                else
                {
                    if (i > 0 || j > 0) return posY;
                }
            }

            return posY;
        }




        private void GenBeach(int x, int posY, int z, IServerChunk[] chunks, float rainRel, float temp, float beachRel, int topRockId)
        {
            int sandBlockId = blockLayerConfig.BeachLayer.BlockId;

            if (blockLayerConfig.BeachLayer.BlockIdMapping != null && !blockLayerConfig.BeachLayer.BlockIdMapping.TryGetValue(topRockId, out sandBlockId))
            {
                return;
            }

            if (posY == TerraGenConfig.seaLevel - 1 && beachRel > 0.5 && chunks[posY / chunksize].Blocks[(chunksize * (posY % chunksize) + z) * chunksize + x] != GlobalConfig.waterBlockId)
            {
                chunks[posY / chunksize].Blocks[(chunksize * (posY % chunksize) + z) * chunksize + x] = sandBlockId;
            }
        }

        void PlaceTallGrass(int x, int posY, int z, IServerChunk[] chunks, float rainRel, float temp, float forestRel)
        {
            double rndVal = blockLayerConfig.Tallgrass.RndWeight * rnd.NextDouble() + blockLayerConfig.Tallgrass.PerlinWeight * grassDensity.Noise(x, z, -0.5f);

            if (rndVal <= GameMath.Clamp(forestRel, 0.05, 0.99) || posY >= mapheight - 1 || posY < 1) return;

            int blockId = chunks[posY / chunksize].Blocks[(chunksize * (posY % chunksize) + z) * chunksize + x];

            if (api.World.Blocks[blockId].Fertility <= rnd.NextInt(100)) return;

            double gheight = Math.Max(0, grassHeight.Noise(x, z) * blockLayerConfig.Tallgrass.BlockCodeByMin.Length - 1);
            int start = (int)gheight + (rnd.NextDouble() < gheight ? 1 : 0);

            for (int i = start; i < blockLayerConfig.Tallgrass.BlockCodeByMin.Length; i++)
            {
                TallGrassBlockCodeByMin bcbymin = blockLayerConfig.Tallgrass.BlockCodeByMin[i];

                if (forestRel <= bcbymin.MaxForest && rainRel >= bcbymin.MinRain && temp >= bcbymin.MinTemp)
                {
                    chunks[(posY + 1) / chunksize].Blocks[(chunksize * ((posY + 1) % chunksize) + z) * chunksize + x] = bcbymin.BlockId;
                    return;
                }
            }
        }


        private void LoadBlockLayers(double posRand, float rainRel, float temperature, int unscaledTemp, int posY, int firstBlockId)
        {
            float heightRel = ((float)posY - TerraGenConfig.seaLevel) / ((float)api.WorldManager.MapSizeY - TerraGenConfig.seaLevel);
            float fertilityRel = TerraGenConfig.GetFertilityFromUnscaledTemp((int)(rainRel * 255), unscaledTemp, heightRel) / 255f;



            float depthf = TerraGenConfig.SoilThickness(rainRel, temperature, posY - TerraGenConfig.seaLevel, 1f);
            int depth = (int)depthf;
            depth += (int)((depthf - depth) * rnd.NextDouble());

            BlockLayersIds.Clear();
            for (int j = 0; j < blockLayerConfig.Blocklayers.Length; j++)
            {
                BlockLayer bl = blockLayerConfig.Blocklayers[j];

                float tempDist = Math.Abs(temperature - GameMath.Clamp(temperature, bl.MinTemp, bl.MaxTemp));
                float rainDist = Math.Abs(rainRel - GameMath.Clamp(rainRel, bl.MinRain, bl.MaxRain));
                float fertDist = Math.Abs(fertilityRel - GameMath.Clamp(fertilityRel, bl.MinFertility, bl.MaxFertility));
                float yDist = Math.Abs((float)posY / mapheight - GameMath.Min((float)posY / mapheight, bl.MaxY));


                if (tempDist + rainDist + fertDist + yDist <= posRand)
                {
                    int blockId = bl.GetBlockId(posRand, temperature, rainRel, fertilityRel, firstBlockId);
                    if (blockId != 0)
                    {
                        BlockLayersIds.Add(blockId);

                        // Would be correct, but doesn't seem to cause noticable problems
                        // so lets not add it for faster chunk gen
                        posY--;
                        temperature = TerraGenConfig.GetScaledAdjustedTemperatureFloat(unscaledTemp, posY - TerraGenConfig.seaLevel);
                        //  rainRel = TerraGenConfig.GetRainFall(unscaledRain, posY) / 255f;
                        heightRel = ((float)posY - TerraGenConfig.seaLevel) / ((float)api.WorldManager.MapSizeY - TerraGenConfig.seaLevel);
                        fertilityRel = TerraGenConfig.GetFertilityFromUnscaledTemp((int)(rainRel * 255), unscaledTemp, heightRel) / 255f;
                    }
                }

                if (BlockLayersIds.Count >= depth) break;
            }


            layersUnderWater = null;
            for (int j = 0; j < blockLayerConfig.LakeBedLayer.BlockCodeByMin.Length; j++)
            {
                LakeBedBlockCodeByMin lbbc = blockLayerConfig.LakeBedLayer.BlockCodeByMin[j];
                if (lbbc.Suitable(temperature, rainRel, (float)posY / api.WorldManager.MapSizeY, rnd))
                {
                    layersUnderWater = new int[] { lbbc.GetBlockForMotherRock(firstBlockId) };
                    break;
                }
            }
            if (layersUnderWater == null) layersUnderWater = new int[0];

        }


    }
}
