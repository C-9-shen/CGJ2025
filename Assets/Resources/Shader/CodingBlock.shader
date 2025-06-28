Shader "Unlit/CodingBlock"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _OutlineWidth ("Outline Width", Range(0, 50)) = 5.0
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        
        [Header(Color Control)]
        _GrayscaleAmount ("Grayscale Amount", Range(0, 1)) = 0
        
        [Header(Jelly Animation)]
        _JellyStrength ("Jelly Strength", Range(0, 0.1)) = 0.02
        _WaveSpeed ("Wave Speed", Range(0.1, 5.0)) = 1.5
        _WaveFrequency ("Wave Frequency", Range(0.1, 10.0)) = 4.0
        _PulseSpeed ("Pulse Speed", Range(0.1, 3.0)) = 1.0
        _PulseAmount ("Pulse Amount", Range(0, 0.05)) = 0.01
        _EdgeWaveIntensity ("Edge Wave Intensity", Range(0, 2.0)) = 1.0
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
                float jellyFactor : TEXCOORD3;
                UNITY_FOG_COORDS(4)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollSpeed;
            float4 _Offset;
            float _OutlineWidth;
            fixed4 _OutlineColor;
            
            // Color control
            float _GrayscaleAmount;
            
            // Jelly animation properties
            float _JellyStrength;
            float _WaveSpeed;
            float _WaveFrequency;
            float _PulseSpeed;
            float _PulseAmount;
            float _EdgeWaveIntensity;

            // Smooth noise function for organic movement
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Convert color to grayscale using Unity's standard formula
            float calculateGrayscale(fixed4 color)
            {
                return (0.299 * color.r) + (0.587 * color.g) + (0.114 * color.b);
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                // 原始顶点位置
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                
                // 计算距离中心的位置，用于边缘检测
                float2 centeredUV = v.uv - 0.5;
                float distFromCenter = length(centeredUV);
                float edgeFactor = smoothstep(0.3, 0.5, distFromCenter);
                
                // === 蔚蓝风格果冻动画效果 ===
                
                // 1. 全局脉冲效果 - 周期性整体缩放
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmount;
                float3 localPos = v.vertex.xyz * (1.0 + pulse);
                
                // 2. 波浪效果 - 基于UV坐标的正弦波
                float wave1 = sin(_Time.y * _WaveSpeed + v.uv.x * _WaveFrequency * 3.14159) * _JellyStrength;
                float wave2 = cos(_Time.y * _WaveSpeed * 0.7 + v.uv.y * _WaveFrequency * 3.14159) * _JellyStrength * 0.8;
                
                // 3. 边缘强化波动 - 边缘的波动更明显
                float edgeWave = sin(_Time.y * _WaveSpeed * 1.3 + distFromCenter * 6.28) * _JellyStrength * _EdgeWaveIntensity * edgeFactor;
                
                // 4. 有机噪声效果 - 添加轻微的随机运动
                float2 noiseInput = worldPos.xy * 2.0 + _Time.y * 0.5;
                float noiseX = (smoothNoise(noiseInput) - 0.5) * _JellyStrength * 0.5;
                float noiseY = (smoothNoise(noiseInput + float2(100.0, 0.0)) - 0.5) * _JellyStrength * 0.5;
                
                // 5. 果冻弹性效果 - 基于时间的二次方波
                float bounce = sin(_Time.y * _WaveSpeed * 2.0) * cos(_Time.y * _WaveSpeed * 1.7);
                bounce = bounce * bounce * _JellyStrength * 0.3;
                
                // 综合所有效果
                localPos.x += wave1 + noiseX + centeredUV.x * bounce;
                localPos.y += wave2 + noiseY + centeredUV.y * bounce;
                localPos.z += edgeWave;
                
                // 转换到裁剪空间
                o.vertex = UnityObjectToClipPos(float4(localPos, 1.0));
                o.originalUV = v.uv;
                
                // 存储果冻强度因子，用于片段着色器中的动态描边
                o.jellyFactor = 1.0 + (abs(wave1) + abs(wave2) + abs(edgeWave)) * 2.0;
                
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
                // 动态描边宽度 - 根据果冻效果调整
                float dynamicOutlineWidth = _OutlineWidth * i.jellyFactor;
                
                // 使用固定像素宽度的描边
                float2 pixelSize = float2(1.0 / i.objectScale.x, 1.0 / i.objectScale.y) * dynamicOutlineWidth * 0.01;
                
                // 检查是否在描边区域
                float2 uv = i.originalUV;
                float2 distFromEdge = min(uv, 1.0 - uv);
                bool isOutline = distFromEdge.x < pixelSize.x || distFromEdge.y < pixelSize.y;
                
                fixed4 col;
                if (isOutline)
                {
                    // 动态描边颜色 - 添加轻微的亮度变化
                    float brightness = 1.0 + sin(_Time.y * _PulseSpeed * 2.0) * 0.1;
                    col = _OutlineColor * brightness;
                    
                    // 对描边也应用灰度效果
                    if (_GrayscaleAmount > 0)
                    {
                        float grayValue = calculateGrayscale(col);
                        fixed4 grayColor = fixed4(grayValue, grayValue, grayValue, col.a);
                        col = lerp(col, grayColor, _GrayscaleAmount);
                    }
                }
                else
                {
                    // 采样滚动贴图
                    col = tex2D(_MainTex, i.uv);
                    
                    // 应用灰度效果
                    if (_GrayscaleAmount > 0)
                    {
                        float grayValue = calculateGrayscale(col);
                        fixed4 grayColor = fixed4(grayValue, grayValue, grayValue, col.a);
                        col = lerp(col, grayColor, _GrayscaleAmount);
                    }
                }
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
