Shader "Custom/Terrain"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
<<<<<<< HEAD
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
        #define SHADER_API_D3D11 1
=======

>>>>>>> main
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #include "Assets/Scripts/Compute/Includes/Noise.compute"
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 id;
        };

        uniform float4 height_properties[128];
 
        uniform int height_properties_count;

        uniform int biomes_count;
        uniform float biomes_values[16];

<<<<<<< HEAD
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

=======
>>>>>>> main
        uniform float biome_scale;

        uniform float4 biome_offset;
        uniform float4 init_pos;
        float biomeScale;
        float radius;
        float falloff;
<<<<<<< HEAD
=======
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
>>>>>>> main


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color

            float3 color = half3(0.9, 0.2,0.2);
<<<<<<< HEAD

            int general = 0;
            const float3 biomePos = float3(IN.worldPos.xz * biome_scale , 0) + biome_offset;
=======
            float3 local_pos = IN.worldPos -  mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            
            int general = 0;
            const float3 biomePos = float3(local_pos.xz * biome_scale , 0) + biome_offset;
>>>>>>> main
            float biome_noise = (snoise(biomePos) + 0.866025403785f) / (0.866025403785f*2.f);

            const float3 biome_init_pos = float3(init_pos.xz * biomeScale, 0) + biome_offset;
            const float circle_dist = (biomePos.x - biome_init_pos.x)*(biomePos.x - biome_init_pos.x) +
                (biomePos.y - biome_init_pos.y)*(biomePos.y - biome_init_pos.y) - (radius * radius);

            const float mult_normal = smoothstep(0, falloff, circle_dist);
            const float mult_island = 1 - mult_normal;

            biome_noise = mult_normal * biome_noise + mult_island * 0.9;
<<<<<<< HEAD
=======

            biome_noise += snoise(local_pos*0.05)*0.01;
>>>>>>> main
            
            for (int biome = biomes_count - 1; biome >= 0; biome --)
            {
                const int section_data_size = round(height_properties[general].x);
                general += 1;
                const int target = general + section_data_size;
                while (general < target)
                {
                    float4 s = height_properties[general];
                    const float min_height = s.x;
                    const int num_colors = round(s.y);
                    const int next = general + num_colors + 1;
            
                    float a = biomes_values[biome*2-2];
                    float b = biomes_values[biome*2-1];
            
                    // if (biomeNoise < a && biome > 1)
                    if (biome_noise < a && biome > 0)
                    {
                        general = next;
                        continue;
                    }
            
                    // float multBiomeB = smoothstep(a, b, biomeNoise);
                    // float multBiomeA = 1 - multBiomeB;
                    
<<<<<<< HEAD
                    if (IN.worldPos.y > min_height + snoise(IN.worldPos*0.05)*5)
=======
                    if (local_pos.y > min_height + snoise(local_pos*0.05)*5)
>>>>>>> main
                    {
                        const float noise = (snoise(IN.worldPos*0.03) + 0.866025403785f) / (0.866025403785f*2.f);
                        for (int color_index = general + 1; color_index < next; color_index++)
                        {
                            float4 c = height_properties[color_index];
<<<<<<< HEAD
                            if (noise > c.w) // Min Noise
                            {
=======
                            // float4 c_prev = height_properties[color_index-1];
                            if (noise > c.w) // Min Noise
                            {
                                // noise = smoothstep(c_prev.w, c.w, noise);
                                // color = saturate(lerp(c_prev, c, noise)); //float3(c.x, c.y, c.z);
>>>>>>> main
                                color = float3(c.x, c.y, c.z);
                        
                                break;
                            }
                        }
<<<<<<< HEAD
=======
                        // color_index = general + num_colors + 1; 
                        //
                        // for(int normal_index = color_index; normal_index < next - 1; normal_index++)
                        // {
                        //     float4 n = height_properties[normal_index];
                        //     float4 n_next = height_properties[normal_index+1];
                        //     float normal = saturate(IN.worldNormal.y);
                        //     if (normal > n.w) // Min Noise
                        //     {
                        //         normal = smoothstep(n.w, n_next.w, normal);
                        //         color =  saturate(color + lerp(n, n_next, normal));
                        //         break;
                        //     }
                        // }
>>>>>>> main
            
                        break;
                    }
            
                    general = next;
                }
            }

            // color = biomeNoise;


            o.Albedo = color;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
