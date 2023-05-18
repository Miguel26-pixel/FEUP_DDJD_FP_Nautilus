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
public struct ColorHeightSection
{
    public float minHeight;
    public List<SectionColor> colors;
}

[Serializable]
public struct BiomeColorSections
{
    public List<ColorHeightSection> sections;
}

internal class ReverseSectionComparer : IComparer<ColorHeightSection>
{
    public int Compare(ColorHeightSection x, ColorHeightSection y)
    {
        return y.minHeight.CompareTo(x.minHeight);
    }
}

public class ColorGenerator : MonoBehaviour {
    public MeshRenderer meshRenderer;

    public List<BiomeColorSections> biomes;
    private List<Vector4> sectionBuffer;
    private Material mat;
    public BiomeProcessingStep biomeProcessingStep;
    
    public void UpdateColors(int seed)
    {
        // sections.Sort(new ReverseSectionComparer());
        sectionBuffer = SerializeSection();

        mat = meshRenderer.material;

        // mat.SetBuffer("height_properties", sectionBuffer);
        mat.SetVectorArray("height_properties", sectionBuffer);
        mat.SetInt("biomes_count", biomes.Count);
        mat.SetFloatArray("biomes_values", biomeProcessingStep.biomesValues);
        mat.SetInt("height_properties_count", sectionBuffer.Count);
        mat.SetFloat("biome_scale", biomeProcessingStep.biomeScale);

        float offsetRange = 1000;

        var prng = new System.Random(seed);
        Vector3 biomeOffset = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;;
        
        mat.SetVector("biome_offset", biomeOffset);
    }

    private List<Vector4> SerializeSection()
    {
        List<Vector4> buffer = new List<Vector4>();

        foreach (var biome in biomes)
        {
            List<Vector4> temp = new List<Vector4>();
            foreach (var section in biome.sections)
            {
                temp.Add(new Vector4(section.minHeight, section.colors.Count));

                foreach (var color in section.colors)
                {
                    temp.Add(new Vector4(color.color.r, color.color.g, color.color.b, color.minNoise));
                }
            }
            buffer.Add(new Vector4(temp.Count, 0));
            buffer.AddRange(temp);
        }

        return buffer;
    }
}
