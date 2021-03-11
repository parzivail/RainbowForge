#version 330 core

uniform vec3 lightPos;
uniform sampler2D texModel;
uniform int isTexture;

in vec3 fragPos;
in vec3 fragNormal;
in vec2 fragTexCoord;

out vec4 color;

void main()
{
    vec4 samp = texture(texModel, fragTexCoord);

    vec3 norm = normalize(fragNormal);
    vec3 lightDir = normalize(lightPos);// light very far away, use direction only. If light has position: normalize(lightPos - fragPos)  
    float diffuse = clamp(dot(norm, lightDir), -1, 1) * 0.3;
    float ambient = 0.7;

    if (isTexture > 0)
        color = mix(vec4(vec3(0.0), 1.0), vec4(samp.rgb, 1.0), samp.a);
    else
        color = vec4(vec3(1.0) * clamp(ambient + diffuse, 0, 1), 1.0);
}