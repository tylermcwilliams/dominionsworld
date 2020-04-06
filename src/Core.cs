using Vintagestory.API.Common;

[assembly: ModInfo("dominionsworld",
    Description = "Custom world generation for dominions server",
    Website = "",
    Authors = new[] { "archpriest" },
    Version = "1.1.4"
    )]

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
