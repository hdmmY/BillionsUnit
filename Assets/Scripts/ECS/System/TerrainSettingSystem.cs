// using Unity.Entities;

// public class TerrainSettingSystem : ComponentSystem
// {
//     protected override void OnCreateManager (int capacity)
//     {
//         GenerateTerrain (GameSettingSingletonComponent.MAP_WIDTH,
//             GameSettingSingletonComponent.MAP_HEIGHT);
//     }

//     protected override void OnUpdate ()
//     {

//     }

//     private void GenerateTerrain (int width, int height)
//     {
//         int size = width * height;

//         TerrainSingletonComponent.Instance.Terrains = new TerrainType[size];

//         for (int i = 0; i < size; i++)
//         {
//             TerrainSingletonComponent.Instance.Terrains[i] = TerrainType.Normal;
//         }
//     }

// }