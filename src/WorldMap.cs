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
        public static int[] GenClimateLayer(int regionX, int regionZ, int sizeX, int sizeZ, ICoreServerAPI api)
        {
            int[] outData = new int[sizeX * sizeZ];

            int noiseSizeLandform = api.WorldManager.RegionSize / TerraGenConfig.climateMapScale;
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

        public static Bitmap map = null;
        public static Bitmap climateMap = null;
        // Colors
        public static Color ocean = Color.FromArgb(0, 0, 0);
        public static Color abyss = Color.FromArgb(1, 0, 0);

        public static Color mesa = Color.FromArgb(2, 0, 0);
        public static Color mount = Color.FromArgb(3, 0, 0);
        public static Color flatlandsh = Color.FromArgb(4, 0, 0);
        public static Color flatlandsl = Color.FromArgb(5, 0, 0);


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
