// Unlit, alpha-blended shader with two effects: hit flash and dissolve.
// Made for SpriteRenderer only: it relies on the per-sprite data (flip, colour) Unity sends to sprite shaders.
// Structure mirrors URP's own Sprite-Unlit-Default shader, with the two effects added.
//
// This file is written to be read as a first HLSL shader: every block is commented.
// The same effects are rebuilt node by node in Docs/Shaders.md with Shader Graph.
Shader "MyUnityPackage/Visual Effect/Sprite Effects"
{
    // Properties are what the Inspector shows and what scripts drive.
    // The name on the left (_FlashAmount...) is the "reference name" used by MaterialFloatEffect.
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Hit Flash)]
        [HDR] _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0

        [Header(Dissolve)]
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveScale ("Noise Scale", Float) = 20
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.3)) = 0.05
        [HDR] _DissolveEdgeColor ("Edge Color", Color) = (2, 0.8, 0.2, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        // Classic transparency: the sprite is blended over what is already on screen,
        // drawn on both faces (flipped sprites) and does not write depth.
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        // Code shared by the two passes below.
        HLSLINCLUDE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Sprite helpers: flip, skinning (2D Animation) and SpriteRenderer.color
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Material properties live in this buffer so the shader is SRP Batcher compatible.
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FlashColor;
                half _FlashAmount;
                half _DissolveAmount;
                float _DissolveScale;
                half _DissolveEdgeWidth;
                half4 _DissolveEdgeColor;
            CBUFFER_END

            // What the mesh gives to the vertex function.
            struct Attributes
            {
                float3 positionOS : POSITION;   // position in object space
                float4 color      : COLOR;      // vertex colour of the sprite mesh
                float2 uv         : TEXCOORD0;
                UNITY_SKINNED_VERTEX_INPUTS
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // What the vertex function passes to the fragment function (interpolated across the triangle).
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // position on screen (clip space)
                float2 uv         : TEXCOORD0;
                float2 noiseUV    : TEXCOORD1;
                half4 color       : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Vertex function: runs once per vertex, places it on screen.
            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                UNITY_SKINNED_VERTEX_COMPUTE(input);

                // Object-space position for the noise: the pattern does not depend on the sprite atlas layout.
                output.noiseUV = input.positionOS.xy * _DissolveScale;

                // SpriteRenderer.flipX / flipY are applied here, not in the mesh
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                // unity_SpriteColor is SpriteRenderer.color
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            // Pseudo-random value in [0, 1] for a 2D cell.
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Value noise: random values on a grid, smoothly interpolated. Returns [0, 1].
            float ValueNoise(float2 uv)
            {
                float2 cell = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f); // smoothstep, hides the grid

                float a = Hash21(cell);
                float b = Hash21(cell + float2(1.0, 0.0));
                float c = Hash21(cell + float2(0.0, 1.0));
                float d = Hash21(cell + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Fragment function: runs once per pixel, returns its colour.
            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // --- Dissolve ---
                // A pixel disappears once the threshold passes its noise value. The threshold is remapped
                // so that Amount = 0 shows everything (no edge) and Amount = 1 hides everything.
                half noise = ValueNoise(input.noiseUV);
                half threshold = _DissolveAmount * (1.0 + _DissolveEdgeWidth) - _DissolveEdgeWidth;
                half visible = step(threshold, noise);
                half edge = visible * step(noise, threshold + _DissolveEdgeWidth) * step(0.0001, _DissolveAmount);

                color.rgb = lerp(color.rgb, _DissolveEdgeColor.rgb, edge);
                color.a *= visible;

                // --- Hit flash ---
                // Replaces the colour but keeps the alpha, so the silhouette flashes.
                color.rgb = lerp(color.rgb, _FlashColor.rgb, _FlashAmount);

                return color;
            }
        ENDHLSL

        // Drawn by the 2D Renderer.
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }

        // Drawn by the Universal Renderer (sprites in a 3D project).
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            ENDHLSL
        }
    }

    Fallback Off
}
