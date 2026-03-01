using System.IO;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using NarrativeGraphTool.Editor.Model;
using NarrativeGraphTool.Editor.Parser;
using UnityEngine;

namespace NarrativeGraphTool.Editor.AssetImport
{
    /// <summary>
    /// Imports .narrativegraph assets so Unity recognizes them and the Graph Toolkit can open them.
    /// Also parses the graph into a <see cref="NarrativeGraphTool.Runtime.Data.NarrativeGraphData"/>
    /// sub-asset, which can be assigned directly to a NarrativeRunner by dragging the
    /// .narrativegraph file onto the field.
    /// </summary>
    [ScriptedImporter(1, NarrativeGraph.AssetExtension)]
    public class NarrativeGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<NarrativeGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"Failed to load Narrative Graph asset: {ctx.assetPath}");
                return;
            }

            var objects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(ctx.assetPath);
            if (objects == null || objects.Length == 0)
            {
                Debug.LogError($"No serialized objects in Narrative Graph asset: {ctx.assetPath}");
                return;
            }

            // Parse runtime data first so it can be the main (visible) asset.
            var runtimeData = NarrativeGraphParser.Parse(graph);
            runtimeData.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            ctx.AddObjectToAsset("RuntimeData", runtimeData);
            ctx.SetMainObject(runtimeData);

            // Hide the internal Graph Toolkit objects — they must stay in the file so
            // the GT editor can open and edit the graph, but they shouldn't appear in
            // the Project window alongside the runtime data.
            for (var i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].hideFlags = HideFlags.HideInHierarchy;
                    ctx.AddObjectToAsset("Object_" + i, objects[i]);
                }
            }
        }
    }
}
