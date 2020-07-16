using Essentials;
using System.Collections;
using UnityEngine;
using System;

#if UNITY_EDITOR 
    using UnityEditor; 
#endif

public class Portal : MonoBehaviour
{
    [Tooltip("Length of the portal")]
    public float length = 1;
    [Tooltip("What can teleport")]
    public LayerMask teleportMask = Physics2D.AllLayers;
    [Tooltip("The teleport this teleport connects to")]
    public Portal link;
    [Tooltip("Only enter the portal from one side")]
    public bool oneWay;
    [HideInInspector]
    public bool flipSide;
    

    public Sign GetSignSide(Vector3 ObjPos) => Vector2.Dot(-ObjPos + transform.position, transform.right);
    public float GetLinkAngleOffset => Vector2.SignedAngle(transform.up, link.transform.up);

    BoxCollider2D teleportCol, wallCol;

    void Start()
    {
        transform.localScale = new Vector2(.01f, length);
        teleportCol = gameObject.AddComponent<BoxCollider2D>();
        teleportCol.isTrigger = true;
        wallCol = new GameObject("WallCollider").AddComponent<BoxCollider2D>();
        wallCol.gameObject.layer = LayerMask.NameToLayer("Portal");
        wallCol.transform.SetParent(transform, false);

        if (link == null) Debug.LogError("Portal link missing!");
    }

    //[ContextMenu("CreateLayer")]
    //public void CreateLayer()
    //{
    //    TagsAndLayers.CreateLayer("Portal");
    //    Physics2D.SetLayerCollisionMask(3, 0b00000);
    //}

    Collider2D[] colliderHits = new Collider2D[4];

    void FixedUpdate()
    {
        //Get all collissions with the portal
        int hitCount = teleportCol.GetContacts(colliderHits);

        for (int i = 0; i < hitCount; i++)
        {
            Transform hitTransform = colliderHits[i].transform;
            Collider2D hitCollider = colliderHits[i];

            (Sign enter, Sign exit) side;
            side.enter = GetSignSide(hitTransform.position);

            //One way is on and 
            if (oneWay && side.enter == (flipSide?-1:1))
            {
                EssentialFuncs.IgnoreCollision(hitCollider, teleportCol);
                break;
            }



            //Create a copy of the enterer
            GameObject mirrorCopy = InstantiateAMirrorCopy(hitCollider);

            void Teleport()
            {
                side.exit = GetSignSide(hitTransform.position);
                if (side.enter != side.exit)
                    hitTransform.position = mirrorCopy.transform.position;

                Destroy(mirrorCopy);
                EssentialFuncs.IgnoreCollision(hitCollider, link.teleportCol);
            }

            IEnumerator ICR = EssentialFuncs.IgnoreCollisionRoutine(teleportCol, colliderHits[i]);
            ICR.AddEvents(Teleport);
        }
    }


    private GameObject InstantiateAMirrorCopy(Collider2D hitCollider)
    {
        Transform hitTransform = hitCollider.transform;
        //Create a GameObject called "Copy"
        GameObject copy = new GameObject(hitTransform.name + " Copy");
        copy.transform.position = GetRelPosForLinkPortal(hitTransform.position);
        copy.transform.parent = hitTransform;
        copy.layer = hitTransform.gameObject.layer;
        //Copy Collider
        BoxCollider2D copyCol = copy.AddComponent(hitCollider as BoxCollider2D);
        Physics2D.IgnoreCollision(copyCol, link.teleportCol);
        //Copy SpriteRenderer
        if (hitTransform.TryGetComponent(out SpriteRenderer hitSr))
            copy.AddComponent(hitSr);
        return copy;
    }


    Vector3 GetRelPosForLinkPortal(Vector3 relativePoint)
    {
        Sign enterSide = GetSignSide(relativePoint);

        Vector3 localPos = transform.InverseTransformPoint(relativePoint);
        localPos.x = (Abs)localPos.x * -enterSide;
        Vector3 linkRelativePoint = link.transform.TransformPoint(localPos);
        return linkRelativePoint;
    }

    private void OnValidate()
    {
        link.length = length;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position, name);
        Handles.color = Color.green;
        Handles.DrawLine(transform.position - transform.up * length * .5f, transform.position + transform.up * length * .5f);
        if(oneWay)
            Handles.DrawLine(transform.position, transform.position + transform.right * (flipSide ? -1 : 1) * .5f);
        
    }

    private void OnDrawGizmosSelected()
    {
        if(link != null)
            Handles.DrawLine(transform.position, link.transform.position);

        if (link && transform.hasChanged)
            link.transform.rotation = transform.rotation;
    }
#endif
}