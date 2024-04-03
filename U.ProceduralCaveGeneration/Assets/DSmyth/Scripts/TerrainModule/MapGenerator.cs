using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.TerrainModule
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private int m_Width;
        [SerializeField] private int m_Height;


        [SerializeField] private string m_Seed;
        [SerializeField] private bool m_UseRandomSeed;

        [Range(0,100)]
        [SerializeField] private int m_RandomFillPercent;

        private int[,] m_Map;

        private void Start() {
            GenerateMap();
        }

        private void GenerateMap() {
            m_Map = new int[m_Width, m_Height];
            RandomFillMap();
        }

        private void RandomFillMap() {
            if (m_UseRandomSeed) {
                m_Seed = DateTime.Now.TimeOfDay.ToString();
            }

            System.Random psuedoRandom = new System.Random(m_Seed.GetHashCode());

            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    m_Map[x, y] = psuedoRandom.Next(0, 100) < m_RandomFillPercent ? 1 : 0;
                }
            }
        }

        private void OnDrawGizmos() {
            if (!Application.isPlaying || m_Map == null) return;

            for (int x = 0; x < m_Width; x++) {
                for (int y = 0; y < m_Height; y++) {
                    Gizmos.color = m_Map[x, y] == 1 ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-m_Width/2 + x + 0.5f, 0, -m_Height/2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
