Shader "Custom/PaletteSprite_Fixed" {
    Properties {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _TeamPalette("Team Palette", 2D) = "white" {}
//        _TeamPalette_TexelSize("Team Palette TexelSize", Vector) = (0,0,0,0)
        _TeamColor("Team Color Row", Int) = 0
        _SkinPalette("Skin Palette", 2D) = "white" {}
//        _SkinPalette_TexelSize("Skin Palette TexelSize", Vector) = (0,0,0,0)
        _SkinColor("Skin Color Row", Int) = 0
        _ColorTolerance("Color Match Tolerance", Range(0.001, 0.05)) = 0.01
    }
    
    SubShader {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Off 
        ZWrite Off 
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _TeamPalette;
            sampler2D _SkinPalette;
            float4 _TeamPalette_TexelSize;
            float4 _SkinPalette_TexelSize;
            int _TeamColor;
            int _SkinColor;
            float _ColorTolerance;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // 支持SpriteRenderer的颜色
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float2 PxToUV(int x, int y, float width, float height) {
                //Godot：纹理原点在左上（UV.y 向下）
                // Unity：纹理原点在左下（UV.y 向上）
                return float2(
                    (x + 0.5) / width,
                    1.0 - (y + 0.5) / height
                );
            }

            bool ColorMatch(float3 a, float3 b, float tolerance) {
                return all(abs(a - b) < tolerance);
            }

            float4 ReplaceByPalette(
                float4 color,
                sampler2D palette,
                float4 texelSize,
                int targetRow,
                float tolerance
            ) {
                int width = (int)texelSize.z;
                int height = (int)texelSize.w;
                
                // 确保targetRow在有效范围内
                targetRow = clamp(targetRow, 0, height - 1);

                [unroll(64)]
                for (int col = 0; col < width; col++) {
                    float2 uvRef = PxToUV(col, 0, width, height);
                    float4 refColor = tex2Dlod(palette, float4(uvRef, 0, 0));
                    
                    if (ColorMatch(refColor.rgb, color.rgb, tolerance)) {
                        float2 uvDst = PxToUV(col, targetRow, width, height);
                        float4 newColor = tex2Dlod(palette, float4(uvDst, 0, 0));
                        newColor.a = color.a;
                        return newColor;
                    }
                }
                return color;
            }

            float4 frag(v2f i) : SV_Target {
                float4 col = tex2D(_MainTex, i.uv);
                
                if (col.a < 0.01) 
                    return col;

                // Team 替换
                col = ReplaceByPalette(col, _TeamPalette, _TeamPalette_TexelSize, 
                                       _TeamColor, _ColorTolerance);

                // Skin 替换
                if (_SkinColor > 0) {
                    col = ReplaceByPalette(col, _SkinPalette, _SkinPalette_TexelSize, 
                                           _SkinColor, _ColorTolerance);
                }

                // 应用SpriteRenderer的颜色
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}