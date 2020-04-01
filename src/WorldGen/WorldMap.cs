using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace dominions.world
{
    public class WorldMap
    {
        public ICoreServerAPI api;
        public Bitmap landformMap;
        public Bitmap climateMap;

        public static Color outOfBoundsLandform = Color.FromArgb(0, 0, 0, 0);
        public static Color outOfBoundsClimate = Color.FromArgb(0, 0, 0, 0);

        public static int chunkOffset = 8376;

        public WorldMap(ICoreServerAPI api)
        {
            this.api = api;
            string mapFolder = api.GetOrCreateDataPath("Maps");

            string landformPath = mapFolder + @"/landforms.png";
            if (File.Exists(landformPath))
            {
                this.landformMap = new Bitmap(landformPath);
            }

            string climatePath = mapFolder + @"/climate.png";
            if (File.Exists(climatePath))
            {
                this.climateMap = new Bitmap(climatePath);
            }

            api.RegisterCommand("getpixelinfo", "gets pixel values on maps", "", (IServerPlayer player, int i, CmdArgs args) =>
            {
                int chunkx = player.Entity.ServerPos.AsBlockPos.X / 32;
                int chunkz = player.Entity.ServerPos.AsBlockPos.Z / 32;

                player.SendMessage(0,
                    "landform:" +
                    ChunkToPixel(chunkx, chunkz, landformMap).R.ToString() +
                    " temp:" +
                    ChunkToPixel(chunkx, chunkz, climateMap).R.ToString() +
                    " humid:" +
                    ChunkToPixel(chunkx, chunkz, landformMap).R.ToString(),
                EnumChatType.Notification);
            });
        }

        public int[] GenClimateLayer(int regionX, int regionZ, int sizeX, int sizeZ)
        {
            int pad = 2;
            int[] outData = new int[sizeX * sizeZ];

            for (int y = 0; y < sizeZ; ++y)
            {
                for (int x = 0; x < sizeX; ++x)
                {
                    int pixelX = ((regionX * 16) - chunkOffset) + (x - pad);
                    int pixelZ = ((regionZ * 16) - chunkOffset) + (y - pad);
                    if (pixelX >= 2000 || pixelX < 0 || pixelZ >= 2000 || pixelZ < 0)
                    {
                        outData[y * sizeX + x] = ColorToDecimal(outOfBoundsClimate);
                    }
                    else
                    {
                        outData[y * sizeX + x] = ColorToDecimal(climateMap.GetPixel(pixelX, pixelZ));
                    }
                }
            }

            return outData;
        }

        public int[] GenLandformLayer(int regionX, int regionZ, int sizeX, int sizeZ)
        {
            int pad = 2;
            int[] result = new int[sizeX * sizeZ];

            for (int x = 0; x < (sizeX / 2); x++)
            {
                for (int z = 0; z < (sizeZ / 2); z++)
                {
                    int offset = (z * sizeX * 2) + (x * 2);

                    int pixelX = ((regionX * 16) - chunkOffset) + (x - pad);
                    int pixelZ = ((regionZ * 16) - chunkOffset) + (z - pad);


                    if (pixelX >= 2000 || pixelX < 0 || pixelZ >= 2000 || pixelZ < 0)
                    {
                        result[offset] = outOfBoundsLandform.R;
                        result[offset + 1] = outOfBoundsLandform.R;

                        result[offset + 40] = outOfBoundsLandform.R;
                        result[offset + 41] = outOfBoundsLandform.R;
                    }
                    else
                    {
                        result[offset] = landformMap.GetPixel(pixelX, pixelZ).R;
                        result[offset + 1] = landformMap.GetPixel(pixelX, pixelZ).R;

                        result[offset + 40] = landformMap.GetPixel(pixelX, pixelZ).R;
                        result[offset + 41] = landformMap.GetPixel(pixelX, pixelZ).R;
                    }
                }
            }

            return result;

        }

        public static Color ChunkToPixel(int chunkx, int chunkz, Bitmap map)
        {
            if (chunkx < chunkOffset || chunkz < chunkOffset)
            {
                // out of bounds
                return Color.Black;
            }

            int pixelX = (chunkx - chunkOffset);
            int pixelZ = (chunkz - chunkOffset);

            return map.GetPixel(pixelX, pixelZ);
        }

        public static int ColorToDecimal(Color color)
        {
            int result = (color.R << 16) + (color.G << 8) + color.B;

            return result;
        }
    }


}
