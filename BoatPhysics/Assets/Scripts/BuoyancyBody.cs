using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyBody : MonoBehaviour
{
    private Rigidbody buoyancyRigidBody;
    private UnderwaterMesh underwaterMesh;
    private Mesh debuggingMesh; 
    private Water currentWater;



    #region Methods

    #region UnityMethods
    private void Awake()
    {
        buoyancyRigidBody = GetComponent<Rigidbody>();
        RaycastHit _hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out _hit))
        {
            currentWater = _hit.collider.GetComponent<Water>();
        }
    }

    private void Start()
    {
        buoyancyRigidBody = GetComponent<Rigidbody>();
        underwaterMesh = new UnderwaterMesh(this.gameObject, currentWater);
        debuggingMesh = transform.GetChild(0).GetComponent<MeshFilter>().mesh;
    }

    private void Update()
    {
        underwaterMesh.GenerateUnderWaterMesh();

        underwaterMesh.DisplayMesh(debuggingMesh, "Underwater Mesh", underwaterMesh.UnderwaterTriangles);
    }

    private void FixedUpdate()
    {
        ApplyUnderWaterForces(); 
    }
    #endregion

    #region Original Methods
    private void ApplyUnderWaterForces()
    {
        if (underwaterMesh.UnderwaterTriangles.Count == 0) return;
        Triangle _currentTriangle;
        Vector3 _force ; 
        for (int i = 0; i < underwaterMesh.UnderwaterTriangles.Count; i++)
        {
            _currentTriangle = underwaterMesh.UnderwaterTriangles[i];

            _force = BuoyancyForce(currentWater.Density, _currentTriangle);

            buoyancyRigidBody.AddForceAtPosition(_force, _currentTriangle.Center);

            //Debug

            //Normal
            Debug.DrawRay(_currentTriangle.Center, _currentTriangle.Normal * 3f, Color.white);

            //Buoyancy
            Debug.DrawRay(_currentTriangle.Center, _force.normalized * -3f, Color.blue);
        }
    }

    private Vector3 BuoyancyForce(float _waterDensity, Triangle _triangle)
    {
        //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

        // F_buoyancy = rho * g * V
        // rho - density of the mediaum you are in
        // g - gravity
        // V - volume of fluid directly above the curved surface 

        // V = z * S * n 
        // z - distance to surface
        // S - surface area
        // n - normal to the surface

        Vector3 _f = _waterDensity * -Physics.gravity.y * _triangle.DistanceToSurface * _triangle.Surface * _triangle.Normal;
        // We have to nullify the forces on horizontal axis
        _f.x = 0;
        _f.z = 0; 
        return _f; 
    }
    #endregion 

    #endregion
}
