using UnityEngine;
using System;

namespace SeveralBees.Scripts
{
    public class Button : MonoBehaviour
    {
        public Action<bool> Click;
        public string Name;
        private void Awake()
        {
            UnityEngine.Debug.Log($"[Several Bees] Button '{Name}' Awake");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.collider == SeveralBees.Instance.LeftPointerCollider || collision.collider == SeveralBees.Instance.RightPointerCollider)
            {
                UnityEngine.Debug.Log($"[Several Bees] Button '{Name}' Clicked");
                Click?.Invoke(collision.collider == SeveralBees.Instance.LeftPointerCollider);
            }
        }
    }
}
