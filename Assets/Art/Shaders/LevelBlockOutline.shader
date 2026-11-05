Shader "Custom/BlockOutline"
{
    Properties
    {
        _Color ("Block Color", Color) = (0,1,0,1)
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _BorderSize ("Border Size", Range(0.001, 0.2)) = 0.03
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
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
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _BorderColor;
            float _BorderSize;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;
                o.normal = v.normal;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 p = abs(i.localPos);

                float distanceToEdge;

                // Cara X
                if (abs(i.normal.x) > 0.5)
                {
                    distanceToEdge = min(
                        0.5 - p.y,
                        0.5 - p.z
                    );
                }
                // Cara Y
                else if (abs(i.normal.y) > 0.5)
                {
                    distanceToEdge = min(
                        0.5 - p.x,
                        0.5 - p.z
                    );
                }
                // Cara Z
                else
                {
                    distanceToEdge = min(
                        0.5 - p.x,
                        0.5 - p.y
                    );
                }

                if (distanceToEdge < _BorderSize)
                {
                    return _BorderColor;
                }

                return _Color;
            }

            ENDCG
        }
    }
}