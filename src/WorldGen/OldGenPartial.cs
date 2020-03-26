﻿using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace dominions.world
{
    public abstract class OldGenPartial : ModStdWorldGen
    {
        internal ICoreServerAPI api;
        internal int worldheight;
        public int airBlockId = 0;

        internal abstract int chunkRange { get; }

        internal LCGRandom chunkRand;

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            chunksize = api.WorldManager.ChunkSize;

            api.Event.InitWorldGenerator(initWorldGen, "standard");
        }

        public virtual void initWorldGen()
        {
            LoadGlobalConfig(api);

            worldheight = api.WorldManager.MapSizeY;
            chunkRand = new LCGRandom(api.WorldManager.Seed);
        }


        protected virtual void GenChunkColumn(IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkGenParams = null)
        {
            for (int dx = -chunkRange; dx <= chunkRange; dx++)
            {
                for (int dz = -chunkRange; dz <= chunkRange; dz++)
                {
                    chunkRand.InitPositionSeed(chunkX + dx, chunkZ + dz);
                    GeneratePartial(chunks, chunkX, chunkZ, dx, dz);
                }
            }
        }



        public virtual void GeneratePartial(IServerChunk[] chunks, int chunkX, int chunkZ, int basePosX, int basePosZ)
        {

        }

    }
}