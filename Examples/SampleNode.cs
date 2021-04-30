using CZToolKit.GraphProcessor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleNode : BaseNode
{
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public int i;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public long l;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public float f;

    [Port(PortDirection.Input)]
    [HideInInspector]
    public double d;

    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public string s;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Object obj;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public GameObject gameObject;

    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Material mat;

    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Color color;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Vector2 vector2;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Vector2Int vector2Int;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Vector3 vector3;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Vector3Int vector3Int;
    [Port(PortDirection.Input)]
    [ShowAsDrawer]
    [HideInInspector]
    public Vector4 vector4;



    [Port(PortDirection.Output)]
    [HideInInspector]
    public int i1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public long l1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public float f1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public double d1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public string s1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Object obj1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public GameObject gameObject1;

    [Port(PortDirection.Output)]
    [HideInInspector]
    public Material mat1;

    [Port(PortDirection.Output)]
    [HideInInspector]
    public Color color1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Vector2 vector21;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Vector2Int vector2Int1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Vector3 vector31;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Vector3Int vector3Int1;
    [Port(PortDirection.Output)]
    [HideInInspector]
    public Vector4 vector41;
}
