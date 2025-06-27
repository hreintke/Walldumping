using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mafi.Unity.Mine;
using Mafi.Core;
using Mafi.Core.Terrain.Physics;
using Mafi.Collections;
using System.Reflection;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Terrain;
using Mafi.Core.Products;

namespace WallDumpingMod;

public static class ReflectionExtensions
{
    public static T GetFieldValue<T>(this object obj, string name)
    {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        return (T)field?.GetValue(obj);
    }
}


[GlobalDependency(RegistrationMode.AsSelf)]
[HarmonyPatch]
public class WDHarmony
{
    private readonly Harmony harmony;
    static bool printCache = true;
    WDHarmony()
    {
        harmony = new Harmony("myPatch");
        harmony.PatchAll();
        LogWrite.Info("Harmony patches applied");
        Type t2 = AccessTools.TypeByName("TerrainPhysicsSimulator");
        MethodInfo[] methodInfos = t2
                           .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (MethodInfo methodInfo in methodInfos)
        {

            LogWrite.Info($"{methodInfo.Name}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("TerrainPhysicsSimulator", "processSolidHeightChange")]
    static void HeightPrefix(object __instance, Tile2iAndIndex tileAndIndex, Queueue<Tile2iAndIndex> ___m_tilesToUpdate)
    {
        LogWrite.Info($"ProcessSolid {___m_tilesToUpdate.Count} {tileAndIndex}");
    }

    [HarmonyPrefix]
    [HarmonyPatch("TerrainPhysicsSimulator", "<processSolidHeightChange>g__processTile|50_0")]
    static void HeightTilePrefix(object __instance, Tile2iAndIndex t)
    {
        LogWrite.Info($"ProcessSolidProcessTile  {t}");
    }


    [HarmonyPrefix]
    [HarmonyPatch("TerrainPhysicsSimulator", "Update")]
    static void UpdatePrefix(object __instance, ImmutableArray<MinMaxPair<ThicknessTilesF>> ___m_collapseHeightDiffsCache)
    {
        Queueue<Tile2iAndIndex> ttu = (Queueue<Tile2iAndIndex>)__instance.GetFieldValue<Queueue<Tile2iAndIndex>>("m_tilesToUpdate");
//                LogWrite.Info($"PhysicsSim {ttu.Count}");
        Queueue<Tile2iAndIndex> ttu1 = (Queueue<Tile2iAndIndex>)__instance.GetFieldValue<Queueue<Tile2iAndIndex>>("m_tilesToUpdate1");
        //        LogWrite.Info($"PhysicSim1 {ttu1}");

        if (printCache)
        {
            foreach (var mm in ___m_collapseHeightDiffsCache)
            {
                LogWrite.Info($"Collapse cache {mm.Min} {mm.Max}");
            }
            printCache = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("TerrainPhysicsSimulator", "simulateTerrainFall")]
    static void TerrainFallPostfix(object __instance,
      Tile2iAndIndex tileHi,
      Tile2iAndIndex tileLo,
      ThicknessTilesF heightDiff,
      bool __result,
      TerrainManager ___m_terrainManager
        )
    {
        TerrainMaterialThicknessSlim firstLayerSlim = ___m_terrainManager.GetFirstLayerSlim(tileHi.Index);
        LogWrite.Info($"TerrainFall {__result} Hi {xy(tileHi)} Lo {xy(tileLo)} df {heightDiff} ly {firstLayerSlim.SlimId} {fillMaterial(firstLayerSlim,___m_terrainManager)}");
    }

    static string xy(Tile2iAndIndex ti)
    {
        return $"({ti.X},{ti.Y})";
    }

    static string fillMaterial(TerrainMaterialThicknessSlim ts, TerrainManager tm)
    {
        TerrainMaterialProto tp = ts.SlimId.ToFull(tm);
        return ($"{tp.ToString()}");
    }
}
