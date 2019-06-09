using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle 
{
    /* Triangle :
	 *
	 *	#####################
	 *	###### PURPOSE ######
	 *	#####################
	 *
	 *	[PURPOSE]
	 *
	 *	#####################
	 *	####### TO DO #######
	 *	#####################
	 *
	 *	[TO DO]
	 *
	 *	#####################
	 *	### MODIFICATIONS ###
	 *	#####################
	 *
	 *	Date :			[DATE]
	 *	Author :		[NAME]
	 *
	 *	Changes :
	 *
	 *	[CHANGES]
	 *
	 *	-----------------------------------
	*/

    #region Fields / Properties
    // Points of the triangle
    public Vector3 PointA { get; private set; }
    public Vector3 PointB { get; private set; }
    public Vector3 PointC { get; private set; }

    //Center of the triangle
    public Vector3 Center { get { return (PointA + PointB + PointC) / 3; } }
    //Normal of the triangle
    public Vector3 Normal { get { return Vector3.Cross(PointB - PointA, PointC - PointA).normalized;  } }

    //Distance from center to surface of the water
    public float DistanceToSurface { get; private set; }

    // Surface of the triangle
    public float Surface { get; private set; }
    #endregion

    #region Constructor
    public Triangle(Vector3 _p1, Vector3 _p2, Vector3 _p3, Water _currentWater)
    {
        PointA = _p1;
        PointB = _p2;
        PointC = _p3;

        Surface = Vector3.Distance(PointA, PointB) * Vector3.Distance(PointA, PointC) * Mathf.Sin(Vector3.Angle(PointB - PointA, PointC - PointA) * Mathf.Deg2Rad) * .5f;

        DistanceToSurface = _currentWater.DistanceTo(Center); 
    }
    #endregion 

}
