using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Entities.Animations;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.PropertiesDb;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Mafi.Base.Assets.Core;

namespace WallDumpingMod
{
    public class WDRegistrator : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            LogWrite.Info("Registrating Wall Loose Product");

            LooseProductProto.Gfx coalGfx = ((LooseProductProto)registrator.PrototypesDb.Get(Ids.Products.Rock).Value).Graphics;

            Proto.Str ps = Proto.CreateStr(PrototypeIDs.Products.WDLooseID,  "WallLooseProduct", "WallDumping Product");
            LooseProductProto.Gfx LPGfx =
                new LooseProductProto.Gfx(coalGfx.PrefabsPath.Value, "Assets/Materials/IronOre.mat", false, ColorRgba.Red, null,  "Assets/COIExtended/Icons/IlmeniteOre.png"
                    );
            LooseProductProto looseProductProto =
                new LooseProductProto(
                    PrototypeIDs.Products.WDLooseID, ps, true, true, LPGfx);


            looseProductProto.AddOrReplaceParam(new LooseProductParam(PrototypeIDs.Products.WDTerrainID));

            registrator.PrototypesDb.Add(looseProductProto);

            TerrainMaterialProto.Gfx gfx =
                new TerrainMaterialProto.Gfx(ColorRgba.Red, ColorRgba.Red,
                "Assets/Base/Terrain/Textures/Alumina-512-albedo",
                "Assets/Base/Terrain/Textures/Alumina-512-normals");

            TerrainMaterialProto tp =
                new TerrainMaterialProto(
                    PrototypeIDs.Products.WDTerrainID, "WallTerrain", looseProductProto, 100.Percent(), new ThicknessTilesF(100), new ThicknessTilesF(200), gfx);
            
            registrator.PrototypesDb.Add(tp);

            MachineProto mp = (MachineProto)registrator.PrototypesDb.Get(Ids.Machines.IndustrialMixerT2).Value;

            string[] strArray = new string[6]
{
        "         C@v            ",
        "   [1][2][2][2]         ",
        "A~>[2][3][3][3][2][2]   ",
        "D#>[2][3][3][3][2][2]>~X",
        "B~>[2][3][3][3][2][2]   ",
        "   [1][2][2][2]         "
};
            int quantity1 = 1;
            int quantity2 = 12;
            int quantity3 = 10;
            int quantity4 = 2;
            int quantity5 = 24;
            MachineProto machine2 = registrator.MachineProtoBuilder
                .Start("WallMixer II", PrototypeIDs.Machines.WDTerrainMachine)
                .Description("WallMixer").SetCost(Costs.Machines.MixerT2)
                .SetElectricityConsumption(200.Kw())
                .SetCategories(Ids.ToolbarCategories.Production)
                .SetLayout(strArray)
                .SetPrefabPath("Assets/Base/Machines/Infrastructure/ConcreteMixerT2.prefab")
                .SetMachineSound("Assets/Base/Machines/Infrastructure/ConcreteMixer/ConcreteMixer_Sound.prefab")
                .SetAnimationParams((AnimationParams)AnimationParams.Loop(new Percent?(80.Percent()))).EnableSemiInstancedRendering()
                .BuildAndAdd();
            registrator.RecipeProtoBuilder
                .Start("Concrete mixing", PrototypeIDs.Recipes.WDTerrainRecipe, machine2)
                .AddInput<RecipeProtoBuilder.State>(quantity1, Ids.Products.Cement)
                .AddInput<RecipeProtoBuilder.State>(quantity2, Ids.Products.Gravel)
                .AddInput<RecipeProtoBuilder.State>(quantity3, Ids.Products.SlagCrushed)
                .AddInput<RecipeProtoBuilder.State>(quantity4, Ids.Products.Water)
                .SetDurationSeconds(20)
                .AddOutput<RecipeProtoBuilder.State>(quantity5, PrototypeIDs.Products.WDLooseID)
                .BuildAndAdd();

        }
    }

}
