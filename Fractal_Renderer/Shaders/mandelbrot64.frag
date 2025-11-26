#version 460 core
#define pi 3.1415926535897932384626433832795

in vec3 vPos; 
out vec4 FragColor;

uniform double minx;
uniform double maxx;
uniform double miny;
uniform double maxy;

uniform float MAX_ITERATIONS;
uniform float N_POWER;
uniform float rotation_angle;

uniform vec3 palette[10];
uniform int palette_size;

uniform vec2 beginRect;     
uniform vec2 endRect;       
uniform float drawRectangle; // 1.0 if rectangle should be drawn, 0.0 otherwise
uniform float u_borderWidth;



float iterateMandelbrot_naive(dvec2 coord) {
    dvec2 z = dvec2(0.0);
    dvec2 c = coord;
    
    double tempZ = z.x;
    float count = 0.0;

    do {
        tempZ = z.x * z.x - z.y * z.y + c.x;
        z.y = 2.0 * z.x * z.y + c.y;
        z.x = tempZ;

        count += 1.0;
    } while ((z.x*z.x + z.y*z.y) <= 4 && count < MAX_ITERATIONS);

    return count;
}

float iterateMandelbrot_optimized(dvec2 p0) {
    dvec2 p = dvec2(0.0);
    dvec2 p2 = dvec2(0.0);
    
    float count = 0.0;

    do {
        p.y = ((p.x + p.x) * p.y) + p0.y;
        p.x = p2.x - p2.y + p0.x;
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

dvec2 applyRotation(dvec2 point, dvec2 center, float angleDegrees) {

    dvec2 r;
    float angleRad = convertToRad(angleDegrees);

    dvec2 t = point - center;
    r.x = t.x * double(cos(angleRad)) - t.y * double(sin(angleRad));
    r.y = t.x * double(sin(angleRad)) + t.y * double(cos(angleRad));
    r += center;

    return r;
}

void main(){

    double x_interp = (vPos.x + 1.0) / 2.0; 
    double y_interp = (vPos.y + 1.0) / 2.0; 

    double x_coord  = mix(minx, maxx, x_interp);
    double y_coord  = mix(miny, maxy, y_interp);

    dvec2 coord = dvec2(x_coord, y_coord);
    dvec2 u_center = dvec2((minx + maxx) / 2.0, (miny + maxy) / 2.0);

    coord = applyRotation(coord, u_center, rotation_angle); 

    float i = iterateMandelbrot_optimized(coord);
    vec4 color;

    color = colorFractal(i);
    color += drawSelection();
    
    
    FragColor = color;
}