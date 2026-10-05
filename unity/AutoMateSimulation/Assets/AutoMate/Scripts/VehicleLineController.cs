using UnityEngine;

namespace AutoMate.Simulation
{
    public sealed class VehicleLineController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float inspectionPositionX = 0f;
        [SerializeField] private float exitPositionX = 9f;

        private bool waitingForInspection;
        private bool lineRunning = true;

        public bool WaitingForInspection => waitingForInspection;

        private void Update()
        {
            if (!lineRunning)
            {
                return;
            }

            transform.Translate(Vector3.right * (moveSpeed * Time.deltaTime), Space.World);

            if (!waitingForInspection && transform.position.x >= inspectionPositionX)
            {
                transform.position = new Vector3(
                    inspectionPositionX,
                    transform.position.y,
                    transform.position.z
                );
                waitingForInspection = true;
                lineRunning = false;
                Debug.Log("[AutoMate] Vehicle reached the AI inspection station.");
            }

            if (transform.position.x >= exitPositionX)
            {
                lineRunning = false;
            }
        }

        public void ContinueLine()
        {
            waitingForInspection = false;
            lineRunning = true;
        }

        public void StopLine()
        {
            lineRunning = false;
        }

        public void ResetVehicle()
        {
            transform.position = new Vector3(-8f, 1.2f, 0f);
            waitingForInspection = false;
            lineRunning = true;
        }
    }
}
