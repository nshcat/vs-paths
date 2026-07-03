using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Common.Collectible.Block;

namespace fancypaths.BlockEntities;

public class BlockEntityGlowingPath
    : BlockEntity, ITexPositionSource
{
    
    
    /// <summary>
    /// Whether the path is currently glowing.
    /// </summary>
    public bool IsActive { get; set; }
        = false;

    /// <summary>
    /// Remaining time the path will glow.
    /// </summary>
    /// <remarks>This will influence the output light level.</remarks>
    public float RemainingGlowTime { get; set; }
        = 0.0f;
    
    /// <summary>
    /// Maximum amount of time an activated time will glow.
    /// </summary>
    protected static readonly float MaxGlowTime = 5.0f;
    
    protected ICoreClientAPI capi;
    
    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
    
        capi = api as ICoreClientAPI;
        
        RegisterGameTickListener(OnGameTick, 50);
    }

    public void OnGameTick(float dt)
    {
        if (this.IsActive)
        {
            this.RemainingGlowTime -= dt;
            if (this.RemainingGlowTime <= 0.0f)
            {
                this.IsActive = false;
                this.RemainingGlowTime = 0.0f;
            }
            
            this.MarkDirty(true);
        }
    }
    
    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        Shape shape = capi.TesselatorManager.GetCachedShape(new AssetLocation("fancypaths:block/path-stone-kacheln"));
        
        capi.Tesselator.TesselateShape("glowingpath", shape, out MeshData meshdata, this);
        
        if (this.IsActive)
        {
            var glowTime = GameMath.Clamp(this.RemainingGlowTime / MaxGlowTime, 0.0f, 1.0f);
            var lightLevel = GameMath.Lerp(0.0f, 255.0f, glowTime);
            var lightLevelInt = (int)lightLevel;
            
            for (int i = 0; i < meshdata.FlagsCount; i++)
            {
                meshdata.Flags[i] |= (lightLevelInt & 0xFF); // glow level
            }
        }

        mesher.AddMeshData(meshdata);
        
        return true;
    }
    
    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);

        tree.SetFloat("remainingGlowTime", this.RemainingGlowTime);
        tree.SetBool("isActive", this.IsActive);
    }
    
    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);

        this.RemainingGlowTime = tree.GetFloat("remainingGlowTime");
        this.IsActive = tree.GetBool("isActive");
    }

    public void TriggerGlowing()
    {
        this.IsActive = true;
        this.RemainingGlowTime = MaxGlowTime;
        this.MarkDirty(true);
    }
    
    public TextureAtlasPosition this[string textureCode]
    {
        get
        {
            if (IsActive)
            {
                return capi.BlockTextureAtlas.Positions[capi.World.GetBlock(new AssetLocation("ember")).FirstTextureInventory.Baked.TextureSubId];
            }
            
            return capi.BlockTextureAtlas.Positions[capi.World.GetBlock(new AssetLocation("fancypaths:pathkachelnstone-andesite")).FirstTextureInventory.Baked.TextureSubId];
        }
    }

    public Size2i AtlasSize => ((ICoreClientAPI) this.Api).BlockTextureAtlas.Size;
}