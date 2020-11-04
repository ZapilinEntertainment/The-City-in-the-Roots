using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexLift : MonoBehaviour
{
    public float height = 0f, speed = 1f;
    public HexPos position;
    // Update is called once per frame
    void Update()
    {
        float h = height - transform.position.y, t = Time.deltaTime,  s = speed * t;
        if (h > 0)
        {
            if (s >= h)
            {
                transform.position = new Vector3(transform.position.x, height, transform.position.z);
                var t0 = transform.GetChild(0);
                t0.GetComponent<Renderer>().sharedMaterial = PoolMaster.current.baseMaterial;
                t0.GetComponent<MeshCollider>().enabled = true;
                t0.tag = GameConstants.HEX_COLLIDER_TAG;
                var hex = gameObject.AddComponent<Hex>();
                hex.Initialize(position, transform.localScale.y);                
                Destroy(this);
                return;
            }
            else
            {
                transform.Translate(Vector3.up * s);
                speed += t * 2f;
            }
        }
    }

}
