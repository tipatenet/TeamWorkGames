Shader "Custom/LCDScreen_URP"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "black" {}
        _ScanTex ("Scanlines Texture", 2D) = "white" {}

        _BaseColor ("Base Color", Color) = (0,0.05,0.05,1)
        _EmissionColor ("Emission Color", Color) = (0,1,0.6,1)

        _ScanTiling ("Scan Tiling", Float) = 20
        _EmissionStrength ("Emission Strength", Float) = 2
        _FlickerSpeed ("Flicker Speed", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_ScanTex);
            SAMPLER(sampler_ScanTex);

            float4 _BaseColor;
            float4 _EmissionColor;

            float _ScanTiling;
            float _EmissionStrength;
            float _FlickerSpeed;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float4 baseCol = _BaseColor;

                float2 scanUV = uv;
                scanUV.y *= _ScanTiling;

                float scan = SAMPLE_TEXTURE2D(_ScanTex, sampler_ScanTex, scanUV).r;

                float flicker = 0.85 + 0.15 * sin(_Time.y * _FlickerSpeed);

                float3 emission = _EmissionColor.rgb * scan * _EmissionStrength * flicker;

                return float4(baseCol.rgb + emission, 1);
            }

            ENDHLSL
        }
    }
}