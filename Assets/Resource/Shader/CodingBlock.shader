Shader "Unlit/CodingBlock"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _OutlineWidth ("Outline Width", Range(0, 50)) = 5.0
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 originalUV : TEXCOORD1;
                float2 objectScale : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollSpeed;
            float4 _Offset;
            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.originalUV = v.uv;
                
                // 获取对象的缩放信息
                float3 worldScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                o.objectScale = worldScale.xy;
                
                // 计算滚动UV，从上往下滚动
                float2 scrollUV = TRANSFORM_TEX(v.uv, _MainTex);
                scrollUV.y += _Time.y * _ScrollSpeed + _Offset.y;
                scrollUV.x += _Offset.x;
                o.uv = scrollUV;
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 使用固定像素宽度的描边
                float2 pixelSize = float2(1.0 / i.objectScale.x, 1.0 / i.objectScale.y) * _OutlineWidth * 0.01;
                
                // 检查是否在描边区域
                float2 uv = i.originalUV;
                float2 distFromEdge = min(uv, 1.0 - uv);
                bool isOutline = distFromEdge.x < pixelSize.x || distFromEdge.y < pixelSize.y;
                
                fixed4 col;
                if (isOutline)
                {
                    col = _OutlineColor;
                }
                else
                {
                    // 采样滚动贴图
                    col = tex2D(_MainTex, i.uv);
                }
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
