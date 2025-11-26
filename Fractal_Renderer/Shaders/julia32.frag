#version 460 core
#define pi 3.1415

in vec3 vPos; 
out vec4 FragColor;

uniform float minx;
uniform float maxx;
uniform float miny;
uniform float maxy;

uniform float MAX_ITERATIONS;
uniform float N_POWER;
uniform float rotation_angle;

uniform vec3 palette[10];
uniform int palette_size;

uniform vec2 beginRect;     
uniform vec2 endRect;       
uniform vec2 mousePos;
uniform float drawRectangle; // 1.0 if rectangle should be drawn, 0.0 otherwise
uniform float u_borderWidth;


float iterateJulia_naive(vec2 coord) {
    vec2 z = coord;
    vec2 c = mousePos;

    float tempZ = z.x;
    float count = 0.0;

    do {
        tempZ = z.x * z.x - z.y * z.y + c.x;
        z.y = 2.0 * z.x * z.y + c.y;
        z.x = tempZ;

        count += 1.0;
    } while ((z.x*z.x + z.y*z.y) <= 4 && count < MAX_ITERATIONS);

    return count;
}

float iterateJulia_optimized(vec2 p0){
    vec2 p = p0;
    vec2 p2 = p * p;
    
    float count = 0.0;

    do {
        p.y = ((p.x + p.x) * p.y) + mousePos.y;
        p.x = p2.x - p2.y + mousePos.x;
        p2.x = p.x * p.x;
        p2.y = p.y * p.y;

        count += 1.0;
    } while ((p2.x + p2.y) <= 4 && count < MAX_ITERATIONS);

    return count;
}

vec4 drawSelection() {

    if (drawRectangle > 0.5) { 

        vec2 rectMin = min(beginRect, endRect);
        vec2 rectMax = max(beginRect, endRect);

        bool isInsideRect = vPos.x >= rectMin.x && vPos.x <= rectMax.x &&
                            vPos.y >= rectMin.y && vPos.y <= rectMax.y;


        bool onBorder = false;
        if (isInsideRect) { 

            bool nearLeft = abs(vPos.x - rectMin.x) < u_borderWidth;
            bool nearRight = abs(vPos.x - rectMax.x) < u_borderWidth;
            bool nearBottom = abs(vPos.y - rectMin.y) < u_borderWidth;
            bool nearTop = abs(vPos.y - rectMax.y) < u_borderWidth;

            onBorder = (nearLeft || nearRight || nearBottom || nearTop);
        }

        if (onBorder) {
            return vec4(1.0, 0.0, 0.0, 1.0); // Border color
        } else {
            return vec4(0.0); // Fractal color behind border 
        }

    } else {
        return vec4(0.0); // No selection
    }

}

vec4 colorFractal(float count) {
    if (count == MAX_ITERATIONS) {
        return vec4(0.0, 0.0, 0.0, 1.0); // Inside the Mandelbrot 
    } else {
        // Map the count to palette space

        if (palette_size <= 0) {
            return vec4(1.0, 0.0, 1.0, 1.0); // Magenta = error indicator
        }

        float palettePos = count * float(palette_size - 1) / MAX_ITERATIONS;
        
        // Get the two adjacent palette indices (with wrapping)
        int idx1 = int(floor(palettePos)) % palette_size;
        int idx2 = (idx1 + 1) % palette_size;
        
        // Get the fractional part for smooth interpolation
        float t = fract(palettePos);
        
        vec3 color1 = palette[idx1];
        vec3 color2 = palette[idx2];
        vec3 color = mix(color1, color2, t);

        return vec4(color, 1.0);
    }
}

float convertToRad(float degrees) {
    return degrees * (pi / 180.0);
}

vec2 applyRotation(vec2 point, vec2 center, float angleDegrees) {

    vec2 r;
    float angleRad = convertToRad(angleDegrees);

    vec2 t = point - center;
    r.x = t.x * cos(angleRad) - t.y * sin(angleRad);
    r.y = t.x * sin(angleRad) + t.y * cos(angleRad);
    r += center;

    return r;
}

void main(){

    float x_interp = (vPos.x + 1.0) / 2.0; 
    float y_interp = (vPos.y + 1.0) / 2.0; 

    float x_coord  = mix(minx, maxx, x_interp);
    float y_coord  = mix(miny, maxy, y_interp);

    vec2 coord = vec2(x_coord, y_coord);
    vec2 u_center = vec2((minx + maxx) / 2.0, (miny + maxy) / 2.0);
    
    coord = applyRotation(coord, u_center, rotation_angle);

    float i = iterateJulia_optimized(coord);
    vec4 color;

    color = colorFractal(i);
    color += drawSelection();
    
    
    FragColor = color;
}