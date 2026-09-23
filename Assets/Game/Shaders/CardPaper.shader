Shader "EverlightTales/UI/CardPaper"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _CardColor ("Card Paper Color", Color) = (0.95, 0.92, 0.84, 1)
        _CardWidth ("Card Width (px)", Range(0, 40)) = 10
        _ContentScale ("Content Scale", Range(0.5, 1.0)) = 0.95
        _PaperGrain ("Paper Grain", Range(0, 0.3)) = 0.08
        _PaperScale ("Paper Grain Scale", Range(1, 512)) = 96
        [NoScaleOffset] _PaperTex ("Paper Noise Texture", 2D) = "black" {}
        [Toggle(_USEPAPERTEX)] _UsePaperTex ("Use Paper Texture", Float) = 0

        _ShadowColor ("Shadow Color", Color) = (0.04, 0.05, 0.08, 0.45)
        _ShadowOffsetX ("Shadow Offset X (px)", Float) = 5
        _ShadowOffsetY ("Shadow Offset Y (px)", Float) = -5
        _ShadowSoft ("Shadow Soft (px)", Float) = 4
        _AlphaThreshold ("Alpha Threshold (ignore edge noise)", Range(0, 0.5)) = 0.24

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile _ _USEPAPERTEX

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;

            fixed4 _CardColor;
            float _CardWidth;
            float _ContentScale;
            float _PaperGrain;
            float _PaperScale;
            sampler2D _PaperTex;
            float4 _PaperTex_TexelSize;
            fixed4 _ShadowColor;
            float _ShadowOffsetX;
            float _ShadowOffsetY;
            float _ShadowSoft;
            float _AlphaThreshold;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 外扩采样：在 uv 处向 24 个方向取 max alpha（dilation），半径 radiusPx 像素。
            half Dilate(half2 uv, half radiusPx)
            {
                half2 texel = _MainTex_TexelSize.xy;
                half m = tex2D(_MainTex, uv).a;
                [unroll]
                for (int i = 0; i < 24; i++)
                {
                    half ang = (half)i * (6.28318530718 / 24.0);
                    half2 dir = half2(cos(ang), sin(ang));
                    m = max(m, tex2D(_MainTex, uv + dir * radiusPx * texel).a);
                }
                return m;
            }

            half PaperNoise(half2 uv)
            {
                return frac(sin(dot(floor(uv), half2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half2 uv = IN.texcoord;
                half2 texel = _MainTex_TexelSize.xy;

                // 内容 UV 内缩：美术 PNG 内容包围盒常占满纹理（边缘留白≈0），
                // 卡纸外扩/投影会采样到 UV 边界外被 clamp 截断成矩形方框。
                // _ContentScale 让内容围绕中心缩小，给外扩留出空间（1.0=不缩，越小内容越小、外扩空间越大）。
                half2 contentUV = (uv - 0.5) / _ContentScale + 0.5;

                // 内容：手动预乘。美术 PNG 是非预乘 alpha（半透明边缘 RGB 满强度），
                // 若按 UI/Default 直接乘顶点色，alpha≈0 的边缘像素会把满强度颜色泄漏到卡纸/投影。
                fixed4 texCol = tex2D(_MainTex, contentUV) + _TextureSampleAdd;
                half contentA = texCol.a * IN.color.a;
                half3 contentRGB = texCol.rgb * texCol.a * IN.color.rgb;

                // 卡纸外扩
                half cardA = Dilate(contentUV, _CardWidth);
                half cardMask = smoothstep(_AlphaThreshold, _AlphaThreshold + max(_CardWidth * texel.x, 0.0001), cardA);

                // 投影：向偏移方向取外扩（软投影）
                half2 shadowOffset = half2(_ShadowOffsetX * texel.x, _ShadowOffsetY * texel.y);
                half shadowA = Dilate(contentUV - shadowOffset, _CardWidth);
                half shadowMask = smoothstep(_AlphaThreshold, _AlphaThreshold + max(_ShadowSoft * texel.x, 0.0001), shadowA);

                // 卡纸颜色：米白 + 纸纹颗粒（有噪声图用噪声图，否则用程序化 hash）
                #ifdef _USEPAPERTEX
                    half grain = tex2D(_PaperTex, uv * _PaperScale).r;
                #else
                    half grain = PaperNoise(uv * _PaperScale);
                #endif
                half3 cardCol = _CardColor.rgb * (1.0 - grain * _PaperGrain);

                // 从底到顶预乘合成：投影 → 卡纸 → 内容
                half3 rgb = _ShadowColor.rgb * (shadowMask * _ShadowColor.a);
                half a = shadowMask * _ShadowColor.a;

                rgb = cardCol * cardMask + rgb * (1.0 - cardMask);
                a = cardMask + a * (1.0 - cardMask);

                rgb = contentRGB + rgb * (1.0 - contentA);
                a = contentA + a * (1.0 - contentA);

                fixed4 col = fixed4(rgb, a);

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
    CustomEditor "Everlight.Tales.UI.EditorTools.CardPaperGUI"
}
