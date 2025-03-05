Shader "Custom/OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.05
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "Queue"="Overlay" }
        
        Pass
        {
            Name "OUTLINE"
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            
            // Set the outline shader parameters
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
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float _OutlineWidth;
            fixed4 _OutlineColor;
            sampler2D _MainTex;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // Displace the vertices to create the outline effect
                o.pos.xy += float2(_OutlineWidth, _OutlineWidth);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                // If the pixel is transparent in the main texture, we discard it
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < 0.1)
                    discard;
                
                return _OutlineColor; // Color of the outline
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
