using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallDumpingMod
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class WDActions
    {
        private readonly ProtosDb _protosDb;
        private readonly UnlockedProtosDb _unlockedProtosDb;

        public WDActions(
            ProtosDb protosDb,
            UnlockedProtosDb unlockedProtosDb
        )
        {
            // This unlocks the custom entity at startup
            // Next verions will show the use of research
            unlockedProtosDb.Unlock(ImmutableArray.Create((IProto)protosDb.Get(PrototypeIDs.Products.WDLooseID).Value));
            unlockedProtosDb.Unlock(ImmutableArray.Create((IProto)protosDb.Get(PrototypeIDs.Machines.WDTerrainMachine).Value));
            unlockedProtosDb.Unlock(ImmutableArray.Create((IProto)protosDb.Get(PrototypeIDs.Recipes.WDTerrainRecipe).Value));
        }
    }
}
