Shader "Unlit/Code rain shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Rain Color", Color) = (0, 1, 0, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _Speed ("Rain Speed",float) = 1.0
        _Density ("Rain Density",float) = 15.0
        _Brightness ("Brightness",float) = 1.5
        _TrailLength ("Trail Length",float) = 0.3
        _Chaos ("Chaos Level",float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _BackgroundColor;
            float _Speed;
            float _Density;
            float _Brightness;
            float _TrailLength;
            float _Chaos;

            // 改进的随机函数
            float random(float2 st) 
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // 多层随机函数
            float noise(float2 st) 
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // 分形噪声
            float fbm(float2 st) 
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 0.0;
                
                for(int i = 0; i < 4; i++) 
                {
                    value += amplitude * noise(st);
                    st *= 2.0;
                    amplitude *= 0.5;
                }
                return value;
            }

            // 改进的字符生成函数
            float char(float2 uv, float charIndex, float localRandom, float size) 
            {
                // 更大的动态字符大小
                float charSize = 3.0 + size * 4.0; // 大幅增加基础大小
                float2 grid = floor(uv * charSize);
                float gridRandom = random(grid + charIndex);
                
                // 创建字符形状
                float2 localUV = frac(uv * charSize);
                localUV = abs(localUV - 0.5);
                
                float shape = 0.0;
                float thickness = 0.08 + localRandom * 0.08; // 增加厚度
                
                // 更真实的ASCII字符形状
                if(gridRandom > 0.95) 
                {
                    // 数字 "8"
                    float dist = length(localUV);
                    shape = step(abs(dist - 0.25), thickness) + step(abs(dist - 0.15), thickness * 0.7);
                    shape = max(shape, step(abs(localUV.y - 0.1), thickness * 0.5) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.9) 
                {
                    // 数字 "0"
                    float dist = length(localUV);
                    shape = step(abs(dist - 0.2), thickness) * (1.0 - step(dist, 0.12));
                }
                else if(gridRandom > 0.85) 
                {
                    // 字母 "A"
                    shape = step(abs(localUV.x), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(abs(localUV.y - 0.15), thickness) * step(localUV.x, 0.2));
                    shape = max(shape, step(abs(localUV.x - localUV.y * 0.6), thickness) * step(localUV.y, 0.25));
                }
                else if(gridRandom > 0.8) 
                {
                    // 数字 "1"
                    shape = step(abs(localUV.x - 0.05), thickness) * step(localUV.y, 0.4);
                    shape = max(shape, step(localUV.y, thickness) * step(localUV.x, 0.15));
                }
                else if(gridRandom > 0.75) 
                {
                    // 字母 "H"
                    shape = step(abs(localUV.x - 0.15), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(abs(localUV.x + 0.15), thickness) * step(localUV.y, 0.35));
                    shape = max(shape, step(abs(localUV.y), thickness * 0.8) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.7) 
                {
                    // 字母 "E"
                    shape = step(abs(localUV.x - 0.15), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(localUV.y, thickness) * step(localUV.x, 0.25));
                    shape = max(shape, step(abs(localUV.y), thickness * 0.8) * step(localUV.x, 0.2));
                    shape = max(shape, step(abs(localUV.y + 0.3), thickness) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.65) 
                {
                    // 字母 "L"
                    shape = step(abs(localUV.x - 0.15), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(abs(localUV.y + 0.3), thickness) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.6) 
                {
                    // 字母 "C"
                    float dist = length(localUV);
                    shape = step(abs(dist - 0.2), thickness) * (1.0 - step(dist, 0.12));
                    shape *= (1.0 - step(-0.1, localUV.x) * step(abs(localUV.y), 0.15));
                }
                else if(gridRandom > 0.55) 
                {
                    // 数字 "3"
                    shape = step(localUV.y, thickness) * step(localUV.x, 0.25);
                    shape = max(shape, step(abs(localUV.y), thickness * 0.8) * step(localUV.x, 0.2));
                    shape = max(shape, step(abs(localUV.y + 0.3), thickness) * step(localUV.x, 0.25));
                    float arc = length(localUV + float2(-0.2, 0.15));
                    shape = max(shape, step(abs(arc - 0.15), thickness) * step(-0.1, localUV.x));
                }
                else if(gridRandom > 0.5) 
                {
                    // 字母 "P"
                    shape = step(abs(localUV.x - 0.15), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(localUV.y, thickness) * step(localUV.x, 0.25));
                    shape = max(shape, step(abs(localUV.y - 0.15), thickness * 0.8) * step(localUV.x, 0.2));
                    float arc = length(localUV + float2(-0.2, -0.075));
                    shape = max(shape, step(abs(arc - 0.1), thickness) * step(-0.1, localUV.x) * step(localUV.y, 0.15));
                }
                else if(gridRandom > 0.45) 
                {
                    // 符号 "+"
                    shape = step(abs(localUV.x), thickness) * step(localUV.y, 0.25);
                    shape = max(shape, step(abs(localUV.y), thickness) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.4) 
                {
                    // 符号 "="
                    shape = step(abs(localUV.y - 0.1), thickness) * step(localUV.x, 0.25);
                    shape = max(shape, step(abs(localUV.y + 0.1), thickness) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.35) 
                {
                    // 符号 "*"
                    shape = step(abs(localUV.x), thickness * 0.7) * step(localUV.y, 0.2);
                    shape = max(shape, step(abs(localUV.y), thickness * 0.7) * step(localUV.x, 0.2));
                    shape = max(shape, step(abs(localUV.x - localUV.y), thickness * 0.7) * step(length(localUV), 0.2));
                    shape = max(shape, step(abs(localUV.x + localUV.y), thickness * 0.7) * step(length(localUV), 0.2));
                }
                else if(gridRandom > 0.3) 
                {
                    // 数字 "2"
                    shape = step(localUV.y, thickness) * step(localUV.x, 0.25);
                    shape = max(shape, step(abs(localUV.y - 0.15), thickness * 0.8) * step(localUV.x, 0.2));
                    shape = max(shape, step(abs(localUV.y + 0.3), thickness) * step(localUV.x, 0.25));
                    float arc1 = length(localUV + float2(-0.2, -0.075));
                    shape = max(shape, step(abs(arc1 - 0.1), thickness) * step(-0.1, localUV.x) * step(localUV.y, 0.075));
                }
                else if(gridRandom > 0.25) 
                {
                    // 字母 "I"
                    shape = step(abs(localUV.x), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(localUV.y, thickness) * step(localUV.x, 0.15));
                    shape = max(shape, step(abs(localUV.y + 0.3), thickness) * step(localUV.x, 0.15));
                }
                else if(gridRandom > 0.2) 
                {
                    // 字母 "T"
                    shape = step(abs(localUV.x), thickness) * step(localUV.y, 0.35);
                    shape = max(shape, step(localUV.y, thickness) * step(localUV.x, 0.25));
                }
                else if(gridRandom > 0.15) 
                {
                    // 符号 "/"
                    shape = step(abs(localUV.x + localUV.y), thickness) * step(localUV.y, 0.35) * step(-localUV.x, 0.35);
                }
                else if(gridRandom > 0.1) 
                {
                    // 符号 "\"
                    shape = step(abs(localUV.x - localUV.y), thickness) * step(localUV.y, 0.35) * step(localUV.x, 0.35);
                }
                else if(gridRandom > 0.05) 
                {
                    // 符号 "."
                    shape = step(length(localUV + float2(0, 0.25)), 0.06 + thickness);
                }
                else 
                {
                    // 符号 ":"
                    shape = step(length(localUV + float2(0, 0.15)), 0.05 + thickness * 0.5);
                    shape = max(shape, step(length(localUV - float2(0, 0.15)), 0.05 + thickness * 0.5));
                }
                
                // 随机消失效果，但减少消失率以保持密度
                float visibility = step(0.02, localRandom);
                
                return shape * visibility;
            }

            // 生成单层代码雨
            float generateLayer(float2 uv, float layerIndex, float time) 
            {
                // 每层有不同的密度和参数
                float layerDensity = _Density * (0.7 + layerIndex * 0.3);
                float stripWidth = 1.0 / (100.0 * layerDensity);
                
                // 添加更多随机偏移
                float2 chaosOffset = float2(
                    fbm(uv * 5.0 + time * 0.1) - 0.5,
                    0
                ) * _Chaos * 0.1;
                
                float2 offsetUV = uv + chaosOffset;
                float stripIndex = floor(offsetUV.x / stripWidth);
                
                // 每个条带的随机参数
                float stripRandom1 = random(float2(stripIndex, layerIndex * 100.0));
                float stripRandom2 = random(float2(stripIndex, layerIndex * 200.0 + 50.0));
                float stripRandom3 = random(float2(stripIndex, layerIndex * 300.0 + 100.0));
                float stripRandom4 = random(float2(stripIndex, layerIndex * 400.0 + 150.0));
                
                // 更复杂的时间和速度变化
                float timeOffset = stripRandom1 * 2000.0;
                float speedMultiplier = 0.3 + stripRandom2 * 2.0;
                float layerTime = time * speedMultiplier + timeOffset;
                
                // 多方向的随机偏移
                float horizontalOffset = (stripRandom1 - 0.5) * stripWidth * 2.0;
                float verticalOffset = (stripRandom2 - 0.5) * 0.1;
                
                // 滚动UV
                float2 scrollUV = offsetUV;
                scrollUV.x += horizontalOffset;
                scrollUV.y += layerTime + verticalOffset;
                scrollUV.y = frac(scrollUV.y);
                
                // 字符生成
                float noiseValue = fbm(float2(stripIndex * 0.07, layerTime * 0.15));
                float charIndex = floor(layerTime * (1.5 + noiseValue * 2.0)) + stripIndex * 7.0 + layerIndex * 50.0;
                float charSize = stripRandom3;
                float character = char(scrollUV, charIndex, stripRandom1, charSize);
                
                // 改进的亮度分布 - 不再基于Y坐标
                // 使用多重噪声创建随机亮区
                float brightnessMask1 = fbm(float2(stripIndex * 0.1, layerTime * 0.2) + uv * 3.0);
                float brightnessMask2 = fbm(float2(stripIndex * 0.3, layerTime * 0.1) + uv * 7.0);
                float brightnessMask3 = noise(uv * 15.0 + time * 0.5);
                
                // 组合多层亮度mask
                float combinedMask = (brightnessMask1 * 0.6 + brightnessMask2 * 0.3 + brightnessMask3 * 0.1);
                
                // 创建随机亮斑
                float brightSpots = fbm(uv * 8.0 + float2(time * 0.3, stripIndex * 0.05));
                brightSpots = step(0.7, brightSpots) * (brightSpots - 0.7) * 3.33; // 只保留高值部分
                
                // 每个条带有不同的亮度模式
                float stripBrightnessPattern = 0.0;
                if(stripRandom4 > 0.7) 
                {
                    // 完全随机亮度
                    stripBrightnessPattern = fbm(uv * 12.0 + layerTime * 0.4);
                }
                else if(stripRandom4 > 0.4) 
                {
                    // 波浪形亮度
                    stripBrightnessPattern = (sin(uv.y * 20.0 + layerTime * 2.0) + 1.0) * 0.5;
                    stripBrightnessPattern = pow(stripBrightnessPattern, 2.0);
                }
                else 
                {
                    // 垂直分段亮度
                    float segment = floor(uv.y * 10.0);
                    stripBrightnessPattern = random(float2(segment, stripIndex)) * 
                                           random(float2(segment + 10.0, layerTime * 0.1));
                }
                
                // 可选的轻微拖尾效果（很弱，不明显）
                float subtleTrail = 1.0;
                if(_TrailLength > 0.5) // 只在TrailLength较大时添加轻微效果
                {
                    float trailNoise = fbm(float2(stripIndex * 0.05, layerTime * 0.1));
                    subtleTrail = 0.7 + 0.3 * (1.0 - pow(uv.y + trailNoise * 0.3, 1.0 / (_TrailLength * 2.0)));
                    subtleTrail = saturate(subtleTrail);
                }
                
                // 综合亮度计算
                float finalBrightness = combinedMask * stripBrightnessPattern * subtleTrail;
                finalBrightness += brightSpots * 0.8; // 添加亮斑
                finalBrightness = saturate(finalBrightness);
                
                // 基础亮度调节
                float baseBrightness = (0.2 + 0.8 * fbm(float2(stripIndex * 0.2, layerTime * 0.3)));
                baseBrightness *= (0.5 + layerIndex * 0.5);
                
                // 最终强度
                float intensity = character * finalBrightness * baseBrightness;
                
                // 自然闪烁
                float flickerNoise = fbm(float2(stripIndex * 0.5, layerTime * 1.5));
                float flicker = 0.6 + 0.4 * flickerNoise;
                intensity *= flicker;
                
                return intensity;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;
                
                // 生成多层代码雨
                float totalIntensity = 0.0;
                
                // 主层 - 最亮
                totalIntensity += generateLayer(uv, 0.0, time) * 1.0;
                
                // 次要层 - 中等亮度，不同速度
                totalIntensity += generateLayer(uv * 1.3, 1.0, time * 0.7) * 0.6;
                
                // 背景层 - 较暗，更慢
                totalIntensity += generateLayer(uv * 0.8, 2.0, time * 1.4) * 0.4;
                
                // 细节层 - 很暗，随机分布
                totalIntensity += generateLayer(uv * 2.1, 3.0, time * 0.3) * 0.2;
                
                // 限制总强度
                totalIntensity = saturate(totalIntensity * _Brightness);
                
                // 混合背景色和代码雨颜色
                fixed4 rainColor = _Color * totalIntensity;
                fixed4 finalColor = lerp(_BackgroundColor, rainColor, totalIntensity);
                finalColor.a = _BackgroundColor.a + totalIntensity * _Color.a;
                
                // 应用雾效
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
