using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DSmyth.CharacterModule
{
    public class PlayerCtrl : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 10;
        
        private Rigidbody m_Rigid;
        private Vector3 m_Velocity;

        private void Start() {
            m_Rigid = GetComponent<Rigidbody>();
        }
        private void Update() {
            m_Velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * m_Speed;
        }

        private void FixedUpdate() {
            m_Rigid.MovePosition(m_Rigid.position + m_Velocity * Time.fixedDeltaTime);
        }
    }
}
