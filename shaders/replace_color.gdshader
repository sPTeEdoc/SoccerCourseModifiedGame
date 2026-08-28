shader_type canvas_item;

uniform vec4 jersey_color : source_color;
uniform vec4 shorts : source_color;
uniform vec4 socks : source_color;
uniform vec4 keeper_jersey : source_color;
uniform vec4 keeper_shorts : source_color;
uniform vec4 keeper_socks : source_color;
uniform vec4 hair_color : source_color;
uniform vec4 skin_color : source_color;
uniform bool unavailable = false;
uniform bool is_goalkeeper = false; // Add this!

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    if (tex.a < 0.1) {
        discard;
    }

    vec4 col = tex;

    // Baseline reference colors on your sprite sheet
    vec3 jersey_ref = vec3(0.7098, 0.902, 0.114);
    vec3 shorts_ref = vec3(0.0588, 0.3882, 0.702);
    vec3 socks_ref = vec3(0.9843, 1, 0.5255);
    vec3 hair_color_ref = vec3(0.9294, 0.502, 0.6);

    float tolerance = 0.05;

    if (distance(tex.rgb, jersey_ref) < tolerance) {
        col = is_goalkeeper ? vec4(keeper_jersey.rgb, tex.a) : vec4(jersey_color.rgb, tex.a);
    }
    else if (distance(tex.rgb, shorts_ref) < tolerance) {
        col = is_goalkeeper ? vec4(keeper_shorts.rgb, tex.a) : vec4(shorts.rgb, tex.a);
    }
    else if (distance(tex.rgb, socks_ref) < tolerance) {
        col = is_goalkeeper ? vec4(keeper_socks.rgb, tex.a) : vec4(socks.rgb, tex.a);
    }
    else if (distance(tex.rgb, hair_color_ref) < tolerance) {
        col = vec4(hair_color.rgb, tex.a);
    }
    else if (tex.r > 0.7 && tex.g > 0.4 && tex.b > 0.3 && tex.r > tex.b) {
        col = vec4(skin_color.rgb, tex.a);
    }

    if (unavailable) {
        float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
        COLOR = vec4(vec3(gray), col.a);
    } else {
        COLOR = col;
    }
}