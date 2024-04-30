using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class is a helper class which stores a 3D Rect
 * which behaves in the same way as Unity's 2D Rect (but with
 * a z dimenion and depth)
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */
[System.Serializable]
public class Rect3D
{
    // The dimensions of the Rect3D
    public float xMin, xMax, yMin, yMax, zMin, zMax;

    // A contructor to create a Rect3D with min and max values for the
    // three dimensions (x, y, z)
    public Rect3D(float _xMin, float _yMin, float _zMin, float _xWidth, float _yWidth, float _zWidth)
    {
        xMin = _xMin; xMax = _xMin + _xWidth;
        yMin = _yMin; yMax = _yMin + _yWidth;
        zMin = _zMin; zMax = _zMin + _zWidth;
    }

    // Some helper getter functions which align with the Rect functions

    // x = xMin
    public float x
    {
        get
        {
            return xMin;
        }
    }

    // y = yMin
    public float y
    {
        get
        {
            return yMin;
        }
    }

    // z = zMin
    public float z
    {
        get
        {
            return zMin;
        }
    }

    // width = xMax - xMin
    public float width
    {
        get
        {
            return xMax - xMin;
        }
    }

    // height = yMax - yMin
    public float height
    {
        get
        {
            return yMax - yMin;
        }
    }

    // depth = zMax - zMin
    public float depth
    {
        get
        {
            return zMax - zMin;
        }
    }
};