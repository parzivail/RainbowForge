#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;
layout(location = 3) in vec3 color;
layout(location = 4) in int objectId;

out vec3 fragPos;
out vec3 fragNormal;
out vec2 fragTexCoord;
out vec3 fragVertColor;
out vec3 reflectedVector;
out vec2 matcapTexCoord;

flat out int fragObjectId;

out mat4 model;
out mat4 view;
out mat4 proj;

uniform mat4 m;
uniform mat4 v;
uniform mat4 p;

void main()
{
    fragNormal = normalize(normal);
    fragTexCoord = texCoord;

    fragVertColor = color;

    fragObjectId = objectId;

    model = m;
    view = v;
    proj = p;

    mat4 MVP = p*v*m;
    gl_Position =  MVP * vec4(position, 1.);
    fragPos = vec3(v * vec4(position, 1.0));
    vec3 v_normal = normalize(vec3(v * (m * vec4(fragNormal, 0.0))));
    matcapTexCoord = v_normal.xy * vec2(0.5, -0.5) + vec2(0.5, 0.5);
    //vec3 viewVector = normalize(world)
}