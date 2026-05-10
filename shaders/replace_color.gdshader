shader_type canvas_item;

uniform vec4 team_color : source_color;
uniform vec4 team_colorB : source_color;
uniform vec4 team_colorC : source_color;
uniform vec4 skin_color : source_color;
uniform vec4 shorts : source_color;
uniform vec4 socks : source_color;
uniform bool unavailable = false;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    if (tex.a < 0.1) {
        discard;
    }

    vec4 col = tex;
    
    // #C7DCD0 (Main Jersey)
    vec3 jersey_ref = vec3(0.78, 0.86, 0.81);
    
    // Pure White (Second Jersey Color)
    vec3 jersey_b_ref = vec3(1.0, 1.0, 1.0);
	vec3 jersey_c_ref = vec3(0.24, 0.21, 0.27);
	
	vec3 shorts_ref = vec3(0.5686, 0.8588, 0.4118);
	
	vec3 shorts_refB = vec3(0.1176, 0.7373, 0.4510);
	
	vec3 socks_ref = vec3(0.9843, 1, 0.5255);
    
    float tolerance = 0.05;

    // 1. DETECT JERSEY A (#C7DCD0)
    if (distance(tex.rgb, jersey_ref) < tolerance) {
        col = vec4(team_color.rgb, tex.a);
    }
    // 2. DETECT JERSEY B (White)
    else if (distance(tex.rgb, jersey_b_ref) < tolerance) {
        col = vec4(team_colorB.rgb, tex.a);
    }
    else if (distance(tex.rgb, jersey_c_ref) < tolerance) {
        col = vec4(team_colorC.rgb, tex.a);
    }
    else if (distance(tex.rgb, shorts_ref) < tolerance) {
        col = vec4(shorts.rgb, tex.a);
    }
    else if (distance(tex.rgb, shorts_refB) < tolerance) {
        col = vec4(shorts.rgb, tex.a);
    }
    else if (distance(tex.rgb, socks_ref) < tolerance) {
        col = vec4(socks.rgb, tex.a);
    }
    // 3. DETECT SKIN
    else if (tex.r > 0.7 && tex.g > 0.4 && tex.b > 0.3 && tex.r > tex.b) {
        col = vec4(skin_color.rgb, tex.a);
    }

    // 4. FINAL OUTPUT & GREYSCALE
    if (unavailable) {
        float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
        COLOR = vec4(vec3(gray), col.a);
    } else {
        COLOR = col;
    }
}