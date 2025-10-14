Shader "Custom/GrassShader"
{
  Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }


        Pass
        {
            Cull off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardBRDF.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;

            

            StructuredBuffer<float4> GrassPositionsBufferShader;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            

              float4 RotateAroundYInDegrees (float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }


            
            
             v2f vert (MeshData v, uint instanceID : SV_INSTANCEID)
            {
                v2f o;

                uint grassIndex = instanceID / 3;
                uint quadIndex  = instanceID % 3;
                  

                  
                float3 localPosition  = (RotateAroundYInDegrees(v.vertex,_Rotation * quadIndex));
                float4 worldPosition = float4(GrassPositionsBufferShader[grassIndex].xyz + localPosition, 1.0f);

                worldPosition.y *= GrassPositionsBufferShader[grassIndex].w;
                
                o.vertex = UnityObjectToClipPos(worldPosition);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex,i.uv);
                clip(-(0.5 - col.a));
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = DotClamped(lightDir, normalize(float3(0, 1, 0)));
                
                return col * ndotl;
            }
            ENDCG
        }
    }
}