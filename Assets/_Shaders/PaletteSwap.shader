Shader "Custom/PaletteSwap"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color1 ("Color 1 (3f3f3f)", Color) = (1,0,0,1)
        _Color2 ("Color 2 (7f7f7f)", Color) = (0,1,0,1)
        _Color3 ("Color 3 (bfbfbf)", Color) = (0,0,1,1)
        _Color4 ("Color 4 (ffffff)", Color) = (1,1,0,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                
                // 如果是透明像素，直接返回
                if (c.a == 0)
                    return c;
                
                // 获取灰度值（使用red通道，因为是灰度图）
                float gray = c.r;
                
                // 根据灰度值映射到对应颜色
                // 3f3f3f = 0.247 (63/255)
                // 7f7f7f = 0.498 (127/255)  
                // bfbfbf = 0.749 (191/255)
                // ffffff = 1.0 (255/255)
                
                fixed4 finalColor;
                
                if (gray < 0.25) finalColor = _Color1;
                else if (gray < 0.50) finalColor = _Color2;
                else if (gray < 0.75) finalColor = _Color3;
                else finalColor = _Color4;
                
                finalColor.a = c.a; // 保持原始alpha值
                return finalColor * IN.color;
            }
        ENDCG
        }
    }
}