#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleGraphTest : MonoBehaviour
{
    public SampleGraph graph;

    // Start is called before the first frame update
    void Start()
    {
        graph = new SampleGraph();

        var nodeA = graph.AddNode<SampleNode>(Vector2.down * 100);
        var nodeB = graph.AddNode<SampleNode>(Vector2.down * 200);

        graph.Connect(nodeA, "Output", nodeB, "Input");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Jump"))
        {
            CZToolKit.GraphProcessor.Editors.BaseGraphWindow.Open(graph);
        }
    }
}
