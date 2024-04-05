using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule
{
    public class MapGenerator : MonoBehaviour
    {

        [Header("Settings")]
        [SerializeField] private int m_Width;
        [SerializeField] private int m_Height;
        [SerializeField] private int m_BorderSize = 5;
        [SerializeField] private int m_MinWallSizeThreshold = 50;
        [SerializeField] private int m_MinCavitySizeThreshold = 50;

        [SerializeField] private string m_Seed;
        [SerializeField] private bool m_UseRandomSeed;
        [SerializeField] private int m_SmoothingIterations = 5;

        [Range(0,100)]
        [SerializeField] private int m_RandomFillPercent;

        [Header("Debug")]
        [SerializeField] private bool m_DrawGizmos = false;


        private int[,] m_Map;



        private void Start() {
            GenerateMap();
        }
        private void Update() {
            //if (Input.GetKeyDown(KeyCode.Space)) {
            //    GenerateMap();
            //}
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


        private void GenerateMap() {
            m_Map = new int[m_Width, m_Height];
            RandomFillMap();

            for (int i = 0; i < m_SmoothingIterations; i++) {
                SmoothMap();
            }

            ProcessMap();


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

        private void ProcessMap() {
            List<List<Coord>> wallRegions = GetRegions(1); // Regions of type 1 (wall type)

            foreach (List<Coord> wallRegion in wallRegions) { 
                if (wallRegion.Count < m_MinWallSizeThreshold) {
                    foreach (Coord tile in wallRegion) {
                        m_Map[tile.TileX, tile.TileY] = 0; // Remove any wall regions that are less than a threshold size
                    }
                }
            }

            List<List<Coord>> roomRegions = GetRegions(0); // Regions of type 1 (wall type)

            foreach (List<Coord> roomRegion in roomRegions) {
                if (roomRegion.Count < m_MinCavitySizeThreshold) {
                    foreach (Coord tile in roomRegion) {
                        m_Map[tile.TileX, tile.TileY] = 1; // Remove any room/cavity regions that are less than a threshold size
                    }
                }
            }
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

        List<Coord> GetRegionTiles(int startX, int startY) {
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

        private bool IsInMapRange(int x, int y) {
            return x >= 0 && x < m_Width && y >= 0 && y < m_Height;
                
        }

        private void RandomFillMap() {
            if (m_UseRandomSeed) {
                m_Seed = DateTime.Now.TimeOfDay.ToString();
            }

            System.Random pseudoRandom = new System.Random(m_Seed.GetHashCode());

            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    if (x == 0 || x == m_Width-1 || y == 0 || y == m_Height-1) {
                        m_Map[x, y] = 1;
                    } else {
                        m_Map[x, y] = pseudoRandom.Next(0, 100) < m_RandomFillPercent ? 1 : 0;
                    }
                }
            }
        }

        private void SmoothMap() {
            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y);

                    if (neighbourWallTiles > 4) {
                        m_Map[x, y] = 1;
                    } else if (neighbourWallTiles < 4) {
                        m_Map[x, y] = 0;
                    }
                }
            }
        }

        private int GetSurroundingWallCount(int gridX, int gridY) {
            int wallCount = 0;

            for (int neighbourX = gridX-1; neighbourX <= gridX + 1; neighbourX++) {
                for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {

                    // Dont look at tiles out of the bounds of the m_Map array
                    if (!IsInMapRange(neighbourX, neighbourY)) {
                        wallCount++;    // if it is out of bound, add to wall count anyway to encourage wall growth
                        continue;       
                    }
                    if (neighbourX == gridX && neighbourY == gridY) continue;   // Dont look at original tile

                    wallCount += m_Map[neighbourX, neighbourY];
                }
            }

            return wallCount;
        }

        public struct Coord {
            public int TileX;
            public int TileY;

            public Coord(int _tileX, int _tileY) {
                this.TileX = _tileX;
                this.TileY = _tileY;
            }
        }

        
    }
}
