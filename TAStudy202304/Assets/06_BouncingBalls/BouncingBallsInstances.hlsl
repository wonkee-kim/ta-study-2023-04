#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    #if defined(SHADER_API_GLCORE) || defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PSSL) || defined(SHADER_API_XBOXONE)
        #include "./Ball.hlsl"
        uniform StructuredBuffer<Ball> _Balls; 
    #endif
#endif

#pragma instancing_options procedural:setup
void setup(){}

void GetInstancedBalls_float(float3 positionOS, out float3 position, out float4 color)
{
    position = positionOS;
    color = (float4)1.0;

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        Ball ball = _Balls[unity_InstanceID];
        position = (positionOS * ball.r) + ball.pos;
        color = ball.c;
    #endif
}