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
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
        #define SHADER_API_D3D11 1
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
        };

        struct Section
        {
            float minHeight;
            float3 color;
        };
        uniform float4 height_properties[16];
 
        uniform int height_properties_count;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color

            float3 color = half3(0.9, 0.2,0.2);

            for (int i = 0; i < height_properties_count;)
            {
                float4 s = height_properties[i];
                const float min_height = s.x;
                const int num_colors = round(s.y);
                const int next = i + num_colors + 1;
                

                if (IN.worldPos.y > min_height + snoise(IN.worldPos*0.05)*5)
                {
                    const float noise = (snoise(IN.worldPos*0.03) + 0.866025403785f) / (0.866025403785f*2.f);
                    for (int color_index = i + 1; color_index < next; color_index++)
                    {
                        float4 c = height_properties[color_index];
                        if (noise > c.w) // Min Noise
                        {
                            color = float3(c.x, c.y, c.z);
                    
                            break;
                        }
                    }

                    break;
                }

                i = next;
            }

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
