using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace dominions.world
{
    public static class WorldMap
    {
        public static Bitmap map = null;
        public static Bitmap climateMap = null;

        public static int[] GenClimateLayer(int regionX, int regionZ, int sizeX, int sizeZ, ICoreServerAPI api)
        {
            int[] outData = new int[sizeX * sizeZ];

            int pad = 2;

            Bitmap climateMap = WorldMap.TryGetClimate(api);

            for (int y = 0; y < sizeZ; ++y)
            {
                for (int x = 0; x < sizeX; ++x)
                {
                    // pixel
                    int pixelX = (((regionX * 512) / 32) - 8376) + (x - pad);
                    int pixelZ = (((regionZ * 512) / 32) - 8376) + (y - pad);
                    if (pixelX >= 2000 || pixelX < 0 || pixelZ >= 2000 || pixelZ < 0)
                    {
                        outData[y * sizeX + x] = 16711680;
                    }
                    else
                    {
                        int temperature = climateMap.GetPixel(pixelX, pixelZ).R;
                        int rain = climateMap.GetPixel(pixelX, pixelZ).G;

                        int climate = (temperature << 16) + (rain << 8);

                        outData[y * sizeX + x] = climate;
                    }
                }
            }

            return outData;
        }

        public static int[] GenLandformLayer(int regionX, int regionZ, int sizeX, int sizeZ, ICoreServerAPI api)
        {
            int[] result = new int[sizeX * sizeZ];

            Bitmap worldMap = WorldMap.TryGet(api);

            for (int x = 0; x < (sizeX / 2); x++)
            {
                for (int z = 0; z < (sizeZ / 2); z++)
                {
                    int offset = (z * sizeX * 2) + (x * 2);

                    // pixel
                    int pixelX = (((regionX * 512) / 32) - 8376) + (x - 2);
                    int pixelZ = (((regionZ * 512) / 32) - 8376) + (z - 2);


                    if (pixelX >= 2000 || pixelX < 0 || pixelZ >= 2000 || pixelZ < 0)
                    {

                        result[offset] = 1;
                        result[offset + 1] = 1;

                        result[offset + 40] = 1;
                        result[offset + 41] = 1;
                    }
                    else
                    {
                        result[offset] = WorldMap.GetBiomeFromPixel(worldMap.GetPixel(pixelX, pixelZ));
                        result[offset + 1] = WorldMap.GetBiomeFromPixel(worldMap.GetPixel(pixelX, pixelZ));

                        result[offset + 40] = WorldMap.GetBiomeFromPixel(worldMap.GetPixel(pixelX, pixelZ));
                        result[offset + 41] = WorldMap.GetBiomeFromPixel(worldMap.GetPixel(pixelX, pixelZ));
                    }
                }
            }

            return result;

        }

        public static Bitmap TryGet(ICoreServerAPI sapi)
        {
            ICoreAPI coreApi = sapi;
            if (map == null)
            {
                string mapFolder = coreApi.GetOrCreateDataPath("Maps");
                string mapFile = mapFolder + @"/landforms.png";
                if (File.Exists(mapFile))
                {
                    Bitmap bitmapRef = null;
                    bitmapRef = new Bitmap(mapFile);
                    return map = bitmapRef;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return map;
            }
        }

        public static Bitmap TryGetClimate(ICoreServerAPI sapi)
        {
            ICoreAPI coreApi = sapi;
            if (climateMap == null)
            {
                string mapFolder = coreApi.GetOrCreateDataPath("Maps");
                string mapFile = mapFolder + @"/climate.png";
                if (File.Exists(mapFile))
                {
                    Bitmap bitmapRef = null;
                    bitmapRef = new Bitmap(mapFile);
                    return climateMap = bitmapRef;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return climateMap;
            }
        }

        // to be replaced with dictionary
        public static int GetBiomeFromPixel(Color color)
        {
            return color.R;
            //water
            if (color.R > 0)
            {
                return 1;
            }
            //land
            else
            {
                return 0;
            }

            if (color.R > 5)
            {
                return new Random().Next(6, 30);
            }
            else
            {
                return color.R;

            }

        }
    }
}
