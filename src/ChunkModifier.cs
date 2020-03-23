using dominions.world;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace landtester.src
{
    class ChunkModifier : ModStdWorldGen
    {
        ICoreServerAPI api;

        int chunkSize;

        int glacierIceId;

        List<int> ignore;

        public enum Modifiers
        {
            None = -1,
            Glacial = 0
        }

        int[][] modifiers = new int[][]
        {
            new int[] { 0 } // Glacial
        };

        public Modifiers GetLandformMod(int chunkLandformIndex)
        {
            // Tests if landform has a modifier specified.
            // Returns enum of modifier

            for (int m = 0; m < modifiers.Length; m++)
            {
                int[] modifier = modifiers[m];
                for (int l = 0; l < modifier.Length; l++)
                {
                    int modLandformIndex = modifier[l];
                    if (modLandformIndex == chunkLandformIndex)
                    {
                        return (Modifiers)m;
                    }
                }
            }

            return Modifiers.None;
        }

        public override double ExecuteOrder() 
        {
            // mod exec order
            return 0.41;

            // 0.41 overrides all blocklayers
            // the way this will function is that
            // i will manage adding my own blocklayer

            // i also need to stop overriding ores
            // unless they are below a certain level
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            LoadGlobalConfig(api);

            // This should happen automatically.
            WorldMap.TryGet(api);

            chunkSize = api.WorldManager.ChunkSize;
            glacierIceId = api.WorldManager.GetBlockId(new AssetLocation("snowblock"));

            ignore = new List<int>();
            ignore.Add(api.WorldManager.GetBlockId(new AssetLocation("air")));
            ignore.Add(GlobalConfig.waterBlockId);


            api.Event.ChunkColumnGeneration(ModifyChunkCol, EnumWorldGenPass.Terrain, "standard");
        }

        //step 1..3
        //1 = regiongen
        //2 = chunkgen
        //3 = columngen

        //Check chunk gen params
        private void ModifyChunkCol(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams)
        {
            int landformIndex = WorldMap.GetBiomeFromPixel(WorldMap.map.GetPixel(chunkX, chunkZ));
            Modifiers mod = GetLandformMod(landformIndex);

            if (mod == Modifiers.None)
            {
                // No modifier
                return;
            }

            for (int bColX = 0; bColX < chunkSize; bColX++)
            {
                for (int bColZ = 0; bColZ < chunkSize; bColZ++)
                {
                    switch (mod)
                    {
                        case Modifiers.Glacial:
                            ModColToGlacial(chunks, chunkX, chunkZ, bColX, bColZ);
                            break;
                    }
                }
            }
        }

        private void ModColToGlacial(IServerChunk[] chunks, int chunkX, int chunkZ, int x, int z)
        {
            

            // skip arbitrary amount of chunks
            for (int chunkY = 2; chunkY < chunks.Length; chunkY++)
            {
                IServerChunk chunk = chunks[chunkY];

                // inY is the Y relative to the CHUNK, not the floor-to-ceiling chunk collumn.
                // chunks are 32x32x32
                for (int inY = 0; inY < chunkSize; inY++)
                {
                    //int y = chunkY * chunkSize + inY;

                    int blockPosInChunk = (inY * chunkSize + z) * chunkSize + x;


                    int currentBlock = chunk.Blocks[blockPosInChunk];
                    bool shouldIgnore = false;
                    
                    foreach (int i in ignore)
                    {
                        if (i == currentBlock)
                        {
                            shouldIgnore = true;
                            break;
                        }
                    }

                    if (!shouldIgnore)
                    {
                        chunk.Blocks[blockPosInChunk] = glacierIceId;
                    }
                } 

            }
        }



    }

}
