//https://iquilezles.org/articles/distfunctions2d/

float sdCircle(float2 p, float r)
{
    return length(p) - r;
}

//float sdRoundedBox(in float2 p, in float2 b, in float4 r)
//{
//    r.xy = (p.x > 0.0) ? r.xy : r.zw;
//    r.x = (p.y > 0.0) ? r.x : r.y;
//    float2 q = abs(p) - b + r.x;
//    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
//}

//float sdChamferBox(in float2 p, in float2 b, in float chamfer)
//{
//    p = abs(p) - b;

//    p = (p.y > p.x) ? p.yx : p.xy;
//    p.y += chamfer;
    
//    const float k = 1.0 - sqrt(2.0);
//    if (p.y < 0.0 && p.y + p.x * k < 0.0)
//        return p.x;
    
//    if (p.x < p.y)
//        return (p.x + p.y) * sqrt(0.5);
    
//    return length(p);
//}

//float sdBox(in float2 p, in float2 b)
//{
//    float2 d = abs(p) - b;
//    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
//}

//float sdOrientedBox(in float2 p, in float2 a, in float2 b, float th)
//{
//    float l = length(b - a);
//    float2 d = (b - a) / l;
//    float2 q = (p - (a + b) * 0.5);
//    q = mat2(d.x, -d.y, d.y, d.x) * q;
//    q = abs(q) - float2(l, th) * 0.5;
//    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
//}

//float sdSegment(in float2 p, in float2 a, in float2 b)
//{
//    float2 pa = p - a, ba = b - a;
//    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
//    return length(pa - ba * h);
//}

//float sdRhombus(in float2 p, in float2 b)
//{
//    b.y = -b.y;
//    p = abs(p);
//    float h = clamp((dot(b, p) + b.y * b.y) / dot(b, b), 0.0, 1.0);
//    p -= b * float2(h, h - 1.0);
//    return length(p) * sign(p.x);
//}

//float sdTrapezoid(in float2 p, in float r1, float r2, float he)
//{
//    float2 k1 = float2(r2, he);
//    float2 k2 = float2(r2 - r1, 2.0 * he);
//    p.x = abs(p.x);
//    float2 ca = float2(p.x - min(p.x, (p.y < 0.0) ? r1 : r2), abs(p.y) - he);
//    float2 cb = p - k1 + k2 * clamp(dot(k1 - p, k2) / dot2(k2), 0.0, 1.0);
//    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
//    return s * sqrt(min(dot2(ca), dot2(cb)));
//}

//float sdParallelogram(in float2 p, float wi, float he, float sk)
//{
//    float2 e = float2(sk, he);
//    p = (p.y < 0.0) ? -p : p;
//    float2 w = p - e;
//    w.x -= clamp(w.x, -wi, wi);
//    float2 d = float2(dot(w, w), -w.y);
//    float s = p.x * e.y - p.y * e.x;
//    p = (s < 0.0) ? -p : p;
//    float2 v = p - float2(wi, 0);
//    v -= e * clamp(dot(v, e) / dot(e, e), -1.0, 1.0);
//    d = min(d, float2(dot(v, v), wi * he - abs(s)));
//    return sqrt(d.x) * sign(-d.y);
//}

//float sdEquilateralTriangle(in float2 p, in float r)
//{
//    const float k = sqrt(3.0);
//    p.x = abs(p.x) - r;
//    p.y = p.y + r / k;
//    if (p.x + k * p.y > 0.0)
//        p = float2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
//    p.x -= clamp(p.x, -2.0 * r, 0.0);
//    return -length(p) * sign(p.y);
//}

//float sdTriangle(in float2 p, in float2 p0, in float2 p1, in float2 p2)
//{
//    float2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
//    float2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
//    float2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
//    float2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
//    float2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
//    float s = sign(e0.x * e2.y - e0.y * e2.x);
//    float2 d = min(min(float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
//                     float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
//                     float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
//    return -sqrt(d.x) * sign(d.y);
//}

//float sdUnevenCapsule(float2 p, float r1, float r2, float h)
//{
//    p.x = abs(p.x);
//    float b = (r1 - r2) / h;
//    float a = sqrt(1.0 - b * b);
//    float k = dot(p, float2(-b, a));
//    if (k < 0.0)
//        return length(p) - r1;
//    if (k > a * h)
//        return length(p - float2(0.0, h)) - r2;
//    return dot(p, float2(a, b)) - r1;
//}

//float sdPentagon(in float2 p, in float r)
//{
//    const vec3 k = vec3(0.809016994, 0.587785252, 0.726542528);
//    p.x = abs(p.x);
//    p -= 2.0 * min(dot(float2(-k.x, k.y), p), 0.0) * float2(-k.x, k.y);
//    p -= 2.0 * min(dot(float2(k.x, k.y), p), 0.0) * float2(k.x, k.y);
//    p -= float2(clamp(p.x, -r * k.z, r * k.z), r);
//    return length(p) * sign(p.y);
//}

//float sdEllipse(in float2 p, in float2 ab)
//{
//    p = abs(p);
//    if (p.x > p.y)
//    {
//        p = p.yx;
//        ab = ab.yx;
//    }
//    float l = ab.y * ab.y - ab.x * ab.x;
//    float m = ab.x * p.x / l;
//    float m2 = m * m;
//    float n = ab.y * p.y / l;
//    float n2 = n * n;
//    float c = (m2 + n2 - 1.0) / 3.0;
//    float c3 = c * c * c;
//    float q = c3 + m2 * n2 * 2.0;
//    float d = c3 + m2 * n2;
//    float g = m + m * n2;
//    float co;
//    if (d < 0.0)
//    {
//        float h = acos(q / c3) / 3.0;
//        float s = cos(h);
//        float t = sin(h) * sqrt(3.0);
//        float rx = sqrt(-c * (s + t + 2.0) + m2);
//        float ry = sqrt(-c * (s - t + 2.0) + m2);
//        co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
//    }
//    else
//    {
//        float h = 2.0 * m * n * sqrt(d);
//        float s = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
//        float u = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
//        float rx = -s - u - c * 4.0 + 2.0 * m2;
//        float ry = (s - u) * sqrt(3.0);
//        float rm = sqrt(rx * rx + ry * ry);
//        co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
//    }
//    float2 r = ab * float2(co, sqrt(1.0 - co * co));
//    return length(r - p) * sign(p.y - r.y);
//}

//float opRound(in float2 p, in float r)
//{
//    return sdShape(p) - r;
//}
