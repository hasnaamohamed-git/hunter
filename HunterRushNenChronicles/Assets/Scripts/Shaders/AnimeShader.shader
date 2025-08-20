Shader "HunterRush/AnimeCharacter"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.7,0.7,0.8,1)
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0,1)) = 0.1
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0,10)) = 3
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.01
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AuraColor ("Aura Color", Color) = (1,1,1,0)
        _AuraIntensity ("Aura Intensity", Range(0,2)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 200
        
        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            float _OutlineWidth;
            fixed4 _OutlineColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // Expand vertex along normal for outline
                float3 norm = normalize(v.normal);
                float3 outlinePos = v.vertex.xyz + norm * _OutlineWidth;
                
                o.pos = UnityObjectToClipPos(float4(outlinePos, 1.0));
                o.color = _OutlineColor;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
        
        // Main Rendering Pass
        Pass
        {
            Name "Main"
            Tags { "LightMode"="ForwardBase" }
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                LIGHTING_COORDS(4,5)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSoftness;
            fixed4 _RimColor;
            float _RimPower;
            fixed4 _AuraColor;
            float _AuraIntensity;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample main texture
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * _Color;
                
                // Calculate lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(i.worldNormal, lightDir);
                
                // Toon shading
                float lightIntensity = smoothstep(_ShadowThreshold - _ShadowSoftness, _ShadowThreshold + _ShadowSoftness, NdotL);
                fixed4 lightColor = lerp(_ShadowColor, fixed4(1,1,1,1), lightIntensity);
                
                // Apply lighting
                col.rgb *= lightColor.rgb;
                
                // Rim lighting
                float rimDot = 1 - dot(i.viewDir, i.worldNormal);
                float rimIntensity = pow(rimDot, _RimPower);
                col.rgb += _RimColor.rgb * rimIntensity;
                
                // Aura effect
                if (_AuraIntensity > 0)
                {
                    float auraRim = pow(rimDot, 2);
                    col.rgb += _AuraColor.rgb * auraRim * _AuraIntensity;
                }
                
                // Apply shadows
                float attenuation = LIGHT_ATTENUATION(i);
                col.rgb *= attenuation;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Diffuse"
}