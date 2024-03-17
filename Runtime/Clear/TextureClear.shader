Shader "Hidden/Prebut/TextureClear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Rotation ("Rotation", Float) = 0
        _Scale ("Scale", Vector) = (1, 1, 0, 0)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _ScrollAngle ("Scroll Angle", Float) = 0
        _ScrollSpeed ("Scroll Speed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Rotation;
            float2 _Scale;
            fixed4 _TintColor;
            float _ScrollAngle;
            float _ScrollSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                _ScrollAngle = 3.14159265359 / 4;
                _ScrollSpeed = 0.1;
                float2 uv = i.uv;

                uv -= 0.5;
                uv = float2(uv.x * _Scale.x, uv.y * _Scale.y);
                float2 offset = float2(
                    sin(_ScrollAngle),
                    cos(_ScrollAngle))
                * _ScrollSpeed
                * _Time.y;
                uv += offset;
                float2x2 rot = float2x2(cos(_Rotation), -sin(_Rotation), sin(_Rotation), cos(_Rotation));
                uv = mul(rot, uv);
                uv += 0.5;

                fixed4 col = tex2D(_MainTex, uv) * _TintColor;
                
                return col;
            }
            ENDCG
        }
    }
}
