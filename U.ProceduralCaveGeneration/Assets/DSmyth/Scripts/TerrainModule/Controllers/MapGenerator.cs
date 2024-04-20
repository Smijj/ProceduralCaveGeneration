using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule {
    public class MapGenerator : MonoBehaviour
    {

        [Header("Basic Settings")]
        [SerializeField] private int m_Width;
        [SerializeField] private int m_Height;
        [SerializeField] private int m_BorderSize = 5;
        [SerializeField] private int m_MinWallSizeThreshold = 50;
        [SerializeField] private int m_MinCavitySizeThreshold = 50;
        [Range(0,100)]
        [SerializeField] private int m_RandomFillPercent;
        
        [Header("Smoothing Settings")]
        [SerializeField] private bool m_SmoothMapWithBias = true;
        [SerializeField] private int m_SmoothingIterations = 5;

        [Header("Seed Generation")]
        [SerializeField] private string m_Seed;
        [SerializeField] private bool m_UseRandomSeed;


        [Header("Debug")]
        [SerializeField] private bool m_DrawGizmos = false;


        private int[,] m_Map;


        #region Unity

        private void Start() {
            GenerateMap();
        }
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                GenerateMap();
            }
        }
        private void OnDrawGizmos() {
            if (!Application.isPlaying || m_Map == null || !m_DrawGizmos) return;

            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    Gizmos.color = m_Map[x, y] == 1 ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-m_Width / 2 + x + 0.5f, 0, -m_Height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }

        #endregion


        private void GenerateMap() {
            m_Map = new int[m_Width, m_Height];
            RandomFillMap();

            if (m_SmoothMapWithBias)
                SmoothMapWithBias();
            else
                SmoothMap();

            // Removes any walls or cavities below a certain threshold
            ProcessMapRegions();

            // Create solid border around the map
            int[,] borderedMap = new int[m_Width + m_BorderSize * 2, m_Height + m_BorderSize * 2];
            for (int x = 0; x < borderedMap.GetLength(0); x++) {
                for (int y = 0; y < borderedMap.GetLength(1); y++) {
                    if (x >= m_BorderSize && x < m_Width + m_BorderSize && y >= m_BorderSize && y < m_Height + m_BorderSize) {
                        borderedMap[x, y] = m_Map[x - m_BorderSize, y - m_BorderSize];
                    } else {
                        borderedMap[x, y] = 1;
                    }
                }
            }

            // Generate mesh using map data
            MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
            if (meshGenerator != null ) {
                meshGenerator.GenerateMesh(borderedMap);
            }
        }


        // Creating the raw Map data

        /// <summary>
        /// Randomly fill the Map[,] array with data. 1 or 0, wall or no wall.
        /// </summary>
        private void RandomFillMap()
        {
            if (m_UseRandomSeed)
            {
                m_Seed = DateTime.Now.TimeOfDay.ToString();
            }

            System.Random pseudoRandom = new System.Random(m_Seed.GetHashCode());

            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Height; y++)
                {
                    if (x == 0 || x == m_Width - 1 || y == 0 || y == m_Height - 1)
                    {
                        m_Map[x, y] = 1;
                    } else
                    {
                        m_Map[x, y] = pseudoRandom.Next(0, 100) < m_RandomFillPercent ? 1 : 0;
                    }
                }
            }
        }
        /// <summary>
        /// Smooths out the Map[,] data. 
        /// This method has bias due to smoothing algorithm using the map data its already smoothed throughout the smoothing loop
        /// instead of working off a copy of the map data that gets cloned at the start of each smoothing iteration.
        /// </summary>
        private void SmoothMapWithBias()
        {
            for (int i = 0; i < m_SmoothingIterations; i++)
            {
                for (int x = 0; x < m_Width; x++)
                {
                    for (int y = 0; y < m_Height; y++)
                    {
                        int neighbourWallTiles = GetSurroundingWallCount(x, y);

                        // Alters a seperate map array using the cellular automata rules to avoid altering the base data for with the smoothing is being derived from
                        if (neighbourWallTiles > 4)
                        {
                            m_Map[x, y] = 1;
                        } else if (neighbourWallTiles < 4)
                        {
                            m_Map[x, y] = 0;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Smooths out the Map[,] data without bias.
        /// </summary>
        private void SmoothMap()
        {
            for (int i = 0; i < m_SmoothingIterations; i++)
            {

                int[,] smoothedMap = (int[,])m_Map.Clone();    // Stores the edited map data from the celluar automata algorithm

                for (int x = 0; x < m_Width; x++)
                {
                    for (int y = 0; y < m_Height; y++)
                    {
                        int neighbourWallTiles = GetSurroundingWallCount(x, y);

                        // Alters a seperate map array using the cellular automata rules to avoid altering the base data for with the smoothing is being derived from
                        if (neighbourWallTiles > 4)
                        {
                            smoothedMap[x, y] = 1;
                        } else if (neighbourWallTiles < 4)
                        {
                            smoothedMap[x, y] = 0;
                        }
                    }
                }

                // Sets the main map array to the altered smoothMap array for the next itteration of smoothing.
                m_Map = smoothedMap;
            }
        }
        /// <summary>
        /// Returns the number of wall tiles that surround a point on the Map grid. 
        /// Counts grid positions that are out of bounds of the Map data as walls.
        /// </summary>
        private int GetSurroundingWallCount(int gridX, int gridY)
        {
            int wallCount = 0;

            for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {

                    // Dont look at tiles out of the bounds of the m_Map array
                    if (!IsInMapRange(neighbourX, neighbourY))
                    {
                        wallCount++;    // if it is out of bound, add to wall count anyway to encourage wall growth
                        continue;
                    }
                    if (neighbourX == gridX && neighbourY == gridY) continue;   // Dont look at original tile

                    wallCount += m_Map[neighbourX, neighbourY];
                }
            }

            return wallCount;
        }


        // Processing the different regions of the map and connecting seperate Rooms together

        private void ProcessMapRegions() {
            List<List<Coord>> wallRegions = GetRegions(1); // Regions of type 1 (wall type)

            foreach (List<Coord> wallRegion in wallRegions) { 
                if (wallRegion.Count < m_MinWallSizeThreshold) {
                    foreach (Coord tile in wallRegion) {
                        m_Map[tile.TileX, tile.TileY] = 0; // Remove any wall regions that are less than a threshold size
                    }
                }
            }

            List<List<Coord>> roomRegions = GetRegions(0); // Regions of type 1 (wall type)
            List<Room> survivingRooms = new List<Room>();

            foreach (List<Coord> roomRegion in roomRegions) {
                if (roomRegion.Count < m_MinCavitySizeThreshold) {
                    foreach (Coord tile in roomRegion) {
                        m_Map[tile.TileX, tile.TileY] = 1; // Remove any room/cavity regions that are less than a threshold size
                    }
                } else {
                    survivingRooms.Add(new Room(roomRegion, m_Map));
                }
            }

            survivingRooms.Sort();  // Sorts rooms in order of largest to smallest
            survivingRooms[0].IsMainRoom = true;
            survivingRooms[0].IsAccesibleFromMainRoom = true;

            ConnectClosestRooms(survivingRooms);
        }

        private void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {
            // Go through each of the surviving rooms and compare them to eachother and find the closest connection points to connect at

            List<Room> roomListA = new List<Room>();    // All rooms that are not accessible from the main room
            List<Room> roomListB = new List<Room>();    // All the rooms that are accessible from the main room

            // Add rooms to different accessiblity list depending on if they are accessible to the main room or not
            if (forceAccessibilityFromMainRoom) {
                foreach(Room room in allRooms) {
                    if (room.IsAccesibleFromMainRoom) {
                        roomListB.Add(room);
                    } else {
                        roomListA.Add(room);
                    }
                }
            } else {
                roomListA = allRooms;
                roomListB = allRooms;
            }

            int bestDistance = 0;
            Coord bestTileA = new Coord();
            Coord bestTileB = new Coord();
            Room bestRoomA = new Room();
            Room bestRoomB = new Room();
            bool possibleConnectionFound = false;

            foreach(Room roomA in roomListA) {
                if (!forceAccessibilityFromMainRoom) {
                    possibleConnectionFound = false;    // Dont reset the possible connection found if forceAccessibilityFromMainRoom is true,
                                                        // as we want to make sure it considers all the possible rooms for the best connection to the main room and not just the first one.

                    if (roomA.ConnectedRooms.Count > 0) continue;   // If this room already has a connection, continue to the next room
                }

                foreach(Room roomB in roomListB) {
                    if (roomA == roomB || roomA.IsConnected(roomB)) continue;       // Dont consider RoomB if its comparing to itself (i.e. RoomA == RoomB), or if they are already connected

                    // Go through each of the edge tiles in RoomA & RoomB and find the two tiles that are closest together
                    for(int tileIndexA = 0; tileIndexA < roomA.EdgeTiles.Count; tileIndexA++) {
                        for (int tileIndexB = 0; tileIndexB < roomB.EdgeTiles.Count; tileIndexB++) {
                            Coord tileA = roomA.EdgeTiles[tileIndexA];
                            Coord tileB = roomB.EdgeTiles[tileIndexB];
                            int distanceBetweenRooms = (int)Mathf.Pow(tileA.TileX - tileB.TileX, 2) + (int)Mathf.Pow(tileA.TileY - tileB.TileY, 2);

                            // Found new best connection
                            if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
                                possibleConnectionFound = true;
                                bestDistance = distanceBetweenRooms;
                                bestTileA = tileA;
                                bestTileB = tileB;
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }

                // If a possible connection was found when comparing RoomA to all the other surviving Rooms, connect RoomA to that other room. Only called in the foreach loop if ForceAccessibility is false.
                if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
                    CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
                }
            }

            // If ForceAccessiblityToMainRoom is true, and there was possible connection found when comparing all the non-connected Rooms to all the connected Rooms,
            // connect the closest non-main connected Room to its closest room thats connected to the main Room.
            if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
                ConnectClosestRooms(allRooms, true);    // Make sure to iterate through again to ensure connectivity
            }

            // Reiterate through this function, but with ForceAccessibilityFromMainRoom == true, to ensure all the rooms are accessible from the main room
            if (!forceAccessibilityFromMainRoom) {
                ConnectClosestRooms(allRooms, true);
            }
        }

        private void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
            Room.ConnectRooms(roomA, roomB);
            Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.yellow, 10);
        }

        private List<List<Coord>> GetRegions(int tileType) {
            List<List<Coord>> regions = new List<List<Coord>>();
            int[,] mapFlags = new int[m_Width, m_Height];   // 2d int array that keeps track of if a tile has been looked at already

            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    if (mapFlags[x, y] == 1 || m_Map[x, y] != tileType) continue;   // Skip if this tile has already been looked at or the tile doesnt match the tileType
                    
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion) {
                        mapFlags[tile.TileX, tile.TileY] = 1;   // Mark all the tiles in this new region as looked at so they wont be considered in the following iterations
                    }
                }
            }

            return regions;
        }

        private List<Coord> GetRegionTiles(int startX, int startY) {
            List<Coord> tiles = new List<Coord>();          // List of tiles in this region
            int[,] mapFlags = new int[m_Width, m_Height];   // 2d int array that keeps track of if a tile has been looked at already
            int tileType = m_Map[startX, startY];           // the type of tile the starting tile is

            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(startX, startY));
            mapFlags[startX, startY] = 1;                   // mark the tile at the start pos as been looked out

            while (queue.Count > 0) {
                Coord tile = queue.Dequeue();               // Gets first item in queue, an removes it from the queue
                tiles.Add(tile);

                // look at the current tiles adjacent tiles
                for (int x = tile.TileX - 1; x <= tile.TileX + 1; x++) {
                    for (int y = tile.TileY - 1; y <= tile.TileY + 1; y++) {
                        if (!IsInMapRange(x, y) || mapFlags[x, y] == 1 || m_Map[x, y] != tileType) continue;   // Continue if the tile is out of range,
                                                                                                               // has already been looked at,
                                                                                                               // or isnt the same type as the starting tile
                        // Dont look at diagonal tiles
                        if (y == tile.TileY || x == tile.TileX) {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }

            return tiles;
        }


        #region Helpers

        private bool IsInMapRange(int x, int y) {
            return x >= 0 && x < m_Width && y >= 0 && y < m_Height;

        }
        private Vector3 CoordToWorldPoint(Coord tile) {
            return new Vector3(-m_Width / 2f + 0.5f + tile.TileX, 2, -m_Height / 2 + 0.5f + tile.TileY);
        }

        #endregion

    }
}
