#version 330 core

uniform sampler2D texModel;
uniform sampler2D checkerTex;
uniform vec3 lightPos;
uniform vec3 colorIn;
uniform vec3 pos;
uniform vec3 lightColor;
uniform sampler2D radianceTex;

in vec3 fragPos;
in vec3 fragNormal;
in vec2 fragTexCoord;
in vec2 matcapTexCoord;


out vec4 color;

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

void main()
{
    float specularStrength = 0.5;
    float specularIntensity = 0.5;
    vec4 radianceSamp = texture(radianceTex, matcapTexCoord);
    vec4 checkerSamp = texture(checkerTex, fragTexCoord);
    vec4 samp = texture(texModel, matcapTexCoord);
    vec3 norm = normalize(fragNormal);
      
    vec3 lightDir = normalize(lightPos);// light very far away, use direction only. If light has position: normalize(lightPos - fragPos)  
    float diffuse = clamp(dot(norm, lightDir), -1, 1) * 0.3;
    float ambient = 0.7;

    //phong r dot v ^ strength
    // r - light off normal

    vec3 specR = reflect(lightDir, norm);
    float nDotR = dot(specR, normalize(fragPos));
    vec3 coloredSpec = clamp(pow(nDotR,pow(specularStrength+1,8))*lightColor*specularIntensity,0,1);
    vec4 beforeMatCap = (clamp(checkerSamp+vec4(vec3(0.75),1.0),0,1))*vec4(colorIn*clamp(ambient + diffuse, 0, 1)+coloredSpec,1.0);
    vec4 beforeReflection = mix(beforeMatCap,radianceSamp*beforeMatCap,0.75);
    
    color = mix(beforeReflection,beforeReflection*samp,0.2);
    //color = samp;
    
}
