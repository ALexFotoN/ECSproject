using UnityEngine;

namespace RTS.MonoBehaviours
{
    /// <summary>
    /// Handles unit movement towards a target position.
    /// </summary>
    public class UnitMovement : MonoBehaviour
    {
        [Header("Settings")]
        public float moveSpeed = 4f;
        public float rotationSpeed = 10f;
        public float stoppingDistance = 0.2f;

        private Vector3? targetPosition;
        private bool isMoving;

        public bool IsMoving => isMoving;

        public void MoveTo(Vector3 position)
        {
            targetPosition = position;
            targetPosition = new Vector3(position.x, transform.position.y, position.z);
            isMoving = true;
        }

        public void Stop()
        {
            targetPosition = null;
            isMoving = false;
        }

        private void Update()
        {
            if (!targetPosition.HasValue)
                return;

            var direction = targetPosition.Value - transform.position;
            direction.y = 0;
            var distance = direction.magnitude;

            if (distance <= stoppingDistance)
            {
                Stop();
                return;
            }

            // Move
            var normalizedDir = direction.normalized;
            transform.position += normalizedDir * moveSpeed * Time.deltaTime;

            // Rotate
            if (normalizedDir != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(normalizedDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
