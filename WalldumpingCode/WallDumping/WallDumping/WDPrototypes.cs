using Mafi;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Factory.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mafi.Core.Prototypes.Proto;

namespace WallDumpingMod;


public partial class PrototypeIDs
{
    public partial class Products
    {
        public static readonly LooseProductProto.ID WDLooseID = new LooseProductProto.ID("WDLooseProduct");
        public static readonly TerrainMaterialProto.ID WDTerrainID = new TerrainMaterialProto.ID("WDTerrainMaterial");
    }

    public partial class Recipes
    {
        public static readonly RecipeProto.ID WDTerrainRecipe = new RecipeProto.ID("WDTerrainRecipe");
    }

    public partial class Machines
    {
        public static readonly MachineProto.ID WDTerrainMachine = new MachineProto.ID("WDTerrainMachine");
    }
}
