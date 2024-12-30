using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class CameraLock : MonoBehaviour
{
    [SerializeField] private Lockable lockX;
    [SerializeField] private Lockable lockY;
    [SerializeField] private Lockable lockZ;
    private Vector3 lockedPos;

    private void Update()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        lockedPos.x = GetLockValue(lockX, cameraPos.x);
        lockedPos.y = GetLockValue(lockY, cameraPos.y);
        lockedPos.z = GetLockValue(lockZ, cameraPos.z);
        transform.position = lockedPos;
    }

    private float GetLockValue(Lockable lockable, float pos)
    {
        return lockable.lockCoordinate ? Mathf.Clamp(pos, lockable.lockMin, lockable.lockMax) : pos;
    }

    [System.Serializable]
    public struct Lockable
    {
        public bool lockCoordinate;
        public float lockMin;
        public float lockMax;
    }
}
