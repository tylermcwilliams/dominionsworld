using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace dominions.world
{
    class Core : ModSystem
    {
        ICoreAPI api;

        public override void Start(ICoreAPI api)
        {
            this.api = api;
            RegisterBlockBehaviors();
            base.Start(api);
        }

        void RegisterBlockBehaviors()
        {
            api.RegisterBlockBehaviorClass("FiniteSpreadingWater", typeof(BlockBehaviorFiniteSpreadingWater));
        }
    }
}
