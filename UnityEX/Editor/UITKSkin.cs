using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Graph Processor/UITK Skin", fileName = "new skin")]
public class UITKSkin : ScriptableObject
{
    [Serializable]
    public class StyleSheetPair
    {
        public string name;
        public StyleSheet styleSheet;
    }
    
    public List<UITKSkin> otherSkins = new List<UITKSkin>();
    
    public List<StyleSheetPair> styleSheets = new List<StyleSheetPair>();
}
