using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.CharacterModule
{
    public class Player2DCtrl : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 10;

        private Rigidbody2D m_Rigid;
        private Vector2 m_Velocity;

        private void Start() {
            m_Rigid = GetComponent<Rigidbody2D>();
        }
        private void Update() {
            m_Velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * m_Speed;
        }

        private void FixedUpdate() {
            m_Rigid.MovePosition(m_Rigid.position + m_Velocity * Time.fixedDeltaTime);
        }
    }
}
