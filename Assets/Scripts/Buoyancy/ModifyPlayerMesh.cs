using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyPlayerMesh
{
    //The player transform needed to get the global position of a vertice
    private Transform playerTrans;

    //Coordinates of all vertices in the original player
    Vector3[] playerVertices;

    //Positions in allVerticesArray, such as 0, 3, 5, to build triangles
    int[] playerTriangles;

    //So we only need to make the transformation from local to global once
    public Vector3[] playerVerticesGlobal;

    //Find all the distances to water once because some triangles share vertices, so reuse
    float[] allDistancesToWater;

    [Tooltip("The triangles which are parts of the player that's under water")]
    public List<TriangleData> underWaterTriangleData = new List<TriangleData>();

    //Help class to store triangle data so we can sort the distances
    private class VertexData
    {
        /// <summary>
        /// The distance to water from this vertex
        /// </summary>
        public float distance;

        /// <summary>
        /// An index so we can form clockwise triangles
        /// </summary>
        public int index;

        /// <summary>
        /// The global Vector3 position of the vertex
        /// </summary>
        public Vector3 globalVertexPos;
    }


    public ModifyPlayerMesh(GameObject player)
    {
        //Get the transform
        playerTrans = player.transform;

        //Init the arrays and lists
        playerVertices = player.GetComponent<MeshFilter>().mesh.vertices;
        playerTriangles = player.GetComponent<MeshFilter>().mesh.triangles;

        //The player vertices in global position
        playerVerticesGlobal = new Vector3[playerVertices.Length];
        //Find all the distances to water once because some triangles share vertices, so reuse
        allDistancesToWater = new float[playerVertices.Length];
    }

    //Generate the underwater mesh
    public void GenerateUnderwaterMesh()
    {
        //Reset
        underWaterTriangleData.Clear();

        //Find all the distances to water once because some triangles share vertices, so reuse
        for (int j = 0; j < playerVertices.Length; j++)
        {
            //The coordinate should be in global position
            Vector3 globalPos = playerTrans.TransformPoint(playerVertices[j]);

            //Save the global position so we only need to calculate it once here
            //And if we want to debug we can convert it back to local
            playerVerticesGlobal[j] = globalPos;

            allDistancesToWater[j] = WaterController.current.DistanceToWater(globalPos, Time.time);
        }

        //Add the triangles that are below the water
        AddTriangles();
    }

    //Add all the triangles that's part of the underwater mesh
    private void AddTriangles()
    {
        //List that will store the data we need to sort the vertices based on distance to water
        //Its length is always 3 because of a triangle.
        List<VertexData> vertexDataList = new List<VertexData>();

        //Add init data that will be replaced
        vertexDataList.Add(new VertexData());
        vertexDataList.Add(new VertexData());
        vertexDataList.Add(new VertexData());


        //Loop through all the triangles (3 vertices at a time = 1 triangle)
        int i = 0;
        while (i < playerTriangles.Length)
        {
            //Loop through the 3 vertices. Initialiing 3 VertexData of an i-th triangle of player.
            for (int j = 0; j < 3; j++)
            {
                //Save the data we need
                vertexDataList[j].distance = allDistancesToWater[playerTriangles[i]];
                vertexDataList[j].index = j;
                vertexDataList[j].globalVertexPos = playerVerticesGlobal[playerTriangles[i]];
                i++;
            }


            //All vertices are above the water
            if (vertexDataList[0].distance > 0f && vertexDataList[1].distance > 0f && vertexDataList[2].distance > 0f)
            {
                continue;
            }

            //Create the triangles that are below the waterline

            //All vertices are underwater
            if (vertexDataList[0].distance < 0f && vertexDataList[1].distance < 0f && vertexDataList[2].distance < 0f)
            {
                Vector3 p1 = vertexDataList[0].globalVertexPos;
                Vector3 p2 = vertexDataList[1].globalVertexPos;
                Vector3 p3 = vertexDataList[2].globalVertexPos;

                //Save the triangle
                underWaterTriangleData.Add(new TriangleData(p1, p2, p3));
            }
            //1 or 2 vertices are below the water. This will be handled by other functions.
            else
            {
                //Sort the vertices. vertexDataList[0] has the highest distance.
                vertexDataList.Sort( (x, y) => x.distance.CompareTo(y.distance));

                vertexDataList.Reverse();

                //Only 1 vertice is above the water, the rest is below
                if (vertexDataList[0].distance > 0f && vertexDataList[1].distance < 0f && vertexDataList[2].distance < 0f)
                {
                    AddTrianglesOneAboveWater(vertexDataList);
                }
                //Only 2 vertices are above the water.
                else if (vertexDataList[0].distance > 0f && vertexDataList[1].distance > 0f && vertexDataList[2].distance < 0f)
                {
                    AddTrianglesTwoAboveWater(vertexDataList);
                }
            }
        }
    }

    //Build the new triangles where one of the old vertices is above the water.
    private void AddTrianglesOneAboveWater(List<VertexData> vertexData)
    {
        //Ths 1st vertex H is always at position 0. Only H is above water.
        Vector3 H = vertexData[0].globalVertexPos;

        //Left of H is the vertex M
        //Right of H is the vertex L
        // HML is the original triangle of the player.

        //Find the index of M
        int M_index = vertexData[0].index - 1;
        if (M_index < 0)
        {
            M_index = 2;
        }

        //We also need the heights to water. M, L are exactly on the suface of water.
        float h_H = vertexData[0].distance;
        float h_M = 0f;
        float h_L = 0f;

        Vector3 M = Vector3.zero;
        Vector3 L = Vector3.zero;

        //This means M is at position 1 in the List
        if (vertexData[1].index == M_index)
        {
            M = vertexData[1].globalVertexPos;
            L = vertexData[2].globalVertexPos;

            h_M = vertexData[1].distance;
            h_L = vertexData[2].distance;
        }
        else
        {
            M = vertexData[2].globalVertexPos;
            L = vertexData[1].globalVertexPos;

            h_M = vertexData[2].distance;
            h_L = vertexData[1].distance;
        }


        //Now we can calculate where we should cut the triangle to form 2 new triangles
        //because the resulting area will always form a square  M-I_M-I_L-L.

        //Point I_M.
        Vector3 MH = H - M;   //The vector from M to H. 

        float t_M = -h_M / (h_H - h_M);  //using similarity of triangles. Note that h_M is negative.

        Vector3 MI_M = t_M * MH;  //

        Vector3 I_M = MI_M + M;   //I_M is the position where the segment MH and the surface of water intersects.


        //Point I_L
        Vector3 LH = H - L;

        float t_L = -h_L / (h_H - h_L);

        Vector3 LI_L = t_L * LH;

        Vector3 I_L = LI_L + L;


        //Save the data, such as normal, area, etc      
        //2 triangles below the water  
        underWaterTriangleData.Add(new TriangleData(M, I_M, I_L));
        underWaterTriangleData.Add(new TriangleData(M, I_L, L));
    }


    //Build the new triangles where two of the old vertices are above the water
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
        underWaterTriangleData.Add(new TriangleData(L, J_H, J_M));
    }

    //Display the underwater mesh
    public void DisplayMesh(Mesh mesh, string name, List<TriangleData> triangesData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Build the mesh
        for (int i = 0; i < triangesData.Count; i++)
        {
            //From global coordinates to local coordinates
            Vector3 p1 = playerTrans.InverseTransformPoint(triangesData[i].p1);
            Vector3 p2 = playerTrans.InverseTransformPoint(triangesData[i].p2);
            Vector3 p3 = playerTrans.InverseTransformPoint(triangesData[i].p3);

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


}
