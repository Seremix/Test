using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HittableRigidHandler : MonoBehaviour
{
    public List<Vector3> _collisionPoints = new List<Vector3>();
    public HittableRigid hrModel;

    private HittableRigid[] _hittableRigidsPool;

    private void Start()
    {
        hrModel.hittableRigidHandler = this;
    }

    public void CollectCollisionPoint(Vector3 point)
    {
        _collisionPoints.Add(point);
    }

    public void ClearCollisionList()
    {
        _collisionPoints.Clear();
    }

    public void InitializePool (int poolSize)
    {
        _hittableRigidsPool = new HittableRigid[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            _hittableRigidsPool[i] = Instantiate(hrModel);
            _hittableRigidsPool[i].hittableRigidHandler = this;
            _hittableRigidsPool[i].gameObject.SetActive(false);
        }
    }

    public void ActivatedHittableRigid(Vector3 position, Quaternion rotation)
    {
        foreach (HittableRigid hittableRigid in _hittableRigidsPool)
        {
            if (!hittableRigid.isActiveAndEnabled)
            {
                hittableRigid.gameObject.SetActive(true);
                hittableRigid.transform.position = position;
                hittableRigid.transform.rotation = rotation;
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(Vector3 point in _collisionPoints)
        {
            Gizmos.DrawSphere(point, .5f);
        }
    }
}
