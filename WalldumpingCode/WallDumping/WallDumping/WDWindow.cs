using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Core.Prototypes;
using Mafi.Core.Products;
using Mafi;
using UnityEngine;
using Mafi.Unity;
using static Mafi.Unity.Assets.Unity.Generated.Icons;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Utils;
using Mafi.Unity.InputControl.RecipesBook;
using Mafi.Collections;
using System.Collections;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ports.Io;
using static Mafi.Base.Ids;
using Mafi.Core;
using Mafi.Core.Terrain;
using System.Runtime.Remoting.Contexts;
using Mafi.Base;
using Mafi.Core.Factory.Machines;
using Mafi.Collections.ImmutableCollections;


namespace WallDumpingMod;

[GlobalDependency(RegistrationMode.AsEverything)]
public class WDWindow : WindowView
{
    private TxtField xvalueBox;
    private TxtField yvalueBox;
    private TxtField quantity;
    private Btn dumpButton;
    private Btn logButton;

    private int currentX = 0;
    private int currentY = 0;
    private int currentQ = 0;

    private readonly ProtosDb _protosDb;
    private readonly TerrainManager terrainManager;

    public WDWindow(ProtosDb db, TerrainManager tm ) : base("WDWindow")
    {
        _protosDb = db;
        terrainManager = tm;
    }

    private readonly Set<Proto> m_protosFound = new Set<Proto>();

    protected override void BuildWindowContent()
    {
        SetTitle("WallDumping");
        SetContentSize(600f, 300f);
        PositionSelfToCenter();
        MakeMovable();

        xvalueBox = Builder.NewTxtField("X-Value")
            .SetPlaceholderText("X-Value")
            .SetCharLimit(30)
            .SetStyle(Builder.Style.Global.LightTxtFieldStyle)
            .PutToLeftTopOf<TxtField>((IUiElement)GetContentPanel(), new Vector2(200f, 30f), Offset.Top(10f) + Offset.Left(10f))
            .SetDelayedOnEditEndListener((s) => { updateCurrent(ref currentX, s); });

        yvalueBox = Builder.NewTxtField("Y-Value")
            .SetPlaceholderText("Y-Value")
            .SetCharLimit(30)
            .SetStyle(Builder.Style.Global.LightTxtFieldStyle)
            .PutToLeftTopOf<TxtField>((IUiElement)GetContentPanel(), new Vector2(200f, 30f), Offset.Top(50f) + Offset.Left(10f))
            .SetDelayedOnEditEndListener((s) => { updateCurrent(ref currentY, s); });

        yvalueBox = Builder.NewTxtField("X-Value")
            .SetPlaceholderText("X-Value")
            .SetCharLimit(30)
            .SetStyle(Builder.Style.Global.LightTxtFieldStyle)
            .PutToLeftTopOf<TxtField>((IUiElement)GetContentPanel(), new Vector2(200f, 30f), Offset.Top(90f) + Offset.Left(10f))
            .SetDelayedOnEditEndListener((s) => { updateCurrent(ref currentQ, s); });

        dumpButton = Builder.NewBtnPrimary("test")
            .SetText("Dump")
            .OnClick(() => onDump())
            .PutToLeftTopOf((IUiElement)GetContentPanel(), new Vector2(100f, 30f), Offset.Top(130f) + Offset.Left(10f));

        logButton = Builder.NewBtnPrimary("test")
            .SetText("Print")
            .OnClick(() => onDump(false))
            .PutToLeftTopOf((IUiElement)GetContentPanel(), new Vector2(100f, 30f), Offset.Top(170f) + Offset.Left(10f));

        void onDump(bool dump = true)
        {
            Quantity q = currentQ.Quantity();
            ProductQuantity pq;
            Tile2iAndIndex txia = new Tile2i(currentX, currentY).ExtendIndex(terrainManager);
            LogWrite.Info($"sh {terrainManager.GetHeight(new Tile2f(currentX, currentY))}");
            LogWrite.Info(Debug_ExplainTileContents(txia.Index));
            explainNeighbours(txia.Index);
            if (dump)
            {
                ProductProto looseProto = (ProductProto)_protosDb.Get(PrototypeIDs.Products.WDLooseID).Value;
                //            ProductProto looseProto = (ProductProto)_protosDb.Get(Ids.Products.Rock).Value;
                ThicknessTilesF thickness = looseProto.DumpableProduct.Value.TerrainMaterial.Value.QuantityToThickness(q);
                LogWrite.Info($"q {q} t {thickness}");
                terrainManager.DumpMaterial(txia, new TerrainMaterialThicknessSlim(looseProto.DumpableProduct.Value.TerrainMaterial.Value.SlimId, thickness));
                LogWrite.Info(Debug_ExplainTileContents(txia.Index));
                LogWrite.Info($"fh {terrainManager.GetHeight(new Tile2f(currentX, currentY))}");
                explainNeighbours(txia.Index);

            }
        }

        void onClick(ProductProto product)
        {
        }
        
        void updateCurrent(ref int current, string v)
        {
            int.TryParse(v, out current);
        }
    }

    void explainNeighbours(Tile2iIndex index)
    {
        ImmutableArray<Tile2iAndIndexRel>.Enumerator enumerator2 = terrainManager.EightNeighborsDeltas.GetEnumerator();
        while (enumerator2.MoveNext())
        {
            LogWrite.Info("Neighbour " + Debug_ExplainTileContents(index + enumerator2.Current.IndexDelta));
        }
    }
    internal string Debug_ExplainTileContents(Tile2iIndex index)
    {
        StringBuilder stringBuilder = new StringBuilder(500);
        HeightTilesF height = terrainManager.GetHeight(index);
        stringBuilder.AppendLine($"Coord: {terrainManager.IndexToTile_Slow(index)}");
        stringBuilder.AppendLine($"Height: {height}");
        stringBuilder.AppendLine($"Layers: {terrainManager.GetLayersCountNoBedrock(index)}");
        int num = 0;
        TerrainLayerEnumerator enumerator = terrainManager.EnumerateLayers(index).GetEnumerator();
        while (enumerator.MoveNext())
        {
            TerrainMaterialThicknessSlim current = enumerator.Current;
            stringBuilder.AppendLine($"  #{num++}: {current.Thickness} of {current.SlimId.ToFull(terrainManager)} (slim {current.SlimId})");
        }

        ThicknessTilesF thicknessTilesF = ThicknessTilesF.One;
        ThicknessTilesF thicknessTilesF2 = -ThicknessTilesF.One;
        ImmutableArray<Tile2iAndIndexRel>.Enumerator enumerator2 = terrainManager.EightNeighborsDeltas.GetEnumerator();
        while (enumerator2.MoveNext())
        {
            ThicknessTilesF thicknessTilesF3 = terrainManager.GetHeight(index + enumerator2.Current.IndexDelta) - height;
            if (thicknessTilesF3 < thicknessTilesF)
            {
                thicknessTilesF = thicknessTilesF3;
            }

            if (thicknessTilesF3 > thicknessTilesF2)
            {
                thicknessTilesF2 = thicknessTilesF3;
            }
        }

        TerrainMaterialProto material = terrainManager.GetFirstLayer(index).Material;
        stringBuilder.AppendLine($"Diff to lower: {-thicknessTilesF}");
        stringBuilder.AppendLine($"Diff to higher: {thicknessTilesF2}");
        stringBuilder.AppendLine($"Collapse range: {material.MinCollapseHeightDiff} - {material.MaxCollapseHeightDiff}");
        return stringBuilder.ToString();
    }
}
