using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHandler : MonoBehaviour
{

    public bool debugTrail = false;

    public struct BufferObj
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 size;
    }
    private LinkedList<BufferObj> _trailList = new LinkedList<BufferObj>();
    private LinkedList<BufferObj> _trailFillerList = new LinkedList<BufferObj>();
    private int _maxFrameBuffer = 10;

    public LayerMask hitLayers;
    public WeaponHandler weaponHandlerRef;
    private BoxCollider weaponCollider;
    Animator _anim;
    public int _attackId = 0;
    private HittableRigidHandler _hittableRigidHandler;

    public float rotationSpeed = 20f;
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _hittableRigidHandler = GetComponent<HittableRigidHandler>();
        weaponCollider = weaponHandlerRef.Weapon.GetComponent<BoxCollider>();
        _hittableRigidHandler.InitializePool(_maxFrameBuffer+1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetAttack(1);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            SetAttack(2);
        }
    }
    private void LateUpdate()
    {
        if (_anim.GetBool("IsAttacking"))
        {
            CheckTrail();
        }
    }

    private void SetAttack (int attackType)
    {
        if (_anim.GetBool("CanAttack"))
        {
            _attackId++;
            _anim.SetTrigger("Attack");
            _anim.SetInteger("AttackType", attackType);
            _hittableRigidHandler.ClearCollisionList();
        }
    }

    private void CheckTrail()
    {
        BufferObj bo = new BufferObj();
        bo.size = weaponCollider.size;
        bo.rotation = weaponCollider.transform.rotation;
        bo.position = weaponCollider.transform.position + weaponCollider.transform.TransformDirection(weaponCollider.center);
        _trailList.AddFirst(bo);

        if (_trailList.Count > _maxFrameBuffer)
        {
            _trailList.RemoveLast();
        }

        if (_trailFillerList.Count > 1)
        {
            _trailFillerList = FillTrail(_trailList.First.Value, _trailList.Last.Value);
        }
        Collider[] hits = Physics.OverlapBox(bo.position, bo.size / 2, bo.rotation, hitLayers, QueryTriggerInteraction.Ignore);

        Dictionary<long, Collider> colliderList = new Dictionary<long, Collider>();

        CollectColliders(bo, hits, colliderList);

        foreach (BufferObj cbo in _trailFillerList)
        {
            hits = Physics.OverlapBox(cbo.position, cbo.size / 2, cbo.rotation, hitLayers, QueryTriggerInteraction.Ignore);
            CollectColliders(cbo, hits, colliderList);
        }

        foreach (Collider collider in colliderList.Values)
        {
            HitData hd = new HitData();
            hd.id = _attackId;
            Hittable hittable = collider.GetComponent<Hittable>();
            if (hittable)
            {
                hittable.Hit(hd);
            }
        }
    }

    private void CollectColliders(BufferObj source, Collider[] hits, Dictionary<long, Collider> colliderList)
    {
        if (hits.Length > 0)
        {
            _hittableRigidHandler.ActivatedHittableRigid(source.position, source.rotation);
        }
        for (int i = 0; i < hits.Length; i++)
        {
            if (!colliderList.ContainsKey(hits[i].GetInstanceID()))
                colliderList.Add(hits[i].GetInstanceID(), hits[i]);
        }
    }

    private LinkedList<BufferObj> FillTrail(BufferObj from, BufferObj to)
    {
        LinkedList<BufferObj> fillerList = new LinkedList<BufferObj>();
        float distance = Mathf.Abs((from.position - to.position).magnitude);

        if (distance > weaponCollider.size.z)
        {
            float steps = Mathf.Ceil(distance / weaponCollider.size.z);
            float stepsAmount = 1 / (steps + 1);
            float stepValue = 0;
            for (int i = 0; i < (int)steps; i++)
            {
                stepValue += stepsAmount;
                BufferObj tmpBo = new BufferObj();
                tmpBo.size = weaponCollider.size;
                tmpBo.position = Vector3.Lerp(from.position, to.position, stepValue);
                tmpBo.rotation = Quaternion.Lerp(from.rotation, to.rotation, stepValue);
            }
        }
        return fillerList;
    }

    private void OnDrawGizmos()
    {
        foreach (BufferObj bo in _trailList)
        {
            
            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.TRS(bo.position, bo.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, bo.size);
        }
        foreach (BufferObj bo in _trailFillerList)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(bo.position, bo.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, bo.size);
        }
    }
}
