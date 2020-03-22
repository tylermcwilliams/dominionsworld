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

        public override double ExecuteOrder()
        {
            // mod exec order
            return 0.9;

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

            chunkSize = api.WorldManager.ChunkSize;
            glacierIceId = api.WorldManager.GetBlockId(new AssetLocation("snowblock"));

            ignore = new List<int>();
            ignore.Add(api.WorldManager.GetBlockId(new AssetLocation("air")));
            ignore.Add(GlobalConfig.waterBlockId);

            

            //api.Event.ChunkColumnGeneration(ModifyChunkCol, EnumWorldGenPass.Terrain, "standard");
        }

        //step 1..3
        //1 = regiongen
        //2 = chunkgen
        //3 = columngen

        //Check chunk gen params
        private void ModifyChunkCol(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams)
        {
            for (int bColX = 0; bColX < chunkSize; bColX++)
            {
                for (int bColZ = 0; bColZ < chunkSize; bColZ++)
                {
                    ModifyBlockCol(chunks, chunkX, chunkZ, bColX, bColZ);
                }
            }
        }

        private void ModifyBlockCol(IServerChunk[] chunks, int chunkX, int chunkZ, int x, int z)
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
