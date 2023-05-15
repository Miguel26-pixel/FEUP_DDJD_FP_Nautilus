using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Section
{
    public float minHeight;
    public Color color;
}

internal class ReverseSectionComparer : IComparer<Section>
{
    public int Compare(Section x, Section y)
    {
        return y.minHeight.CompareTo(x.minHeight);
    }
}

public class ColorGenerator : MonoBehaviour {
    public MeshRenderer meshRenderer;
    
    Texture2D texture;
    const int textureResolution = 50;

    public List<Section> sections;
    private List<Vector4> sectionBuffer;
    private Material mat;

    private void Start()
    {
        // sections.Sort(new ReverseSectionComparer());
        sectionBuffer = new List<Vector4>();
        foreach (var section in sections)
        {
            // Debug.Log(new Vector4(section.color.r, section.color.g, section.color.b, section.minHeight));
            sectionBuffer.Add(new Vector4(section.color.r, section.color.g, section.color.b, section.minHeight));
        }
        
        mat = meshRenderer.material;

        // mat.SetBuffer("height_properties", sectionBuffer);
        mat.SetVectorArray("height_properties", sectionBuffer);
        mat.SetInt("height_properties_count", sectionBuffer.Count);
    }

    void Update () {
    }
}
