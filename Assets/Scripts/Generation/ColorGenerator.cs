using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

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

public class ColorGenerator : MonoBehaviour
{
    public List<BiomeColorSections> biomes;
    public BiomeProcessingStep biomeProcessingStep;
    private List<Vector4> sectionBuffer;

    public void UpdateColors(int seed, Material mat)
    {
        // sections.Sort(new ReverseSectionComparer());
        sectionBuffer = SerializeSection();

        // mat.SetBuffer("height_properties", sectionBuffer);
        mat.SetVectorArray("height_properties", sectionBuffer);
        mat.SetInt("biomes_count", biomes.Count);
        mat.SetFloatArray("biomes_values", biomeProcessingStep.biomesValues);
        mat.SetInt("height_properties_count", sectionBuffer.Count);
        mat.SetFloat("biome_scale", biomeProcessingStep.biomeScale);
        mat.SetFloat("falloff", biomeProcessingStep.islandFalloff);
        mat.SetFloat("radius", biomeProcessingStep.islandRadius);
        mat.SetVector("init_pos", biomeProcessingStep.initPos);

        float offsetRange = 1000;

        Random prng = new Random(seed);
        Vector3 biomeOffset = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1,
            (float)prng.NextDouble() * 2 - 1) * offsetRange;
        ;

        mat.SetVector("biome_offset", biomeOffset);
    }

    private List<Vector4> SerializeSection()
    {
        List<Vector4> buffer = new();

        foreach (BiomeColorSections biome in biomes)
        {
            List<Vector4> temp = new();
            foreach (ColorHeightSection section in biome.sections)
            {
                temp.Add(new Vector4(section.minHeight, section.colors.Count));

                foreach (SectionColor color in section.colors)
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