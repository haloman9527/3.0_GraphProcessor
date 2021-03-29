using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortSizeAttribute : Attribute
{
    public int size;
    public PortSizeAttribute(int _size)
    {
        size = _size;
    }
}
