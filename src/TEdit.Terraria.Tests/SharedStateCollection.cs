namespace TEdit.Terraria.Tests;

/// <summary>
/// Classes in this collection reset the static WorldConfiguration / TerrariaDataStore,
/// so they must not run while other tests read that state.
/// </summary>
[CollectionDefinition("SharedState", DisableParallelization = true)]
public class SharedStateCollection
{
}
