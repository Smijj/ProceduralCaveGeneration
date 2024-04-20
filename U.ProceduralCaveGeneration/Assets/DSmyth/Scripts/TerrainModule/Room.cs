using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule
{
    public class Room : IComparable<Room> {
        public List<Coord> Tiles;
        public List<Coord> EdgeTiles;
        public List<Room> ConnectedRooms;
        public int RoomSize;
        public bool IsAccesibleFromMainRoom;
        public bool IsMainRoom;

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

        public void SetAccessibleFromMainRoom() {
            if (IsAccesibleFromMainRoom) return;    // Return if this room has already been set as connected to the Main room
            
            // Set this room as being connected
            IsAccesibleFromMainRoom = true;
            // Go through each of this room's connected rooms and set them as connected as well
            foreach (Room room in ConnectedRooms) {
                room.SetAccessibleFromMainRoom();
            }
            
        }

        public static void ConnectRooms(Room roomA, Room roomB) {
            // When connecting 2 rooms together, if either of them are connected to the main room, set the other as connected to the main room as well
            if (roomA.IsAccesibleFromMainRoom) {
                roomB.SetAccessibleFromMainRoom();
            } else if (roomB.IsAccesibleFromMainRoom) {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);
        }
        public bool IsConnected(Room otherRoom) {
            return ConnectedRooms.Contains(otherRoom);
        }
        public int CompareTo(Room otherRoom) {
            return otherRoom.RoomSize.CompareTo(this.RoomSize);
        }
    }
}
