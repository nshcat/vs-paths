using fancypaths.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace fancypaths.Blocks;

public class BlockGlowingPath
    : Block
{
    public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
    {
        BlockEntityGlowingPath be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityGlowingPath;
        if (be != null)
        {
            be.TriggerGlowing();
        }
    }
}