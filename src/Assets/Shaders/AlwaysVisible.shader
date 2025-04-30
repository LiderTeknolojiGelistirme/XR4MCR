Shader "Custom/AlwaysVisible"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            ZWrite Off
            ZTest Always
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            Material
            {
                Diffuse [_Color]
                Ambient [_Color]
            }
            Lighting Off
            SetTexture [_MainTex]
            {
                combine texture * primary
            }
        }
    }
}
