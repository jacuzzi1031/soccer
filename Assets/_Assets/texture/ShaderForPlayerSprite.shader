Shader "Custom/PaletteSprite_GodotStyle"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _TeamPalette("Team Palette", 2D) = "white" {}
        _SkinPalette("Skin Palette", 2D) = "white" {}

        _TeamColor("Team Color Row", Float) = 0
        _SkinColor("Skin Color Row", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _TeamPalette;
            sampler2D _SkinPalette;
            float4 _TeamPalette_TexelSize;
            float4 _SkinPalette_TexelSize;
            float _TeamColor;
            float _SkinColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 PxToUV(int x, int y, float4 texelSize)
            {
                return float2((x + 0.5) * texelSize.x, (y + 0.5) * texelSize.y);
            }

            float4 SamplePalette(float4 color, sampler2D palette, float4 texelSize, float rowIndex)
            {
                int width = (int)texelSize.z;
                int height = (int)texelSize.w;

                // 忽略透明像素
                if(color.a < 0.01)
                    return color;
                [unroll(100)]
                for(int x = 0; x < width; x++)
                {
                    float2 uvRef = PxToUV(x, 0, texelSize);
                    float4 refColor = tex2D(palette, uvRef);

                    // RGB 容差匹配
                    if(abs(color.r - refColor.r) < 0.02 &&
                       abs(color.g - refColor.g) < 0.02 &&
                       abs(color.b - refColor.b) < 0.02)
                    {
                        float2 uvDest = PxToUV(x, rowIndex, texelSize);
                        float4 newColor = tex2D(palette, uvDest);
                        newColor.a = color.a; // 保留 alpha
                        return newColor;
                    }
                }

                return color; // 没匹配到保持原色
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                // 替换队色
                col = SamplePalette(col, _TeamPalette, _TeamPalette_TexelSize, _TeamColor);

                // 替换皮肤色
                if(_SkinColor > 0)
                    col = SamplePalette(col, _SkinPalette, _SkinPalette_TexelSize, _SkinColor);

                return col;
            }

            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
