using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Help class to store triangle data so we can sort the distances
[Serializable]
public class VertexData
{
    //The distance to water from this vertex
    public float distance;
    //An index so we can form clockwise triangles
    public int index;
    //The global Vector3 position of the vertex
    public Vector3 globalVertexPos;
}
