using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using Vintagestory.GameContent;
using Vintagestory.API.Config;

// class author: Erik3003

/** Disables:
 * GenTerra
 * GenBlockLayers
 * GenMaps
 * GenCaves
 */

namespace dominions.world
{
    public class DisableVanillaWorldgenHandlers : ModSystem
    {
        private ICoreServerAPI api;
        private ICoreAPI coreApi;

        public override double ExecuteOrder()
        {
            RuntimeEnv.DebugOutOfRangeBlockAccess = true;
            return 0;
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void Start(ICoreAPI api)
        {
            this.coreApi = api;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            api.Event.ServerRunPhase(EnumServerRunPhase.LoadGamePre, WipeVanillaWorldgenHandlers);
        }

        void WipeVanillaWorldgenHandlers()
        {
            //worldtype
            IWorldGenHandler handlergroup = this.api.Event.GetRegisteredWorldGenHandlers("standard");
            foreach (var handlers in handlergroup.OnChunkColumnGen)
            {
                for (int i = 0; handlers != null && i < handlers.Count; i++)
                {
                    if (handlers[i].Target.GetType().Namespace == "Vintagestory.ServerMods" && (handlers[i].Target.GetType().Name == "GenBlockLayers" || handlers[i].Target.GetType().Name == "GenTerra" || handlers[i].Target.GetType().Name == "GenCaves"))
                    {
                        handlers.RemoveAt(i);
                        i--;
                    }
                }
            }

            {
                var handlers = handlergroup.OnMapRegionGen;
                for (int i = 0; handlers != null && i < handlers.Count; i++)
                {
                    if (handlers[i].Target.GetType().Namespace == "Vintagestory.ServerMods" && (handlers[i].Target.GetType().Name == "GenMaps" || handlers[i].Target.GetType().Name == "GenBlockLayers"))
                    {
                        handlers.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
