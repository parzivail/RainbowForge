#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2DMS texScene;
uniform int width;
uniform int height;
uniform int samplesScene;

vec4 mtexture(sampler2DMS s, vec2 coords, int samp)
{
    ivec2 vpCoords = ivec2(width, height);
    vpCoords.x = int(vpCoords.x * coords.x);
    vpCoords.y = int(vpCoords.y * coords.y);

    vec4 avg = vec4(0);
    for (int i = 0; i < samp; i++)
    {
        avg += texelFetch(s, vpCoords, i);
    }
    return avg / float(samp);
}

float linearDepth(float depthSample)
{
    const float zNear = 1;
    const float zFar = 1024;
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depthSample * (zFar - zNear));
    return zLinear;
}

void main()
{
    vec4 color = mtexture(texScene, TexCoords, samplesScene);
    FragColor = color;//vec4(TexCoords.x, TexCoords.y, 0., 1.);
}