#version 330 core

uniform sampler2D texModel;
uniform sampler2D texReflection;
uniform sampler2D texUv;
uniform sampler2D texRadiance;

uniform vec3 lightPos;
uniform vec3 lightColor;

in vec3 fragPos;
in vec3 fragNormal;
in vec3 fragVertColor;
in vec2 fragTexCoord;
in vec2 matcapTexCoord;
flat in int fragObjectId;

out vec4 color;

#define NUM_COLOR_LOOKUPS 8
vec3 COLOR_LOOKUP[NUM_COLOR_LOOKUPS] = vec3[NUM_COLOR_LOOKUPS](
vec3(0.85, 0.32, 0.30),
vec3(0.36, 0.75, 0.87),
vec3(0.36, 0.72, 0.36),
vec3(0.26, 0.55, 0.79),
vec3(1.00, 0.75, 0.14),
vec3(0.95, 0.46, 0.20),
vec3(0.89, 0.12, 0.91),
vec3(0.28, 0.56, 0.56)
);

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

void main()
{
    float specularStrength = 0.5;
    float specularIntensity = 0.5;
    vec4 radianceSamp = texture(texRadiance, matcapTexCoord);
    vec4 checkerSamp = texture(texUv, fragTexCoord);
    vec4 reflectionSamp = texture(texReflection, matcapTexCoord);
    vec3 norm = normalize(fragNormal);

    vec3 lightDir = normalize(lightPos);// light very far away, use direction only. If light has position: normalize(lightPos - fragPos)  
    float diffuse = clamp(dot(norm, lightDir), -1, 1) * 0.3;
    float ambient = 0.7;

    //phong r dot v ^ strength
    // r - light off normal

    vec3 appliedColor = fragVertColor;

    appliedColor = COLOR_LOOKUP[fragObjectId % NUM_COLOR_LOOKUPS];

    vec3 specR = reflect(lightDir, norm);
    float nDotR = dot(specR, normalize(fragPos));
    vec3 coloredSpec = clamp(pow(nDotR, pow(specularStrength + 1, 8)) * lightColor * specularIntensity, 0, 1);
    vec4 beforeMatCap = mix(checkerSamp, vec4(appliedColor, 1.0), 0.5) * vec4(vec3(clamp(ambient + diffuse, 0, 1)), 1);
    vec4 beforeReflection = mix(beforeMatCap, radianceSamp * beforeMatCap, 0.75);

    color = mix(beforeReflection, beforeReflection * reflectionSamp, 0.3);
    //color = samp;

}
