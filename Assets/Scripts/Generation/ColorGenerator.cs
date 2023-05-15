using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct SectionColor
{
    public Color color;
    public float minNoise;
}

[Serializable]
public struct Section
{
    public float minHeight;
    public List<SectionColor> colors;
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
            sectionBuffer.Add(new Vector4(section.minHeight, section.colors.Count));

            foreach (var color in section.colors)
            {
                sectionBuffer.Add(new Vector4(color.color.r, color.color.g, color.color.b, color.minNoise));
            }
        }
        
        mat = meshRenderer.material;

        // mat.SetBuffer("height_properties", sectionBuffer);
        mat.SetVectorArray("height_properties", sectionBuffer);
        mat.SetInt("height_properties_count", sectionBuffer.Count);
    }

    void Update () {
    }
}
