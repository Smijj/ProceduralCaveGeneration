using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule
{
    public class Room {
        public List<Coord> Tiles;
        public List<Coord> EdgeTiles;
        public List<Room> ConnectedRooms;
        public int RoomSize;

        public Room() { }
        public Room (List<Coord> roomTiles, int[,] map) {
            this.Tiles = roomTiles;
            this.RoomSize = this.Tiles.Count;
            this.ConnectedRooms = new List<Room>();

            this.EdgeTiles = new List<Coord>();
            // Go through room's tiles and add the edge tiles to the EdgeTile list
            foreach(Coord tile in Tiles) {
                for (int x = tile.TileX - 1; x < tile.TileX + 1; x++) {
                    for (int y = tile.TileY - 1; y < tile.TileY + 1; y++) {
                        if (x != tile.TileX && y != tile.TileY) continue;   // Exclude diagonal neighbours

                        if (map[x, y] >= 1) {
                            EdgeTiles.Add(tile);
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB) {
            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);
        }
        public bool IsConnected(Room otherRoom) {
            return ConnectedRooms.Contains(otherRoom);
        }
    }
}
