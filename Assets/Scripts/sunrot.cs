using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunrot : MonoBehaviour
{
    [SerializeField]
    public float timescale = 0.5f;

    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, timescale * Time.deltaTime);
    }
}
