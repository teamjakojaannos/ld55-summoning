shader_type canvas_item;

uniform float bright_dist = 75.0;
uniform float darkness_dist_factor = 0.33;

void fragment() {
	vec2 screen_size = vec2(
		1.0 / SCREEN_PIXEL_SIZE.x,
		1.0 / SCREEN_PIXEL_SIZE.y
	);

	float dist = distance(vec4(0.5 * screen_size.x, 0.425 * screen_size.y, 0.0, 0.0), FRAGCOORD);
	dist = dist * dist;
	
	float timescale = 30.0;
	float wobble = sin(sin(TIME * timescale) + sin(TIME) * sin(TIME * 0.123412));
	
	float timescale2 = 0.3;
	float wobble2 = sin(sin(TIME * timescale2) + sin(TIME) * sin(TIME * 0.2213));

	float darkness_dist = darkness_dist_factor * screen_size.y;
	float wobble_dist = darkness_dist_factor * 1.0 * ((wobble + 1.0) / 2.0) + darkness_dist_factor * 20.0 * ((wobble2 + 1.0) / 2.0);
	float dd = darkness_dist + wobble_dist;
	float darkness_dist_sq = dd * dd;
	float d = 1.0 - clamp(dist / darkness_dist_sq, 0.0, 1.0);

	float n_steps = 7.0;
	d = ceil(d * n_steps) / n_steps;
	
	float bayer_matrix_4x4[16] = {
	       -0.5,     0.0,  -0.375,   0.125,
	       0.25,   -0.25,   0.375,  -0.125,
	    -0.3125,  0.1875, -0.4375,  0.0625,
	     0.4375, -0.0625,  0.3125, -0.1875
	};

	float coord_scale = 2.0;
	float rx = FRAGCOORD.x / coord_scale;
	float ry = FRAGCOORD.y / coord_scale;

	int bayer_idx = ((int(rx) % 4) * 4) + int(ry) % 4;
	float bayer_value = bayer_matrix_4x4[bayer_idx];
	
	float bright_dist_sq = bright_dist * bright_dist;
	float bd = dist < bright_dist_sq ? 1.0 : d + (d * bayer_value);
	COLOR = vec4(0.0, 0.0, 0.0, 1.0 - bd);
	
	// HACK: Some tiles are accidentally messy, clean up the alpha channel
	COLOR.a = COLOR.a > 0.5 ? 1.0 : 0.0;
}