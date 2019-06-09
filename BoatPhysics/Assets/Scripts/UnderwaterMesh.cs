using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnderwaterMesh
{

    private Water currentWater = null;

    private Transform originalTransform;

    private Vector3[] originalMeshVertices;
    private int[] originalMeshTriangles;

    private Vector3[] originalMeshVerticesWorld;

    private float[] distancesToWater;

    private List<Triangle> underwaterTriangles = new List<Triangle>();
    public List<Triangle> UnderwaterTriangles { get { return underwaterTriangles; } }


    #region Constructor

    public UnderwaterMesh(GameObject _originalObject, Water _water)
    {
        currentWater = _water;

        originalTransform = _originalObject.transform;

        Mesh _originalMesh = _originalObject.GetComponent<MeshFilter>().mesh;

        originalMeshVertices = _originalMesh.vertices;
        originalMeshTriangles = _originalMesh.triangles;

        originalMeshVerticesWorld = new Vector3[originalMeshVertices.Length];
        distancesToWater = new float[originalMeshVertices.Length];
    }

    #endregion

    #region Methods
    /// <summary>
    /// Get all the triangles and add the underwater ones to the list of underwatertriangles
    /// </summary>
    private void AddTriangles()
    {
        //This list represent a triangle
        List<VertexData> vertices = new List<VertexData>();
        // The three points of a triangle
        vertices.Add(new VertexData());
        vertices.Add(new VertexData());
        vertices.Add(new VertexData());
        int i = 0; 
        while (i < originalMeshTriangles.Length)
        {
            //Set the vertices datas for each triangles
            for (int j = 0; j < 3; j++)
            {
                vertices[j].distance = distancesToWater[originalMeshTriangles[i]];
                vertices[j].index = j;
                vertices[j].globalVertexPos = originalMeshVerticesWorld[originalMeshTriangles[i]];
                i++;
            }

            // If all vertices are above the water, skip to the next triangle
            if (vertices[0].distance > 0 && vertices[1].distance > 0 && vertices[2].distance > 0)
            {
                continue;
            }

            //Add or create the underwatertriangles

            if (vertices[0].distance <= 0 && vertices[1].distance <= 0 && vertices[2].distance <= 0)
            {
                // All vertices are underwater, so save the complete triangle
                underwaterTriangles.Add(new Triangle(vertices[0].globalVertexPos, vertices[1].globalVertexPos, vertices[2].globalVertexPos, currentWater));
            }
            else
            {
                vertices = vertices.OrderByDescending(v => v.distance).ToList();
                // 1 or 2 vertices are underwater, we have to find out which one(s)
                if (vertices.Where(v => v.distance > 0).ToArray().Length == 2)
                {
                    // Two vertices are above the water
                    AddTrianglesTwoAboveWater(vertices);
                }
                else
                {
                    // One vertex is above the water
                    AddTrianglesOneAboveWater(vertices);
                }
            }
        }
    }

    /// <summary>
    ///Build the new triangles where one of the old vertex is above the water
    /// </summary>
    /// <param name="_datas">Original Triangle</param>
    private void AddTrianglesOneAboveWater(List<VertexData> _datas)
    {
        // The first vertex is always the lowest one in the water because we ordered them by distance
        Vector3 H = _datas[0].globalVertexPos;

        // Left of H
        Vector3 M = Vector3.zero;
        // Right of H
        Vector3 L = Vector3.zero;

        int MIndex = _datas[0].index - 1 < 0 ? 2 : _datas[0].index - 1;

        float distanceH = _datas[0].distance;
        float distanceM = 0;
        float distanceL = 0;

        if (_datas[1].index == MIndex)
        {
            M = _datas[1].globalVertexPos;
            distanceM = _datas[1].distance;

            L = _datas[2].globalVertexPos;
            distanceL = _datas[2].distance;
        }
        else
        {
            M = _datas[2].globalVertexPos;
            distanceM = _datas[2].distance;

            L = _datas[1].globalVertexPos;
            distanceL = _datas[1].distance;
        }

        //Point I_M
        Vector3 MH = H - M;

        float t_M = -distanceM / (distanceH - distanceM);

        Vector3 MI_M = t_M * MH;

        Vector3 I_M = MI_M + M;


        //Point I_L
        Vector3 LH = H - L;

        float t_L = -distanceL / (distanceH - distanceL);

        Vector3 LI_L = t_L * LH;

        Vector3 I_L = LI_L + L;

        underwaterTriangles.Add(new Triangle(M, I_M, I_L, currentWater));
        underwaterTriangles.Add(new Triangle(M, I_L, L, currentWater));
    }

    /// <summary>
    ///Build the new triangles where two of the old vertices are above the water
    /// </summary>
    /// <param name="vertexData">Original Triangle</param>
    private void AddTrianglesTwoAboveWater(List<VertexData> vertexData)
    {
        //H and M are above the water
        //H is after the vertice that's below water, which is L
        //So we know which one is L because it is last in the sorted list
        Vector3 L = vertexData[2].globalVertexPos;

        //Find the index of H
        int H_index = vertexData[2].index + 1;
        if (H_index > 2)
        {
            H_index = 0;
        }


        //We also need the heights to water
        float h_L = vertexData[2].distance;
        float h_H = 0f;
        float h_M = 0f;

        Vector3 H = Vector3.zero;
        Vector3 M = Vector3.zero;

        //This means that H is at position 1 in the list
        if (vertexData[1].index == H_index)
        {
            H = vertexData[1].globalVertexPos;
            M = vertexData[0].globalVertexPos;

            h_H = vertexData[1].distance;
            h_M = vertexData[0].distance;
        }
        else
        {
            H = vertexData[0].globalVertexPos;
            M = vertexData[1].globalVertexPos;

            h_H = vertexData[0].distance;
            h_M = vertexData[1].distance;
        }


        //Now we can find where to cut the triangle

        //Point J_M
        Vector3 LM = M - L;

        float t_M = -h_L / (h_M - h_L);

        Vector3 LJ_M = t_M * LM;

        Vector3 J_M = LJ_M + L;


        //Point J_H
        Vector3 LH = H - L;

        float t_H = -h_L / (h_H - h_L);

        Vector3 LJ_H = t_H * LH;

        Vector3 J_H = LJ_H + L;


        //Save the data, such as normal, area, etc
        //1 triangle below the water
        underwaterTriangles.Add(new Triangle(L, J_H, J_M, currentWater));
    }

    /// <summary>
    /// Display the underwatermesh 
    /// </summary>
    /// <param name="mesh">Mesh to display</param>
    /// <param name="name">Name of the mesh</param>
    /// <param name="triangesData">Datas of the mesh</param>
    public void DisplayMesh(Mesh mesh, string name, List<Triangle> triangesData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Build the mesh
        for (int i = 0; i < triangesData.Count; i++)
        {
            //From global coordinates to local coordinates
            Vector3 p1 = originalTransform.InverseTransformPoint(triangesData[i].PointA);
            Vector3 p2 = originalTransform.InverseTransformPoint(triangesData[i].PointB);
            Vector3 p3 = originalTransform.InverseTransformPoint(triangesData[i].PointC);

            vertices.Add(p1);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p2);
            triangles.Add(vertices.Count - 1);

            vertices.Add(p3);
            triangles.Add(vertices.Count - 1);
        }

        //Remove the old mesh
        mesh.Clear();

        //Give it a name
        mesh.name = name;

        //Add the new vertices and triangles
        mesh.vertices = vertices.ToArray();

        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();

    }

    /// <summary>
    /// Generate the underwater mesh
    /// </summary>
    public void GenerateUnderWaterMesh()
    {
        underwaterTriangles.Clear();

        Vector3 _worldPosition;
        for (int i = 0; i < originalMeshVertices.Length; i++)
        {
            // Get the world positon of each vertex
            _worldPosition = originalTransform.TransformPoint(originalMeshVertices[i]);
            originalMeshVerticesWorld[i] = _worldPosition;
            distancesToWater[i] = currentWater.DistanceTo(_worldPosition);
        }

        AddTriangles(); 
    }
    #endregion

}
