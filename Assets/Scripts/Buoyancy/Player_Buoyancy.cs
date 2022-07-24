using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Buoyancy : MonoBehaviour
{
    //Drags
    public GameObject underWaterObj;

    /// <summary>
    /// Script that's doing everything needed with the boat mesh, such as finding out which part is above the water
    /// </summary>
     private ModifyPlayerMesh modifyPlayerMesh;

    /// <summary>
    /// Mesh for debugging
    /// </summary>
    private Mesh underWaterMesh;

    /// <summary>
    /// The player's rigidbody
    /// </summary>
    private Rigidbody myRigidbody;

    /// <summary>
    /// 물의 밀도
    /// </summary>
    private float rhoWater = 1027f;


    //Add all forces that act on the squares below the water
    void AddUnderWaterForces()
    {
        //Get all triangles
        List<TriangleData> underWaterTriangleData = modifyPlayerMesh.underWaterTriangleData;

        for (int i = 0; i < underWaterTriangleData.Count; i++)
        {
            //This triangle
            TriangleData triangleData = underWaterTriangleData[i];

            //Calculate the buoyancy force
            Vector3 buoyancyForce = BuoyancyForce(rhoWater, triangleData);

            //Add the force to the boat
            myRigidbody.AddForceAtPosition(buoyancyForce, triangleData.center);
        }
    }

    //The buoyancy force so the player can float
    Vector3 BuoyancyForce(float rho, TriangleData triangleData)
    {
        // F_buoyancy = rho * g * V
        // rho - density of the mediaum you are in
        // g - gravity
        // V - volume of the submerged part of the player

        // V = z * S * n 
        // z - distance to surface
        // S - surface area
        // n - normal to the surface
        Vector3 buoyancyForce = rho * Physics.gravity.y * triangleData.distanceToSurface * triangleData.area * triangleData.normal;

        //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        return buoyancyForce;
    }



    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();

        //Init the script that will modify the boat mesh
        modifyPlayerMesh = new ModifyPlayerMesh(gameObject);

        //Meshes that are below and above the water
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        //Generate the under water mesh
        modifyPlayerMesh.GenerateUnderwaterMesh();

        //Display the under water mesh
        modifyPlayerMesh.DisplayMesh(underWaterMesh, "UnderWater Mesh", modifyPlayerMesh.underWaterTriangleData);
    }

    void FixedUpdate()
    {
        //Add forces to the part of the boat that's below the water
        if (modifyPlayerMesh.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForces();
        }
    }

}
