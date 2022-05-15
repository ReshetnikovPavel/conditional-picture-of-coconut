using Player;
using UnityEngine;

namespace MainGameScripts
{
    public class CameraController : MonoBehaviour
    {
        public float damping = 1.5f;
        public Vector2 offset = new Vector2(2f, 1f);
        public bool faceLeft;
        public Character player;
        private int lastX;

        private void Start()
        {
            offset = new Vector2(Mathf.Abs(offset.x), offset.y);
            FindPlayer(faceLeft);
        }

        private void FindPlayer(bool playerFaceLeft)
        {
            lastX = Mathf.RoundToInt(player.transform.position.x);
            transform.position = playerFaceLeft
                ? new Vector3(player.transform.position.x - offset.x, player.transform.position.y + offset.y,
                    transform.position.z)
                : new Vector3(player.transform.position.x + offset.x, player.transform.position.y + offset.y,
                    transform.position.z);
        }

        private void Update()
        {
            if (!player) return;
            var currentX = Mathf.RoundToInt(player.transform.position.x);
            if (currentX > lastX) faceLeft = false;
            else if (currentX < lastX) faceLeft = true;
            lastX = Mathf.RoundToInt(player.transform.position.x);

            var target = faceLeft
                ? new Vector3(player.transform.position.x - offset.x, player.transform.position.y + offset.y,
                    transform.position.z)
                : new Vector3(player.transform.position.x + offset.x, player.transform.position.y + offset.y,
                    transform.position.z);
            var currentPosition = Vector3.Lerp(transform.position, target, damping * Time.deltaTime);
            transform.position = currentPosition;
        }
    }
}