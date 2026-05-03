using GameRealisticMap.Arma3.TerrainBuilder;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3
{
    /// <summary>
    /// Converts built terrain feature data into a list of <see cref="TerrainBuilder.TerrainBuilderObject"/> records
    /// for a specific feature type (forests, roads, buildings, etc.).
    /// Registered in <see cref="Arma3LayerGeneratorCatalog"/>; called during WRP generation.
    /// </summary>
    public interface ITerrainBuilderLayerGenerator
    {
        /// <summary>
        /// Generates <see cref="TerrainBuilder.TerrainBuilderObject"/> placement records for this layer.
        /// </summary>
        /// <param name="config">The Arma 3 map configuration (world name, tile size, resolution, etc.).</param>
        /// <param name="context">The build context; call <see cref="IContext.GetData{T}"/> to retrieve feature data.</param>
        /// <param name="scope">Progress scope for reporting generation progress.</param>
        Task<IEnumerable<TerrainBuilderObject>> Generate(IArma3MapConfig config, IContext context, IProgressScope scope);
    }
}