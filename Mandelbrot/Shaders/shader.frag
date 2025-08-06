#version 330 core

in vec3 vPos; 
out vec4 FragColor;

uniform float minx;
uniform float maxx;
uniform float miny;
uniform float maxy;

uniform float MAX_ITERATIONS;
uniform float N_POWER;

uniform vec2 beginRect;     
uniform vec2 endRect;       
uniform float drawRectangle; // 1.0 if rectangle should be drawn, 0.0 otherwise
uniform float u_borderWidth;



float iterateMandelbrot(vec2 coord) {
    vec2 z = coord;
    vec2 c = coord;
    
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
    }else {

        float normalizedValue = count / MAX_ITERATIONS;

        // Color palette
        vec4 color1 = vec4(0.9, 0.9, 0.9, 1.0); // Dark blue
        vec4 color2 = vec4(0.0, 0.5, 1.0, 1.0); // Bright blue
        vec4 color3 = vec4(0.9, 0.9, 0.2, 1.0); // Yellow
        vec4 color4 = vec4(0.0, 0.2, 0.1, 1.0); // Red
        vec4 color5 = vec4(3.0, 0.5, 0.3, 1.0); // Dark Red


        // Interpolate between colors 
        if (normalizedValue < 0.25) {
            return mix(color1, color2, normalizedValue * 4.0);
        } else if (normalizedValue < 0.5) {
            return mix(color2, color3, (normalizedValue - 0.25) * 4.0);
        } else if (normalizedValue < 0.75) {
            return mix(color3, color4, (normalizedValue - 0.5) * 4.0);
        } else {
            return mix(color4, color5, (normalizedValue - 0.75) * 4.0);
        }
    } 
}

void main(){

    float x_interp = (vPos.x + 1.0) / 2.0; 
    float y_interp = (vPos.y + 1.0) / 2.0; 

    float x_coord  = mix(minx, maxx, x_interp);
    float y_coord  = mix(miny, maxy, y_interp);

    vec2 coord = vec2(x_coord, y_coord);

    float i = iterateMandelbrot(coord);
    vec4 color;

    color = colorFractal(i);
    color += drawSelection();
    
    
    FragColor = color;
}