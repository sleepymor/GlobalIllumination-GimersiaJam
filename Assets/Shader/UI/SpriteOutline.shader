Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0, 5)) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane" 
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
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _AlphaThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a;

                if (alpha < _AlphaThreshold)
                {
                    // sample neighboring pixels for outline detection
                    float outline = 0.0;
                    float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                    // 8-direction sample
                    outline += tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a;
                    outline += tex2D(_MainTex, i.uv + float2(offset.x, 0)).a;
                    outline += tex2D(_MainTex, i.uv + float2(0, -offset.y)).a;
                    outline += tex2D(_MainTex, i.uv + float2(0, offset.y)).a;
                    outline += tex2D(_MainTex, i.uv + float2(-offset.x, -offset.y)).a;
                    outline += tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)).a;
                    outline += tex2D(_MainTex, i.uv + float2(-offset.x, offset.y)).a;
                    outline += tex2D(_MainTex, i.uv + float2(offset.x, offset.y)).a;

                    if (outline > 0)
                        return _OutlineColor;
                }

                return col;
            }
            ENDCG
        }
    }
}
